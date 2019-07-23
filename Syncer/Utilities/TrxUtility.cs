namespace Syncer.Utilities
{
    using Newtonsoft.Json.Linq;

    using Serilog;

    using Syncer.Entities;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public static class TrxUtility
    {
        private static IEnumerable<string> TestSuiteIds;
        private static bool Consideration;
        private static TestRun tr;
        private static List<TestResult> TestResults;
        private static List<IGrouping<string, TestCase>> TestCasesByPlanId;
        private static List<IGrouping<string, TestResult>> TestResultsByClass;
        private static List<string> NewTestRunIds = new List<string>();
        private static List<TestCase> TestCases = new List<TestCase>();
        private static List<string> AutomatedTestNames = new List<string>();
        private static Dictionary<string, List<string>> TestPointIds;
        private static readonly ConcurrentDictionary<string, List<TestResult>> TestsPerAssembly = new ConcurrentDictionary<string, List<TestResult>>();

        public static int UpdateTestResults(string filePath, string account, string project, string token, IEnumerable<string> testSuiteIds, bool consideration)
        {
            TestSuiteIds = testSuiteIds;
            Consideration = consideration;
            AzureDevOpsUtility.UpdateAccountDetails(account, project, token);
            var files = new List<string>();
            if (!filePath.EndsWith("trx"))
            {
                files = Directory.GetFiles(filePath, "*.trx").ToList();
            }
            else
            {
                files.Add(filePath);
            }

            if (files.Count() == 0)
            {
                throw new FileNotFoundException($"Cannot find TRX file under {filePath}. Please provide valid TRX file path.");
            }

            ParseTestRun(files[0]);
            GetAutomatedTestNames();
            GetTestCasesByPlanAsync().GetAwaiter().GetResult();
            GetTestPointIdsAsync().GetAwaiter().GetResult();
            CreateNewTestRunsAsync().GetAwaiter().GetResult();
            UpdateTestResultsAsync().GetAwaiter().GetResult();
            UploadTrxFileAsTestRunAttachment(files[0]).GetAwaiter().GetResult();
            UpdateTestRunsAsync().GetAwaiter().GetResult();
            //DeleteTestRunAsync().GetAwaiter().GetResult();
            return 0;
        }

        private static void ParseTestRun(string file)
        {
            Console.WriteLine();
            Log.Information($"Parsing: {file}");
            Console.WriteLine();
            var testAssemblyPathOverride = string.Empty;
            var ser = new XmlSerializer(typeof(TestRun));
            using (var stream = File.Open(file, FileMode.Open))
            {
                tr = (TestRun)ser.Deserialize(stream);
                var defNotFound = 1;
                TestResults = tr.TestEntries.Select(e =>
                {
                    var result = tr.Results.SingleOrDefault(x => e.testId.Equals(x.testId) && tr.TestDefinitions.Any(y => x.executionId.Equals(y.Execution.id)));
                    var def = tr.TestDefinitions.SingleOrDefault(x => e.executionId.Equals(x.Execution.id));
                    var assembly = string.IsNullOrWhiteSpace(testAssemblyPathOverride) ? def?.TestMethod?.codeBase : Path.Combine(testAssemblyPathOverride, Path.GetFileName(def?.TestMethod?.codeBase));
                    var item = new TestResult(assembly, def?.TestMethod?.className, result.testName, result.outcome, result.Output?.ErrorInfo?.Message, result.Output?.ErrorInfo?.StackTrace, result.duration.TimeOfDay, result.computerName);
                    if (def == null)
                    {
                        Log.Warning($"{defNotFound++}. TestDefinition not found for: {result.testName} (testId={result.testId}, executionId={result.executionId})");
                        item = null;
                    }

                    return item;
                }).Where(x => x != null).ToList();

                TestResultsByClass = TestResults.GroupBy(x => x.ClassName).OrderBy(x => x.Key).ToList();
            }
        }

        private static void GetAutomatedTestNames()
        {
            tr.TestDefinitions.ToList().ForEach(x =>
            {
                AutomatedTestNames.Add($"{x.TestMethod.className}.{x.TestMethod.name.Split('(')[0]}");
            });
        }

        private static async Task GetTestCasesByPlanAsync()
        {
            var allWorkItems = new Dictionary<string, JObject>();
            foreach (var item in AutomatedTestNames.Distinct())
            {
                allWorkItems.Add(item, await AzureDevOpsUtility.GetWorkItemsWithAutomatedTestNameAsAsync(item).ConfigureAwait(false));
            }

            foreach (var key in allWorkItems.Keys)
            {
                var result = allWorkItems[key];
                if (result.SelectToken("workItems").Count() != 0)
                {
                    var workItems = result.SelectToken("workItems");
                    foreach (var workItem in workItems)
                    {
                        TestCases.AddRange(await CommonUtility.GetCasesAsync(workItem, TestSuiteIds, Consideration).ConfigureAwait(false));
                    }
                }
                else
                {
                    Log.Information($"No Test-case is found with Automated Test Name as: {key}");
                }
            }

            TestCasesByPlanId = TestCases.GroupBy(y => y.TestPlanId).OrderBy(z => z.Key).ToList();
        }

        private static async Task GetTestPointIdsAsync()
        {
            var testCasesIds = TestCases.Select(x => x.TestCaseId).Distinct();
            if (testCasesIds.Count() > 0)
            {
                var result = await AzureDevOpsUtility.GetTestPointsByTestCaseIdsAsync(testCasesIds).ConfigureAwait(false);
                TestPointIds = CommonUtility.GetTestPointsThatNeedsToBePartOfNewTestRunAsync(TestCases, result, TestSuiteIds, Consideration);
            }
        }

        private static async Task CreateNewTestRunsAsync()
        {
            for (var i = 0; i < TestCasesByPlanId.Count; i++)
            {
                var testPlanId = TestCasesByPlanId[i].Select(x => int.Parse(x.TestPlanId)).FirstOrDefault();
                NewTestRunIds.Add((await AzureDevOpsUtility.CreateNewTestRunAsync(tr, testPlanId, TestPointIds[testPlanId.ToString()].ToArray()).ConfigureAwait(false)).SelectToken("id").ToString());
            }
        }

        private static async Task UpdateTestResultsAsync()
        {
            foreach (var testRunId in NewTestRunIds)
            {
                var testresults = await AzureDevOpsUtility.GetTestResultsOfATestRunAsync(testRunId).ConfigureAwait(false);
                var resultArray = new List<object>();
                foreach (var testresult in testresults.SelectToken("value").ToList())
                {
                    var testResultsFromTrx = TestResults.Where(x => (x.ClassName + "." + x.TestName.Split('(')[0]).Equals(testresult.SelectToken("automatedTestName").ToString()));
                    var testResult = testResultsFromTrx.Any(x => x.Outcome.Equals("Failed")) ? testResultsFromTrx.FirstOrDefault(x => x.Outcome.Equals("Failed")) : testResultsFromTrx.All(x => x.Outcome.Equals("NotExecuted")) ? testResultsFromTrx.FirstOrDefault() : testResultsFromTrx.FirstOrDefault(x => x.Outcome.Equals("Passed"));
                    var (errorMessage, trace) = GetErrorMessageAndStackTrace(testResult, testResultsFromTrx);
                    resultArray.Add(new { id = testresult.SelectToken("id"), state = "Completed", durationInMs = testResultsFromTrx.Sum(x => x.Duration.Value.Milliseconds), outcome = testResult.Outcome, computerName = testResult.ComputerName, errorMessage, stackTrace = trace });
                }

                await AzureDevOpsUtility.UpdateTestResultsOfATestRunAsync(testRunId, resultArray).ConfigureAwait(false);
            }
        }

        private static (string errorMessage, string trace) GetErrorMessageAndStackTrace(TestResult testResult, IEnumerable<TestResult> testResultsFromTrx)
        {
            string errorMessage = string.Empty;
            string trace = string.Empty;
            if (testResult.Outcome.Equals("Failed"))
            {
                var allTestResultErrorMessages = testResultsFromTrx.Where(x => x.Outcome.Equals("Failed")).Select(y => new
                {
                    ErrorMessage = $"Test Name: {y.TestName}\nErrorMessage: {y.Error}",
                    Trace = $"Test Name: {y.TestName}\nTrace: {y.Trace}"
                });

                errorMessage = string.Join("\n\n", allTestResultErrorMessages.Select(x => x.ErrorMessage));
                trace = string.Join("\n\n", allTestResultErrorMessages.Select(x => x.Trace));
            }
            else
            {
                var allTestResultErrorMessages = testResultsFromTrx.Where(x => x.Outcome.Equals("NotExecuted")).Select(y => new
                {
                    ErrorMessage = $"Test Name: {y.TestName}\nErrorMessage: {y.Error}"
                });

                errorMessage = string.Join("\n\n", allTestResultErrorMessages.Select(x => x.ErrorMessage));
            }

            return (errorMessage, trace);
        }

        private static async Task UploadTrxFileAsTestRunAttachment(string file)
        {
            foreach (var testRunId in NewTestRunIds)
            {
                await AzureDevOpsUtility.AddAttachmentToTestRunAsync(testRunId, file, NewTestRunIds).ConfigureAwait(false);
            }
        }

        private static async Task UpdateTestRunsAsync()
        {
            foreach (var testRunId in NewTestRunIds)
            {
                await AzureDevOpsUtility.UpdateTestRunAsync(tr, testRunId).ConfigureAwait(false);
            }
        }

        private static async Task DeleteTestRunAsync()
        {
            if (NewTestRunIds.Count() > 0)
            {
                foreach (var testRunId in NewTestRunIds)
                {
                    await AzureDevOpsUtility.DeleteTestRunAsync(testRunId).ConfigureAwait(false);
                }
            }
        }
    }
}
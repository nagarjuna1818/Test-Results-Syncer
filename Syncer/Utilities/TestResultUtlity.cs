namespace Syncer.Utilities
{
    using Flurl.Http;

    using Ganss.Excel;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Serilog;

    using Syncer.Entities;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Utility to update test results from json file.
    /// </summary>
    public class TestResultUtlity
    {
        private static TestRun Tr;
        private static TestResults TestResults;
        private static List<IGrouping<string, TestCase>> TestCasesByPlanId;
        private static readonly List<string> NewTestRunIds = new List<string>();
        private static readonly List<TestCase> TestCases = new List<TestCase>();
        private static readonly Dictionary<string, List<string>> TestPointIds = new Dictionary<string, List<string>>();

        /// <summary>
        /// Update Test Results.
        /// </summary>
        /// <param name="filePath">file path.</param>
        /// <param name="isJson">Is input file type Json?</param>
        /// <param name="account">Account.</param>
        /// <param name="project">Project.</param>
        /// <param name="token">Token.</param>
        /// <returns>Integer.</returns>
        public static int UpdateTestResults(string filePath, bool isJson, string account, string project, string token)
        {
            AzureDevOpsUtility.UpdateAccountDetails(account, project, token);
            if (isJson)
            {
                var json = File.ReadAllText(filePath);
                TestResults = JsonConvert.DeserializeObject<TestResults>(json);
            }
            else
            {
                var excel = new ExcelMapper(filePath);
                excel.AddMapping<TestCaseOutcome>("TestCaseId", t => t.TestCaseId);
                excel.AddMapping<TestCaseOutcome>("TestSuiteId", t => t.TestSuiteId);
                excel.AddMapping<TestCaseOutcome>("Outcome", t => t.Outcome)
                    .SetPropertyUsing(v =>
                    {
                        if ((v as string).Equals(Constants.NotExecuted))
                            return OutcomeType.NotExecuted;
                        if ((v as string).Equals(Constants.Failed))
                            return OutcomeType.Failed;
                        else
                            return OutcomeType.Passed;
                    });

                var testCases = excel.Fetch<TestCaseOutcome>().ToList();
                TestResults = new TestResults()
                {
                    SuiteId = 0,
                    TestCases = testCases
                };
            }

            try
            {
                GetTestCasesByPlanAsync().GetAwaiter().GetResult();
                GetTestPointIdsAsync().GetAwaiter().GetResult();
                CreateNewTestRunsAsync().GetAwaiter().GetResult();
                UpdateTestRunsAsync().GetAwaiter().GetResult();
            }
            catch (FlurlHttpException e)
            {
                var statusCode = e.Call.Response.StatusCode.ToString();
                if (statusCode.Equals(Constants.NonAuthoritativeInformation) || statusCode.Equals(Constants.Unauthorized))
                {
                    Log.Error("Authentication Error!!! Please provide valid Account Details...\n");
                }
            }
            catch (Exception e)
            {
                CommonUtility.DeleteTestRunsAsync(NewTestRunIds).GetAwaiter().GetResult();
                throw e;
            }
            
            return 0;
        }

        /// <summary>
        /// Get TestCases to be updated.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task GetTestCasesByPlanAsync()
        {
            foreach (var testCase in TestResults.TestCases)
            {
                var testCaseId = testCase.TestCaseId.ToString();
                var tsId = testCase.TestSuiteId.ToString();
                JObject testSuites;
                try
                {
                    testSuites = await AzureDevOpsUtility.GetTestSuitesByTestCaseIdAsync(testCaseId).ConfigureAwait(false);
                }
                catch (FlurlHttpException e)
                {
                    var statusCode = e.Call.Response.StatusCode.ToString();
                    if (statusCode.Equals(Constants.NotFound))
                    {
                        Log.Information($"No Test-case is found with Id - {testCaseId}");
                        continue;
                    }
                    else
                    {
                        throw e;
                    }
                }

                var testSuitesValues = testSuites.SelectToken("value").ToList();
                if (testSuitesValues.Any())
                {
                    if (testCase.TestSuiteId != 0)
                    {
                        var result = testSuitesValues.Any(x => x.SelectToken("id").ToString().Equals(tsId));
                        if (!result)
                        {
                            Log.Information($"Test-Case Id: {testCase.TestCaseId} is not present in Test-Suite: {tsId}");
                            continue;
                        }
                        else
                        {
                            var testPlanId = testSuitesValues.FirstOrDefault(x => x.SelectToken("id").ToString().Equals(tsId)).SelectToken("plan.id").ToString();
                            TestCases.Add(new TestCase(testCaseId, tsId, testPlanId));
                        }
                    }
                    else if (TestResults.SuiteId != 0)
                    {
                        var suiteId = TestResults.SuiteId.ToString();
                        var result = testSuitesValues.Any(x => x.SelectToken("id").ToString().Equals(suiteId));
                        if (!result)
                        {
                            Log.Information($"Test-Case Id: {testCase.TestCaseId} is not present in Test-Suite: {suiteId}");
                            continue;
                        }
                        else
                        {
                            var testPlanId = testSuitesValues.FirstOrDefault(x => x.SelectToken("id").ToString().Equals(suiteId)).SelectToken("plan.id").ToString();
                            TestCases.Add(new TestCase(testCaseId, suiteId, testPlanId));
                        }
                    }
                    else
                    {
                        foreach (var testSuite in testSuitesValues)
                        {
                            var testSuiteId = testSuite.SelectToken("id").ToString();
                            var testPlanId = testSuite.SelectToken("plan.id").ToString();
                            TestCases.Add(new TestCase(testCaseId, testSuiteId, testPlanId));
                        }
                    }
                }
            }

            TestCasesByPlanId = TestCases.GroupBy(y => y.TestPlanId).OrderBy(z => z.Key).ToList();
        }

        /// <summary>
        /// Get Test Point Ids.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task GetTestPointIdsAsync()
        {
            var testCasesIds = TestCases.Select(x => x.TestCaseId).Distinct();
            if (testCasesIds.Count() > 0)
            {
                var result = await AzureDevOpsUtility.GetTestPointsByTestCaseIdsAsync(testCasesIds).ConfigureAwait(false);
                var points = result.SelectToken("points");
                TestCases.Select(x => x.TestPlanId).Distinct().ToList().ForEach(y =>
                {
                    var testCasesInThisPlan = TestCases.Where(l => l.TestPlanId.Equals(y)).GroupBy(m => m.TestCaseId).OrderBy(n => n.Key).ToList();
                    var tps = new List<string>();
                    foreach (var item in testCasesInThisPlan)
                    {
                        var testSuitesFromTestPointsOfThisTestCase = points.ToList().Where(r => r.SelectToken("testCase.id").ToString().Equals(item.Key) && r.SelectToken("testPlan.id").ToString().Equals(y)).ToList();
                        if (testSuitesFromTestPointsOfThisTestCase.Count > 1)
                        {
                            var tctsId = TestResults.TestCases.FirstOrDefault(z => z.TestCaseId.ToString().Equals(item.Key)).TestSuiteId;
                            var sId = TestResults.SuiteId;
                            if (tctsId != 0)
                            {
                                var point = testSuitesFromTestPointsOfThisTestCase.FirstOrDefault(z => z.SelectToken("suite.id").ToString().Equals(tctsId.ToString())).SelectToken("id").ToString();
                                tps.Add(point);
                            }
                            else if (sId != 0)
                            {
                                tps.Add(testSuitesFromTestPointsOfThisTestCase.FirstOrDefault(z => z.SelectToken("suite.id").ToString().Equals(sId.ToString())).SelectToken("id").ToString());
                            }
                            else
                            {
                                tps.AddRange(testSuitesFromTestPointsOfThisTestCase.Select(z => z.SelectToken("id").ToString()));
                            }
                        }
                        else
                        {
                            tps.Add(testSuitesFromTestPointsOfThisTestCase.FirstOrDefault().SelectToken("id").ToString());
                        }
                    }

                    TestPointIds.Add(y, tps);
                });
            }
        }

        /// <summary>
        /// Create new Test Run.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task CreateNewTestRunsAsync()
        {
            Tr = new TestRun
            {
                name = $"ManualTestRun_{DateTime.Now}",
                Times = new TestRunTimes
                {
                    start = DateTime.Now,
                    finish = DateTime.Now.AddHours(1)
                }
            };

            for (var i = 0; i < TestCasesByPlanId.Count; i++)
            {
                var testPlanId = TestCasesByPlanId[i].Select(x => int.Parse(x.TestPlanId)).FirstOrDefault();
                NewTestRunIds.Add((await AzureDevOpsUtility.CreateNewTestRunAsync(Tr, testPlanId, TestPointIds[testPlanId.ToString()].ToArray(), false).ConfigureAwait(false)).SelectToken("id").ToString());
            }
        }

        /// <summary>
        /// Update Test Runs.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task UpdateTestRunsAsync()
        {
            foreach (var testRunId in NewTestRunIds)
            {
                var testresults = await AzureDevOpsUtility.GetTestResultsOfATestRunAsync(testRunId).ConfigureAwait(false);
                var resultArray = new List<object>();
                foreach (var testresult in testresults.SelectToken("value").ToList())
                {
                    var result = TestResults.TestCases.FirstOrDefault(z => z.TestCaseId.ToString().Equals(testresult.SelectToken("testCase.id").ToString()));
                    resultArray.Add(new { id = testresult.SelectToken("id"), state = Constants.Completed, outcome = result.Outcome.ToString(), durationInMs = 1000 });
                }

                await AzureDevOpsUtility.UpdateTestResultsOfATestRunAsync(testRunId, resultArray).ConfigureAwait(false);
                await AzureDevOpsUtility.UpdateTestRunAsync(Tr, testRunId).ConfigureAwait(false);
            }
        }
    }
}
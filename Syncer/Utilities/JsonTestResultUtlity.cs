namespace Syncer.Utilities
{
    using Newtonsoft.Json;

    using Serilog;

    using Syncer.Entities;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class JsonTestResultUtlity
    {
        private static TestResults TestResults;
        private static List<TestCase> TestCases = new List<TestCase>();
        public static int UpdateResults(string filePath, string account, string project, string token)
        {
            AzureDevOpsUtility.UpdateAccountDetails(account, project, token);
            var json = File.ReadAllText(filePath);
            TestResults = JsonConvert.DeserializeObject<TestResults>(json);
            UpdateTestCasesAsync().GetAwaiter().GetResult();
            return 0;
        }

        private static async Task UpdateTestCasesAsync()
        {
            foreach (var testCase in TestResults.TestCases)
            {
                var testCaseId = testCase.TestCaseId.ToString();
                var tsId = testCase.TestSuiteId.ToString();
                var testSuites = await AzureDevOpsUtility.GetTestSuitesByTestCaseIdAsync(testCaseId).ConfigureAwait(false);
                var testSuitesValues = testSuites.SelectToken("value").ToList();
                if (testSuitesValues.Count > 1)
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

            var testCasesByPlanId = TestCases.GroupBy(y => y.TestPlanId).OrderBy(z => z.Key).ToList();
            var testCasesIds = TestCases.Select(x => x.TestCaseId).Distinct();
            var testPointIds = new Dictionary<string, List<string>>();
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

                    testPointIds.Add(y, tps);
                });
            }

            var newTestRunIds = new List<string>();
            var tr = new TestRun
            {
                name = $"ManualTestRun_{DateTime.Now}",
                Times = new TestRunTimes
                {
                    start = DateTime.Now,
                    finish = DateTime.Now.AddHours(1)
                }
            };

            for (var i = 0; i < testCasesByPlanId.Count; i++)
            {
                var testPlanId = testCasesByPlanId[i].Select(x => int.Parse(x.TestPlanId)).FirstOrDefault();
                newTestRunIds.Add((await AzureDevOpsUtility.CreateNewTestRunAsync(tr, testPlanId, testPointIds[testPlanId.ToString()].ToArray(), false).ConfigureAwait(false)).SelectToken("id").ToString());
            }

            foreach (var testRunId in newTestRunIds)
            {
                var testresults = await AzureDevOpsUtility.GetTestResultsOfATestRunAsync(testRunId).ConfigureAwait(false);
                var resultArray = new List<object>();
                foreach (var testresult in testresults.SelectToken("value").ToList())
                {
                    var result = TestResults.TestCases.FirstOrDefault(z => z.TestCaseId.ToString().Equals(testresult.SelectToken("testCase.id").ToString()));
                    resultArray.Add(new { id = testresult.SelectToken("id"), state = "Completed", outcome = result.Outcome.ToString(), durationInMs = 1000 });
                }

                await AzureDevOpsUtility.UpdateTestResultsOfATestRunAsync(testRunId, resultArray).ConfigureAwait(false);
                await AzureDevOpsUtility.UpdateTestRunAsync(tr, testRunId).ConfigureAwait(false);
            }
        }
    }
}
namespace Syncer.Utilities
{
    using Newtonsoft.Json.Linq;

    using Syncer.Entities;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class CommonUtility
    {
        public static async Task<List<TestCase>> GetCasesAsync(JToken workItem, IEnumerable<string> testSuiteIds, bool consideration)
        {
            var testCases = new List<TestCase>();
            var testCaseId = workItem.SelectToken("id").ToString();
            var testSuites = await AzureDevOpsUtility.GetTestSuitesByTestCaseIdAsync(testCaseId).ConfigureAwait(false);
            var testSuitesValues = testSuites.SelectToken("value").ToList();
            foreach (var testSuite in testSuitesValues)
            {
                var testSuiteId = testSuite.SelectToken("id").ToString();
                var testPlanId = testSuite.SelectToken("plan.id").ToString();
                if (!consideration && (testSuiteIds.Count() > 0))
                {
                    if (testSuiteIds.Contains(testSuiteId))
                    {
                        testCases.Add(new TestCase(testCaseId, testSuiteId, testPlanId));
                    }
                }
                else if (consideration && (testSuiteIds.Count() > 0))
                {
                    if (testSuitesValues.Count > 1)
                    {
                        if (testSuiteIds.Contains(testSuiteId))
                        {
                            testCases.Add(new TestCase(testCaseId, testSuiteId, testPlanId));
                        }
                    }
                    else
                    {
                        testCases.Add(new TestCase(testCaseId, testSuiteId, testPlanId));
                    }
                }
                else
                {
                    testCases.Add(new TestCase(testCaseId, testSuiteId, testPlanId));
                }
            }

            return testCases;
        }

        public static Dictionary<string, List<string>> GetTestPointsThatNeedsToBePartOfNewTestRunAsync(IEnumerable<TestCase> testCases, JObject result, IEnumerable<string> testSuiteIds, bool consideration = false)
        {
            var points = result.SelectToken("points");
            var testPointIds = new Dictionary<string, List<string>>();
            testCases.Select(x => x.TestPlanId).Distinct().ToList().ForEach(y =>
            {
                if (!consideration && (testSuiteIds.Count() > 0))
                {
                    testPointIds.Add(y, points.ToList().Where(z => z.SelectToken("testPlan.id").ToString().Equals(y) && testSuiteIds.Contains(z.SelectToken("suite.id").ToString())).Select(z => z.SelectToken("id").ToString()).ToList());
                }
                else if (consideration && (testSuiteIds.Count() > 0))
                {
                    var testCasesInThisPlan = testCases.Where(l => l.TestPlanId.Equals(y)).GroupBy(m => m.TestCaseId).OrderBy(n => n.Key).ToList();
                    var tps = new List<string>();
                    foreach (var item in testCasesInThisPlan)
                    {
                        var testSuitesFromTestPointsOfThisTestCase = points.ToList().Where(r => r.SelectToken("testCase.id").ToString().Equals(item.Key) && r.SelectToken("testPlan.id").ToString().Equals(y)).ToList();
                        if (testSuitesFromTestPointsOfThisTestCase.Count > 1)
                        {
                            testSuitesFromTestPointsOfThisTestCase.ForEach(x =>
                            {
                                var suite = x.SelectToken("suite.id").ToString();
                                if (testSuiteIds.ToList().Contains(suite))
                                {
                                    tps.Add(x.SelectToken("id").ToString());
                                }
                            });
                        }
                        else
                        {
                            tps.Add(testSuitesFromTestPointsOfThisTestCase.FirstOrDefault().SelectToken("id").ToString());
                        }
                    }

                    testPointIds.Add(y, tps);
                }
                else
                {
                    testPointIds.Add(y, points.ToList().Where(z => z.SelectToken("testPlan.id").ToString().Equals(y)).Select(z => z.SelectToken("id").ToString()).ToList());
                }
            });

            return testPointIds;
        }
    }
}
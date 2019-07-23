namespace Syncer.Utilities
{
    using Flurl.Http;
    using Flurl.Http.Content;

    using Newtonsoft.Json.Linq;

    using Syncer.Entities;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class AzureDevOpsUtility
    {
        private static string Account;
        private static string Project;
        private static string Pat;
        private const string Fields = "/fields/";
        private const string AutomatedTestName = Fields + "Microsoft.VSTS.TCM.AutomatedTestName";
        private const string AuthorizationHeader = "Authorization";
        private const string JsonPatchMediaType = "application/json-patch+json";
        private const string JsonBatchHttpRequestMediaType = "application/json";
        private const string OctetStreamMediaType = "application/octet-stream";
        private const string ApiVersion = "?api-version=5.0";
        private const string ApiVersionAsParam = "&api-version=5.0";
        private const string ApiVersionPreview1 = "?api-version=5.0-preview.1";
        private const string ApiVersionPreview2 = "?api-version=5.0-preview.2";
        private const string ContentTypeHeader = "Content-Type";
        private const string ContentSizeHeader = "Content-Size";
        private const string ContentRangeHeader = "Content-Range";
        private static string BaseUrl;
        private static string BaseProjectUrl;
        private static string TestUrl;
        private static string WitUrl;
        private static string WiqlUrl;
        private static string WorkItemByIdUrl;
        private static string TestPointsUrl;
        private static string TestRunsUrl;
        private static string TestRunUrl;
        private static string TestRunAttachmentsUrl;
        private static string TestResultsUrl;
        private static string TestPlansUrl;
        private static string TestSuitesUrl;
        private static string TestSuiteUrl;
        private static string TestCasesUrl;
        private static string TestPointUrl;
        private static string TestSuitesByTestCaseIdUrl;

        public static void UpdateAccountDetails(string account, string project, string token)
        {
            Account = account;
            Project = project;
            Pat = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
            BaseUrl = $"https://dev.azure.com/{account}/_apis/";
            BaseProjectUrl = BaseUrl.Replace("_apis", $"{project}/_apis");
            TestUrl = BaseProjectUrl + "test" + ApiVersion;
            WitUrl = BaseProjectUrl + "wit" + ApiVersion;
            WiqlUrl = WitUrl.Replace(ApiVersion, string.Empty) + "/wiql" + ApiVersion;
            WorkItemByIdUrl = WitUrl.Replace(ApiVersion, string.Empty) + "/workitems/{0}" + ApiVersion;
            TestPointsUrl = TestUrl.Replace(ApiVersion, string.Empty) + "/points" + ApiVersionPreview2;
            TestRunsUrl = TestUrl.Replace(ApiVersion, string.Empty) + "/runs" + ApiVersion;
            TestRunUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}" + ApiVersion;
            TestRunAttachmentsUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}/attachments" + ApiVersionPreview1;
            TestResultsUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}/results?api-version=5.1-preview.6";
            TestPlansUrl = TestUrl.Replace(ApiVersion, string.Empty) + "/plans" + ApiVersion;
            TestSuitesUrl = TestPlansUrl.Replace(ApiVersion, string.Empty) + "/{0}/suites" + ApiVersion;
            TestSuiteUrl = TestSuitesUrl.Replace(ApiVersion, string.Empty) + "/{1}" + ApiVersion;
            TestCasesUrl = TestSuitesUrl.Replace(ApiVersion, string.Empty) + "/{1}/testcases" + ApiVersion;
            TestPointUrl = TestSuitesUrl.Replace(ApiVersion, string.Empty) + "/{1}/points/{2}" + ApiVersion;
            TestSuitesByTestCaseIdUrl = BaseUrl + "test/suites?testCaseId={0}" + ApiVersionAsParam;
        }

        public static async Task<JObject> GetWorkItemsWithAutomatedTestNameAsAsync(string automatedTestName)
        {
            dynamic result;
            var getTestCaseWithAutomatedTestNameAs = "Select id From WorkItems Where [System.WorkItemType] = 'Test Case' AND [State] <> 'Closed' AND [State] <> 'Removed' AND [Microsoft.VSTS.TCM.AutomatedTestName]='{0}'";
            using (var content = new CapturedStringContent(new { query = string.Format(CultureInfo.InvariantCulture, getTestCaseWithAutomatedTestNameAs, automatedTestName) }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var testCase = await WiqlUrl.WithHeader(AuthorizationHeader, Pat)
                                        .PostAsync(content)
                                        .ConfigureAwait(false);
                result = testCase.Content.ReadAsJsonAsync<JObject>().Result;
            }

            return result;
        }

        public static async Task<JObject> GetTestSuitesByTestCaseIdAsync(string testCaseId)
        {
            return await string.Format(CultureInfo.InvariantCulture, TestSuitesByTestCaseIdUrl, testCaseId)
                                                    .WithHeader(AuthorizationHeader, Pat)
                                                    .GetJsonAsync<JObject>()
                                                    .ConfigureAwait(false);
        }

        public static async Task<JObject> GetTestPointsByTestCaseIdsAsync(IEnumerable<string> testCasesIds)
        {
            dynamic result;
            using (var content = new CapturedStringContent(new { PointsFilter = new { TestcaseIds = testCasesIds.ToArray() } }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var testpoints = await TestPointsUrl.WithHeader(AuthorizationHeader, Pat)
                                            .PostAsync(content)
                                            .ConfigureAwait(false);
                result = testpoints.Content.ReadAsJsonAsync<JObject>().Result;
            }

            return result;
        }

        public static async Task<JObject> CreateNewTestRunAsync(TestRun testRun, int testPlanId, string[] testPointIds, bool isAutomated = true)
        {
            dynamic result;
            using (var content = new CapturedStringContent(new { testRun.name, automated = isAutomated, plan = new { id = testPlanId }, pointIds = testPointIds, startDate = testRun.Times.start }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var testrun = await TestRunsUrl.WithHeader(AuthorizationHeader, Pat)
                                        .PostAsync(content)
                                        .ConfigureAwait(false);
                result = testrun.Content.ReadAsJsonAsync<JObject>().Result;
            }

            return result;
        }

        public static async Task<JObject> GetTestResultsOfATestRunAsync(string testRunId)
        {
            return await string.Format(CultureInfo.InvariantCulture, TestResultsUrl, testRunId)
                                                        .WithHeader(AuthorizationHeader, Pat)
                                                        .GetJsonAsync<JObject>()
                                                        .ConfigureAwait(false);
        }

        public static async Task UpdateTestResultsOfATestRunAsync(string testRunId, List<object> resultArray)
        {
            using (var content = new CapturedStringContent(resultArray.ToArray().ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var result = await string.Format(CultureInfo.InvariantCulture, TestResultsUrl, testRunId)
                                            .WithHeader(AuthorizationHeader, Pat)
                                            .PatchAsync(content)
                                            .ConfigureAwait(false);
            }
        }

        public static async Task AddAttachmentToTestRunAsync(string testRunId, string file, List<string> testRunIds)
        {
            var bytes = File.ReadAllBytes(file);
            var stream = Convert.ToBase64String(bytes);
            using (var content = new CapturedStringContent(new { stream, fileName = file.Split('\\').Last(), attachmentType = "GeneralAttachment", comment = $"This file contains Test Results of following Test Runs - {string.Join(", ", testRunIds)}" }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var result = await string.Format(CultureInfo.InvariantCulture, TestRunAttachmentsUrl, testRunId)
                                        .WithHeader(AuthorizationHeader, Pat)
                                        .PostAsync(content)
                                        .ConfigureAwait(false);
            }
        }

        public static async Task UpdateTestRunAsync(TestRun testRun, string testRunId)
        {
            using (var content = new CapturedStringContent(new { state = "Completed", completedDate = testRun.Times.finish.ToString(), comment = "This Test Run has been created using an Automated Custom Utility." }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var updateTestRun = await string.Format(CultureInfo.InvariantCulture, TestRunUrl, testRunId)
                                        .WithHeader(AuthorizationHeader, Pat)
                                        .PatchAsync(content)
                                        .ConfigureAwait(false);
            }
        }

        public static async Task DeleteTestRunAsync(string testRunId)
        {
            if (testRunId != null)
            {
                var deleteTestRun = await string.Format(CultureInfo.InvariantCulture, TestRunUrl, testRunId)
                                        .WithHeader(AuthorizationHeader, Pat)
                                        .DeleteAsync()
                                        .ConfigureAwait(false);
            }
        }
    }
}
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

    /// <summary>
    /// Utility for querying Azure DevOps REST APIs.
    /// </summary>
    public static class AzureDevOpsUtility
    {
        private static string Pat;
        private static string WitUrl;
        private static string BaseUrl;
        private static string TestUrl;
        private static string WiqlUrl;
        private static string TestRunUrl;
        private static string TestRunsUrl;
        private static string TestPointsUrl;
        private static string TestResultsUrl;
        private static string BaseProjectUrl;
        private static string TestRunAttachmentsUrl;
        private static string TestSuitesByTestCaseIdUrl;
        private const string ApiVersion = "?api-version=5.0";
        private const string AuthorizationHeader = "Authorization";
        private const string ApiVersionAsParam = "&api-version=5.0";
        private const string ApiVersionPreview1 = "?api-version=5.0-preview.1";
        private const string ApiVersionPreview2 = "?api-version=5.0-preview.2";
        private const string JsonBatchHttpRequestMediaType = "application/json";        

        /// <summary>
        /// Update account information.
        /// </summary>
        /// <param name="account">Account.</param>
        /// <param name="project">Project.</param>
        /// <param name="token">PAT.</param>
        public static void UpdateAccountDetails(string account, string project, string token)
        {
            Pat = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
            BaseUrl = $"https://dev.azure.com/{account}/_apis/";
            BaseProjectUrl = BaseUrl.Replace("_apis", $"{project}/_apis");
            TestUrl = BaseProjectUrl + "test" + ApiVersion;
            WitUrl = BaseProjectUrl + "wit" + ApiVersion;
            WiqlUrl = WitUrl.Replace(ApiVersion, string.Empty) + "/wiql" + ApiVersion;
            TestPointsUrl = TestUrl.Replace(ApiVersion, string.Empty) + "/points" + ApiVersionPreview2;
            TestRunsUrl = TestUrl.Replace(ApiVersion, string.Empty) + "/runs" + ApiVersion;
            TestRunUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}" + ApiVersion;
            TestRunAttachmentsUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}/attachments" + ApiVersionPreview1;
            TestResultsUrl = TestRunsUrl.Replace(ApiVersion, string.Empty) + "/{0}/results" + ApiVersion;
            TestSuitesByTestCaseIdUrl = BaseUrl + "test/suites?testCaseId={0}" + ApiVersionAsParam;
        }

        /// <summary>
        /// Get WorkItems with Automated Test Name.
        /// </summary>
        /// <param name="automatedTestName">Automated Test Name.</param>
        /// <returns>List of WorkItems.</returns>
        public static async Task<JObject> GetWorkItemsWithAutomatedTestNameAsAsync(string automatedTestName)
        {
            dynamic result;
            var getWorkItemsWithAutomatedTestNameAs = "Select id From WorkItems Where [System.WorkItemType] = 'Test Case' AND [State] <> 'Closed' AND [State] <> 'Removed' AND [Microsoft.VSTS.TCM.AutomatedTestName]='{0}'";
            using (var content = new CapturedStringContent(new { query = string.Format(CultureInfo.InvariantCulture, getWorkItemsWithAutomatedTestNameAs, automatedTestName) }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var workTems = await WiqlUrl.WithHeader(AuthorizationHeader, Pat)
                                        .PostAsync(content)
                                        .ConfigureAwait(false);
                result = workTems.Content.ReadAsJsonAsync<JObject>().Result;
            }

            return result;
        }

        /// <summary>
        /// Get Test Suites by Test Case Id.
        /// </summary>
        /// <param name="testCaseId">Test Case Id.</param>
        /// <returns>List of Test Suites.</returns>
        public static async Task<JObject> GetTestSuitesByTestCaseIdAsync(string testCaseId)
        {
            return await string.Format(CultureInfo.InvariantCulture, TestSuitesByTestCaseIdUrl, testCaseId)
                                                    .WithHeader(AuthorizationHeader, Pat)
                                                    .GetJsonAsync<JObject>()
                                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Get Test Points by Test Case Ids.
        /// </summary>
        /// <param name="testCasesIds">Test Case Ids.</param>
        /// <returns>List of Test Points.</returns>
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

        /// <summary>
        /// Create new Test Run.
        /// </summary>
        /// <param name="testRun">Test Run details.</param>
        /// <param name="testPlanId">Test Plan Id.</param>
        /// <param name="testPointIds">Test Point Ids.</param>
        /// <param name="isAutomated">Automated??</param>
        /// <returns>New Test Run Object.</returns>
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

        /// <summary>
        /// Get Test Results of a Test Run.
        /// </summary>
        /// <param name="testRunId">Test Run Id.</param>
        /// <returns>List of Test Results.</returns>
        public static async Task<JObject> GetTestResultsOfATestRunAsync(string testRunId)
        {
            return await string.Format(CultureInfo.InvariantCulture, TestResultsUrl, testRunId)
                                                        .WithHeader(AuthorizationHeader, Pat)
                                                        .GetJsonAsync<JObject>()
                                                        .ConfigureAwait(false);
        }

        /// <summary>
        /// Update Test Results of a Test Run.
        /// </summary>
        /// <param name="testRunId">Test Run Id.</param>
        /// <param name="resultArray">Test Results Data.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Add Attachment to a Test Run.
        /// </summary>
        /// <param name="testRunId">Test Run Id.</param>
        /// <param name="file">Attachment.</param>
        /// <param name="testRunIds">Test Run Ids in which Attachment should be part of.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Update Test Run.
        /// </summary>
        /// <param name="testRun">Test Run details.</param>
        /// <param name="testRunId">Test Run Id.</param>
        /// <returns>Task.</returns>
        public static async Task UpdateTestRunAsync(TestRun testRun, string testRunId)
        {
            using (var content = new CapturedStringContent(new { state = Constants.Completed, completedDate = testRun.Times.finish.ToString(), comment = "This Test Run has been created using an Automated Custom Utility." }.ToJson(), Encoding.UTF8, JsonBatchHttpRequestMediaType))
            {
                var updateTestRun = await string.Format(CultureInfo.InvariantCulture, TestRunUrl, testRunId)
                                        .WithHeader(AuthorizationHeader, Pat)
                                        .PatchAsync(content)
                                        .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Delete Test Run.
        /// </summary>
        /// <param name="testRunId">Test Run Id.</param>
        /// <returns>Task.</returns>
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
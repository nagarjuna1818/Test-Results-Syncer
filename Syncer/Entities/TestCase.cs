namespace Syncer.Entities
{
    public class TestCase
    {
        public TestCase(string testCaseId, string testSuiteId, string testPlanId)
        {
            this.TestCaseId = testCaseId;
            this.TestSuiteId = testSuiteId;
            this.TestPlanId = testPlanId;
        }

        public string TestCaseId { get; set; }
        public string TestSuiteId { get; set; }
        public string TestPlanId { get; set; }
    }
}
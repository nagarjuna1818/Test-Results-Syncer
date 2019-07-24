namespace Syncer.Entities
{
    /// <summary>
    /// Test Case Modal.
    /// </summary>
    public class TestCase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="testCaseId">Test Case Id.</param>
        /// <param name="testSuiteId">Test Suite Id.</param>
        /// <param name="testPlanId">Test Plan Id.</param>
        public TestCase(string testCaseId, string testSuiteId, string testPlanId)
        {
            this.TestCaseId = testCaseId;
            this.TestSuiteId = testSuiteId;
            this.TestPlanId = testPlanId;
        }

        /// <summary>
        /// Test Case Id.
        /// </summary>
        public string TestCaseId { get; set; }

        /// <summary>
        /// Test Suite Id.
        /// </summary>
        public string TestSuiteId { get; set; }
        
        /// <summary>
        /// Test Plan Id.
        /// </summary>
        public string TestPlanId { get; set; }
    }
}
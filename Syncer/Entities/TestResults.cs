namespace Syncer.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// Test Results modal.
    /// </summary>
    public class TestResults
    {
        /// <summary>
        /// Test Suite Id.
        /// </summary>
        public int SuiteId { get; set; }

        /// <summary>
        /// Test Cases List.
        /// </summary>
        public List<TestCaseOutcome> TestCases { get; set; }
    }
}
namespace Syncer.Entities
{
    using System.Collections.Generic;

    public class TestResults
    {
        public int SuiteId { get; set; }
        public List<TestCaseOutcome> TestCases { get; set; }
    }
}
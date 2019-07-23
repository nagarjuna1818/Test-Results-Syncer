namespace Syncer.Entities
{
    using System;

    public class TestResult
    {
        public TestResult(string assembly, string className, string testName, string outcome = null, string error = null, string trace = null, TimeSpan? duration = null, string owner = null, string computerName = null)
        {
            this.Assembly = assembly;
            this.ClassName = className;
            this.TestName = testName;
            this.Outcome = outcome;
            this.Error = error;
            this.Trace = trace;
            this.Duration = duration;
            this.Owner = owner;
            this.ComputerName = computerName;
        }

        public string Assembly { get; set; }
        public string ClassName { get; set; }
        public string TestName { get; set; }
        public string Outcome { get; set; }
        public string Error { get; set; }
        public string Trace { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Owner { get; set; }

        public string ComputerName { get; set; }
    }
}
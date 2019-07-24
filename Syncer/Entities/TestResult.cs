namespace Syncer.Entities
{
    using System;

    /// <summary>
    /// Test Result.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="className"></param>
        /// <param name="testName"></param>
        /// <param name="outcome"></param>
        /// <param name="error"></param>
        /// <param name="trace"></param>
        /// <param name="duration"></param>
        /// <param name="owner"></param>
        /// <param name="computerName"></param>
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

        /// <summary>
        /// Assembly.
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// Class Name.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Test Name.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Outcome.
        /// </summary>
        public string Outcome { get; set; }

        /// <summary>
        /// Error.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Trace.
        /// </summary>
        public string Trace { get; set; }

        /// <summary>
        /// Duration.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Computer Name.
        /// </summary>
        public string ComputerName { get; set; }
    }
}
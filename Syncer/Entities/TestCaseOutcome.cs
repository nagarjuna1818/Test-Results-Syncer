namespace Syncer.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Test Case Outcome modal.
    /// </summary>
    public class TestCaseOutcome
    {
        /// <summary>
        /// Test Case Id.
        /// </summary>
        [Required]
        public int TestCaseId { get; set; }

        /// <summary>
        /// Outcome.
        /// </summary>
        [RequiredEnum((int)OutcomeType.Passed)]
        [JsonConverter(typeof(StringEnumConverter))]
        public OutcomeType Outcome { get; set; }

        /// <summary>
        /// Test Suite Id.
        /// </summary>
        public int TestSuiteId { get; set; }
    }

    /// <summary>
    /// Outcome Type.
    /// </summary>
    public enum OutcomeType
    {
        Passed,
        Failed,
        NotExecuted
    }
}
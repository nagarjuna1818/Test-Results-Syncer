namespace Syncer.Entities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using System.ComponentModel.DataAnnotations;

    public class TestCaseOutcome
    {
        [Required]
        public int TestCaseId { get; set; }

        [RequiredEnum((int)OutcomeType.Passed)]
        [JsonConverter(typeof(StringEnumConverter))]
        public OutcomeType Outcome { get; set; }

        public int TestSuiteId { get; set; }
    }

    public enum OutcomeType
    {
        Passed,
        Failed,
        NotExecuted
    }
}
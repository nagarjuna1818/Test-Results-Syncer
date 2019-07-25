namespace Syncer
{
    using CommandLine;
    using CommandLine.Text;

    using System.Collections.Generic;

    /// <summary>
    /// Options to be passed from command line.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Account.
        /// </summary>
        [Option('a', "account", Required = true, HelpText = "Azure DevOps Account / Organization")]
        public string Account { get; set; }

        /// <summary>
        /// Project.
        /// </summary>
        [Option('p', "project", Required = true, HelpText = "Azure DevOps Project")]
        public string Project { get; set; }

        /// <summary>
        /// Token.
        /// </summary>
        [Option('t', "token", Required = true, HelpText = "Azure DevOps PAT (Token)")]
        public string Token { get; set; }
    }

    /// <summary>
    /// Options to be passed from command line.
    /// </summary>
    [Verb("update", HelpText = "Update Test Results")]
    class UpdateOptions : Options
    {
        /// <summary>
        /// Input file path.
        /// </summary>
        [Option('f', "file", Required = true, HelpText = "File Path")]
        public string FilePath { get; set; }

        /// <summary>
        /// Test Suite Ids.
        /// </summary>
        [Option(Required = false, HelpText = "Test Suite Ids. Use this argument to specify particular Test-Suites")]
        public IEnumerable<string> TestSuiteIds { get; set; }

        /// <summary>
        /// Consideration of specified Test Suite Ids.
        /// </summary>
        [Option('c', Default = false, Required = false, HelpText = "true/false. Use this argument to specify that specified Test-suite Ids should only be considered for duplicate Test-cases")]
        public bool ConsiderTestSuitesIdsOnlyForDuplicateTestCases { get; set; }

        /// <summary>
        /// Examples.
        /// </summary>
        [Usage(ApplicationAlias = "syncer")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("\nFollowing command will update all Test-cases present in TRX and also updates all Test-Suites having same Test-cases", new UpdateOptions { FilePath = "c:\\users\\abc.trx", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will just update Test-cases present in specified Test-Suites. It will not update Test-cases which are not part of specified Test-Suites even though if Test Results for those Test-Cases are present in TRX file", new UpdateOptions { FilePath = "c:\\users\\abc.trx", TestSuiteIds = new[] { "1", "2" }, Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will update all Test-Cases present in TRX and will only consider specified Test-Suite Ids in-case of duplicate Test-Cases", new UpdateOptions { FilePath = "c:\\users\\abc.trx", ConsiderTestSuitesIdsOnlyForDuplicateTestCases = true, TestSuiteIds = new[] { "1", "2" }, Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will update all Test-cases present in JSON", new UpdateOptions { FilePath = "c:\\users\\abc.json", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will update all Test-cases present in Excel file of type .xlsx", new UpdateOptions { FilePath = "c:\\users\\abc.xlsx", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will update all Test-cases present in Excel file of type .xls", new UpdateOptions { FilePath = "c:\\users\\abc.xls", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
            }
        }
    }
}
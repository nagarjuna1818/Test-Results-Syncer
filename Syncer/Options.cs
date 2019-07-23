namespace Syncer
{
    using CommandLine;
    using CommandLine.Text;

    using System.Collections.Generic;

    public class Options
    {
        [Option('a', "account", Required = true, HelpText = "Azure DevOps Account/Org")]
        public string Account { get; set; }

        [Option('p', "project", Required = true, HelpText = "Azure DevOps Project")]
        public string Project { get; set; }

        [Option('t', "token", Required = true, HelpText = "Azure DevOps PAT (Token)")]
        public string Token { get; set; }
    }

    [Verb("trxupdate", HelpText = "Update Test Results using TRX")]
    class TrxOptions : Options
    {
        [Option('f', "file", Required = true, HelpText = "TRX File Path")]
        public string FilePath { get; set; }

        [Option(Required = false, HelpText = "Test Suite Ids. Use this argument to specify particular Test-Suites")]
        public IEnumerable<string> TestSuiteIds { get; set; }

        [Option('c', Default = false, Required = false, HelpText = "true/false. Use this argument to specify that specified Test-suite Ids should only be considered for duplicate Test-cases")]
        public bool ConsiderTestSuitesIdsOnlyForDuplicateTestCases { get; set; }

        [Usage(ApplicationAlias = "adtu")]

        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("\nFollowing command will update all Test-cases present in TRX and also updates all Test-Suites having same Test-cases", new TrxOptions { FilePath = "c:\\users\\abc.trx", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will just update Test-cases present in specified Test-Suites. It will not update Test-cases which are not part of specified Test-Suites even though if Test Results for those Test-Cases are present in TRX file", new TrxOptions { FilePath = "c:\\users\\abc.trx", TestSuiteIds = new[] { "1", "2" }, Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
                yield return new Example("\nFollowing command will update all Test-Cases present in TRX and will only consider specified Test-Suite Ids in-case of duplicate Test-Cases", new TrxOptions { FilePath = "c:\\users\\abc.trx", ConsiderTestSuitesIdsOnlyForDuplicateTestCases = true, TestSuiteIds = new[] { "1", "2" }, Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
            }
        }
    }

    [Verb("jsonupdate", HelpText = "Update Test Results using JSON")]
    class JsonOptions : Options
    {
        [Option('f', "file", Required = true, HelpText = "JSON File Path")]
        public string FilePath { get; set; }

        [Usage(ApplicationAlias = "adtu")]

        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("\nFollowing command will update all Test-cases present in JSON", new TrxOptions { FilePath = "c:\\users\\abc.json", Account = "namurako", Project = "ResultsUpdater", Token = "ds2f3m35a56s7i78l8w8efksjdcbsklfhwuie" });
            }
        }
    }
}
namespace Syncer
{
    using CommandLine;

    using Serilog;

    using Syncer.Entities;
    using Syncer.Utilities;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// StartUp class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static void Main(string[] args)
        {
            SetLogger();
            try
            {
                var result = Parser.Default.ParseArguments<UpdateOptions>(args)
                    .MapResult(
                    (UpdateOptions opts) => UpdateResults(opts.FilePath, opts.Account, opts.Project, opts.Token, opts.TestSuiteIds, opts.ConsiderTestSuitesIdsOnlyForDuplicateTestCases),
                    errs => HandleParseErrors(errs?.ToList()));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }

            Log.Information("Please press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Update Results from data in the specified input file.
        /// </summary>
        /// <param name="filePath">Input file path.</param>
        /// <param name="account">Azure DevOps Account / Organization.</param>
        /// <param name="project">Azure DevOps Project.</param>
        /// <param name="token">Azure DevOps PAT (Token).</param>
        /// <param name="testSuiteIds">Test Suite Ids.</param>
        /// <param name="consideration">Consideration of specified Test Suite Ids.</param>
        /// <returns>Integer.</returns>
        private static int UpdateResults(string filePath, string account, string project, string token, IEnumerable<string> testSuiteIds, bool consideration)
        {
            var files = new List<string>();
            if ((!filePath.EndsWith(Constants.Trx)) && (!filePath.EndsWith(Constants.Json)) && (!filePath.EndsWith(Constants.Xlsx)) && (!filePath.EndsWith(Constants.Xls)))
            {
                files = Directory.GetFiles(filePath, $"*.{Constants.Trx}").ToList();
                if (!(files.Count > 0))
                    files = Directory.GetFiles(filePath, $"*.{Constants.Json}").ToList();
                if (!(files.Count > 0))
                    files = Directory.GetFiles(filePath, $"*.{Constants.Xlsx}").ToList();
                if (!(files.Count > 0))
                    files = Directory.GetFiles(filePath, $"*.{Constants.Xls}").ToList();
                if (!(files.Count > 0))
                    throw new FileNotFoundException($"Cannot find TRX / JSON / Excel files under {filePath}. Please provide valid TRX / JSON / Excel file path.");
            }
            else
            {
                files.Add(filePath);
            }

            var file = files.FirstOrDefault();
            if (file.EndsWith(Constants.Trx))
                return TrxUtility.UpdateTestResults(file, account, project, token, testSuiteIds, consideration);
            else
                return TestResultUtlity.UpdateTestResults(file, file.EndsWith(Constants.Json), account, project, token);
        }

        /// <summary>
        /// Handle Parse Errors.
        /// </summary>
        /// <param name="errs">Error List.</param>
        /// <returns>Integer.</returns>
        private static int HandleParseErrors(List<Error> errs)
        {
            if (errs.Count > 1)
            {
                errs.ToList().ForEach(e => Log.Error(e.ToString()));
            }

            return errs.Count;
        }

        /// <summary>
        /// Setting up Logger.
        /// </summary>
        private static void SetLogger()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(outputTemplate: "[{Level:u3}] {Message}{NewLine}").Enrich.FromLogContext().CreateLogger();
        }
    }
}
namespace Syncer
{
    using CommandLine;

    using Serilog;

    using Syncer.Utilities;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            SetLogger();
            try
            {
                var result = Parser.Default.ParseArguments<TrxOptions, JsonOptions>(args)
                    .MapResult(
                    (TrxOptions opts) => UpdateUsingTRX(opts.FilePath, opts.Account, opts.Project, opts.Token, opts.TestSuiteIds, opts.ConsiderTestSuitesIdsOnlyForDuplicateTestCases),
                    (JsonOptions opts) => UpdateUsingJson(opts.FilePath, opts.Account, opts.Project, opts.Token),
                    errs => HandleParseErrors(errs?.ToList()));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }

            Log.Information("Please press any key to exit...");
            Console.ReadKey();
        }

        private static object UpdateUsingJson(string filePath, string account, string project, string token)
        {
            return JsonTestResultUtlity.UpdateResults(filePath, account, project, token);
        }

        private static int UpdateUsingTRX(string filePath, string account, string project, string token, IEnumerable<string> testSuiteIds, bool consideration)
        {
            return TrxUtility.UpdateTestResults(filePath, account, project, token, testSuiteIds, consideration);
        }

        private static int HandleParseErrors(List<Error> errs)
        {
            if (errs.Count > 1)
            {
                errs.ToList().ForEach(e => Log.Error(e.ToString()));
            }

            return errs.Count;
        }

        private static void SetLogger()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console(outputTemplate: "[{Level:u3}] {Message}{NewLine}").Enrich.FromLogContext().CreateLogger();
        }
    }
}
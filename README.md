# Test-Results-Syncer
## About Syncer
***Syncer*** is a command-line utility tool with rich set of features which updates Azure DevOps Test-Cases with Test Results depending upon type of test execution (Automated or Manual).

## Features of Syncer
Following are some features of Syncer on high-level:
- Update Azure DevOps test-cases using different supported Test Results files
- Supported file formats: **`Trx`**, **`Json`**, **`Excel (.xlsx / .xls)`**
- Integration with **`Build / Release pipelines`** to update test-cases right away after running tests with no manual intervention
- Support for providing particular Test Suites in which test-cases needs to be updated or in case of same test-case present in multiple test suites
- List down the test-cases which are not part of Azure DevOps

## Videos which can assist you in using the utility

[![How to use the utility locally](http://i3.ytimg.com/vi/Zjxrv3_J2BM/hqdefault.jpg)](https://www.youtube.com/watch?v=Zjxrv3_J2BM)

[![Integrate utility with pipeline](http://i3.ytimg.com/vi/U8cZKv5f7hE/hqdefault.jpg)](https://www.youtube.com/watch?v=U8cZKv5f7hE&feature=youtu.be)

## Usage
Generally, there are 2 types of Test Execution. 
1. Automated Test Execution - Testers automate their test-cases using any automation framework and execute them which will generate a file with Test results.
2. Manual Test Execution - Testers manually execute their test-cases, determine and capture the status / result of test-cases and update in Azure DevOps manually.

So, In case of Automated Test Execution, the generated Test Results (.trx) file will be used to update the Azure DevOps test-cases.

Supported file: **`Trx`**

Whereas, in case of Manual Test Execution, this utility tool supports 2 file formats in which you can provide details of test-cases and corresponding results, which then will be used to update Azure DevOps test-cases.

Supported files: **`Json`**, **`Excel`**

### Install Syncer Tool
To use this utility tool, you need to run the following command to install the Syncer tool from Nuget package and make it globally available to run from anywhere in your local machine.
```
dotnet tool install -g Syncer
```

Once you install the tool, open command prompt and run following command to verify if it is installed properly. Running the following command will show the available commands in Syncer tool.
```
syncer --help
```

### Pre-requisites
In order to update your Azure DevOps test-cases with this utility, following information is needed which will be used to authenticate with your project's Azure DevOps account.
```
1. Azure DevOps Account / Organization
2. Project Name
3. Personal Access Token (PAT)
```
Refer to following sample URI formats of Azure DevOps project to know details of your project - 
```
https://dev.azure.com/{your_organization}/{projectname}/
or
https://{your_organization}.visualstudio.com/{projectname}/
```

Please click [here](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops) to create a new PAT for yourself.


### Automated Test Results
Basically, when we run automated tests using test assemblies mode, a results file `(.trx)` will be generated, which contains the list of tests that it ran and the results of those tests. We then update the Azure DevOps test-cases manually with results from the results file.

Now, let's see, how we can leverage this results `(.trx)` file to update Azure DevOps test-cases automatically.

#### Update using Trx `(.trx)`:
Following command will update all Test-cases present in TRX and also updates all Test-Suites having same Test-cases.
```
syncer update -a account -p project -t token -f "c:\\path\abc.trx"
```
Following command will just update Test-cases present in specified Test-Suites. It will not update Test-cases which are not part of specified Test-Suites even though if Test Results for those Test-Cases are present in TRX file.
```
syncer update -a account -p project -t token -f "c:\\path\abc.trx" --testsuiteids 44 45
```
Following command will update all Test-Cases present in TRX and will only consider specified Test-Suite Ids when a test-case is part of multiple test suites.
```
syncer update -a account -p project -t token -f "c:\\path\abc.trx" -c --testsuiteids 44 45
```
#### Integration with Azure DevOps Build / Release pipelines `(CI / CD)`:
Right now, after running our automated tests using Test task in pipelines, we get the results `(.trx)` file as an attachment which we download, analyze the results, go to Test plan and manually update the test-cases with corresponding status.

This utility is flexible enough to be utilized in Azure DevOps pipelines and can be created as a task after the Test task in pipelines to update test-cases as part of pipeline itself, such that, you don't have to manually download the `.trx` file and update the test-cases in Test plan.

#### Prerequisite for integrating with pipelines:
Install the Syncer nuget package from Nuget package manager in your Test project. This will download the Syncer tool assemblies (dlls) under project's `bin/debug` or `bin/release` folder.

As the pipeline tasks will be run on Build Agents, you need to either make the Syncer tool as .NET global tool in your agent or follow the second approach of executing the Syncer tool. Following will explain you the second approach of running Syncer tool.

#### How do I run Syncer tool if I don't make it .NET Global tool in Build Agent ?

If you do not make it a Global tool, then you should explicitly provide the path of the Syncer tool assembly (.dll) to the `dotnet` command as below.
```
dotnet "pathToSyncer.dll" update -a account -p project -t token -f "c:\\path\abc.json"
```
If you make it .NET Global tool, the above command can be simply run as following:
```
syncer update -a account -p project -t token -f "c:\\path\abc.json"
```
So, the only difference is, instead of providing `dotnet "<path to syncer dll>"` in the command, we simply provide `syncer` to command by registering as .NET Global tool.

### Manual Test Results
#### Update using Json `(.json)`:
Following command will update all Test-cases present in **`JSON`**
```
syncer update -a account -p project -t token -f "c:\\path\abc.json"
```
Please look at the sample Json file [here](https://github.com/nagarjuna1818/Test-Results-Syncer/tree/master/Syncer/TestData) to get started with updating your test-cases from Json file.

**`NOTE:`** Please make sure you follow the same schema as in sample json file while creating your json file.

Talking about Json schema followed by this utility tool:
Basically, it is JObject containing 2 properties:
```
1. suiteId                - integer (Optional.)
2. testCases              - JArray (Required.)
```
Drilling into testCases property, it is an array of JObject items containing following properties:
```
1. testCaseId             - integer (Required.)
2. outcome                - string (Required. Value must be either Passed, Failed or NotExecuted)
3. testSuiteId            - integer (Optional.)
```
**`NOTE:`** *testSuiteId* property will be helpful when a test-case is part of multiple test suites and if you want to update test-case in single test suite only. If *testSuiteId* is not provided for a test-case, then this tool will update the test-case in all the test suites it is part of.

If you see below sample, there are 2 test-cases. First test-case has *testSuiteId* property, whereas second test-case doesn't have *testSuiteId* property. It means that first test-case will be updated in test suite with id as 8, whereas second test-case will be updated in all test suites whichever it is part of.
```
{
   "testCases": [
      {
        "testCaseId": 7,
        "outcome": "Passed",
        "testSuiteId": 8
      },
      {
        "testCaseId": 9,
        "outcome": "Passed"
      }
    ]
}
```
**`NOTE:`** *suiteId* property makes it easier for you when you want to update test-cases of a single test suite. In this case, you don't need to provide *testSuiteId* property for each *testCase* rather provide id of test suite in *suiteId* property. Following sample will update test-cases present in test suite with id 6.
```
{
   "suiteId": 6,
   "testCases": [
      {
        "testCaseId": 7,
        "outcome": "Passed"
      },
      {
        "testCaseId": 9,
        "outcome": "Passed"
      }
    ]
}
```
**`NOTE:`** When both *suiteId* and *testSuiteId* properties are given, the *testSuiteId* property value will be considered for that test-case. Following sample will update test-case with id 7 in test suite with id 8 and test-case with id 9 in test suite with id 6.
```
{
   "suiteId": 6,
   "testCases": [
      {
        "testCaseId": 7,
        "outcome": "Passed",
        "testSuiteId": 8
      },
      {
        "testCaseId": 9,
        "outcome": "Passed"
      }
    ]
}
```

#### Update using Excel `(.xlsx or .xls)`:
Following command will update all Test-cases present in Excel file of type **`.xlsx`**
```
syncer update -a account -p project -t token -f "c:\\path\abc.xlsx"
```
Following command will update all Test-cases present in Excel file of type **`.xls`**
```
syncer update -a account -p project -t token -f "c:\\path\abc.xls"
```
Please look at the sample Excel file [here](https://github.com/nagarjuna1818/Test-Results-Syncer/tree/master/Syncer/TestData) to get started with updating your test-cases from Excel file. 

The Excel file should contain following 3 columns:
```
1. TestCaseId             - integer (Required)
2. Outcome                - string (Optional. Defaults to Passed). Value must be either Passed, Failed or NotExecuted
3. TestSuiteId            - integer (Optional)
```
**`NOTE`**: *TestSuiteId* column will be helpful when a test-case is part of multiple test suites and if you want to update test-case in single test suite only. If *TestSuiteId* is not provided, then this tool will update the test-case in all the test suites it is part of. 

## Help
> general
```
--help                      Display this help screen
USAGE: syncer --help
--version                   Display version information
USAGE: syncer --version
```
> update
```
-a, --account               Required. Azure DevOps Account / Organization
-p, --project               Required. Azure DevOps Project
-t, --token                 Required. Azure DevOps PAT (Token)
-f, --file                  Required. File Path
--testsuiteids              Test Suite Ids. Use this argument to specify particular 
                            Test-Suites.
-c                          (Default: false) true/false. Use this argument to specify 
                            that specified Test-suite Ids should only be considered 
                            for duplicate Test-cases
```

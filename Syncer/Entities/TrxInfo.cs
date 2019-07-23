namespace Syncer.Entities
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010", IsNullable = false)]
    public partial class TestRun
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("UnitTestResult", IsNullable = false)]
        public TestRunUnitTestResult[] Results { get; set; }

        public TestRunResultSummary ResultSummary { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string runUser { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("UnitTest", IsNullable = false)]
        public TestRunUnitTest[] TestDefinitions { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("TestEntry", IsNullable = false)]
        public TestRunTestEntry[] TestEntries { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("TestList", IsNullable = false)]
        public TestRunTestList[] TestLists { get; set; }

        public TestRunTestSettings TestSettings { get; set; }

        public TestRunTimes Times { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunResultSummary
    {
        public TestRunResultSummaryCounters Counters { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string outcome { get; set; }

        public TestRunResultSummaryOutput Output { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("RunInfo", IsNullable = false)]
        public TestRunResultSummaryRunInfo[] RunInfos { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunResultSummaryCounters
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int aborted { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int completed { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int disconnected { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int error { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint executed { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int failed { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int inconclusive { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int inProgress { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int notExecuted { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int notRunnable { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int passed { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int passedButRunAborted { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int pending { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int timeout { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint total { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int warning { get; set; }

    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunResultSummaryOutput
    {
        public string StdOut { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunResultSummaryRunInfo
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string computerName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string outcome { get; set; }

        public string Text { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime timestamp { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunTestEntry
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string executionId { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testId { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testListId { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunTestList
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunTestSettings
    {
        public TestRunTestSettingsDeployment Deployment { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunTestSettingsDeployment
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string runDeploymentRoot { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunTimes
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime creation { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime finish { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime queuing { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime start { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTest
    {
        public TestRunUnitTestExecution Execution { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string storage { get; set; }

        public TestRunUnitTestTestMethod TestMethod { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestExecution
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResult
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string computerName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public System.DateTime duration { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime endTime { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string executionId { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string outcome { get; set; }

        public TestRunUnitTestResultOutput Output { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string relativeResultsDirectory { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime startTime { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testId { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testListId { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string testType { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResultOutput
    {
        public TestRunUnitTestResultOutputErrorInfo ErrorInfo { get; set; }

        public string StdOut { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResultOutputErrorInfo
    {
        public string Message { get; set; }

        public string StackTrace { get; set; }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestTestMethod
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string adapterTypeName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string className { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string codeBase { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name { get; set; }
    }
}
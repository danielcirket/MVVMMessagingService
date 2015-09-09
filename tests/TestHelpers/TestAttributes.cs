using System;
using Xunit.Sdk;

namespace TestHelpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("TestHelpers.TestDiscoverer", "TestHelpers")]
    public class UnitTestAttribute : Xunit.FactAttribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("TestHelpers.TestDiscoverer", "TestHelpers")]
    public class ComponentTestAttribute : Xunit.FactAttribute
    {
        public ComponentTestAttribute()
        {
#if !ENABLE_COMPONENT_TESTS
            base.Skip = "Componenet tests not enabled.";
#endif
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("TestHelpers.TestDiscoverer", "TestHelpers")]
    public class IntegrationTestAttribute : Xunit.FactAttribute
    {
        public IntegrationTestAttribute(IntegrationTestJustification justification)
        {
#if !ENABLE_INTEGRATION_TESTS
            base.Skip = $"Integration Tests not Enabled. This test is marked as an integration test because [{justification.ToString()}]";
#endif
        }
    }

    [Flags]
    public enum IntegrationTestJustification
    {
        Unknown = 0,
        UsesNetworkIO = 1,
        UsesDiskIO = 2,
        UsesUnsafeClass = 4,
        UsesMulitpleThreads = 8,
        UsesThreadSynchronisation = 16
    }
}

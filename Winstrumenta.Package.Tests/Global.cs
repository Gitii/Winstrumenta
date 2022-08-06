namespace Winstrumenta.Package.Tests;

[TestFixture]
public class Global
{
    [OneTimeSetUp]
    public void Init()
    {
        TestEnvironment.KillApp();
        TestEnvironment.KillWAD(TestContext.CurrentContext.TestDirectory);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        TestEnvironment.KillApp();
        TestEnvironment.KillWAD(TestContext.CurrentContext.TestDirectory);
    }
}

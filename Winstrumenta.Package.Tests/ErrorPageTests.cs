namespace Winstrumenta.Package.Tests;

public class ErrorPageTests : TestSuiteBase
{
    [Test]
    public void ShouldShowErrorPageWhenArgumentsAreInvalid()
    {
        var invalidCliArguments = "rjfaioudgjiudigdugh";

        RestartApp(invalidCliArguments);

        GetSession().FindElementByAccessibilityId("CloseButton").Click();
    }
}

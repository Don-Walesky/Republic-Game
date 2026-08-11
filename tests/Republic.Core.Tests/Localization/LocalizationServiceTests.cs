namespace Republic.Core.Tests.Localization;

using Republic.Core.Localization.Models;
using Republic.Core.Localization.Services;
using Xunit;

public sealed class LocalizationServiceTests
{
    [Fact]
    public void GetText_ReturnsEnglishByDefault()
    {
        var service = new LocalizationService();

        string title = service.GetText("app_title");

        Assert.Equal("Republic - Presidential Desk", title);
    }

    [Fact]
    public void SwitchLanguage_ReturnsFrenchTranslation()
    {
        var service = new LocalizationService
        {
            CurrentLanguage = Language.French
        };

        string title = service.GetText("app_title");

        Assert.Equal("République - Bureau Présidentiel", title);
    }

    [Fact]
    public void GetText_FormatsParametersCorrectly()
    {
        var service = new LocalizationService
        {
            CurrentLanguage = Language.Spanish
        };

        string text = service.GetText("defcon_alert", 1);

        Assert.Equal("ALERTA: Nivel DEFCON 1", text);
    }
}

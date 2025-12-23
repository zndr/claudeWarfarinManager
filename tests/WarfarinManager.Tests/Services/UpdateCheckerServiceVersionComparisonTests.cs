using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WarfarinManager.Core.Services;
using Xunit;
using Xunit.Abstractions;

namespace WarfarinManager.Tests.Services;

/// <summary>
/// Test per verificare il confronto versioni in UpdateCheckerService
/// </summary>
public class UpdateCheckerServiceVersionComparisonTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<UpdateCheckerService>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;

    public UpdateCheckerServiceVersionComparisonTests(ITestOutputHelper output)
    {
        _output = output;
        _loggerMock = new Mock<ILogger<UpdateCheckerService>>();
        _httpClientMock = new Mock<HttpClient>();
    }

    [Theory]
    [InlineData("1.2.4.2", "1.2.4.2", false)] // Stessa versione - NON deve segnalare aggiornamento
    [InlineData("1.2.4.3", "1.2.4.2", true)]  // Remota più recente - deve segnalare
    [InlineData("1.2.4.1", "1.2.4.2", false)] // Remota più vecchia - NON deve segnalare
    [InlineData("1.2.5.0", "1.2.4.2", true)]  // Minor version più recente
    [InlineData("1.3.0.0", "1.2.4.2", true)]  // Major version più recente
    [InlineData("2.0.0.0", "1.2.4.2", true)]  // Major version molto più recente
    [InlineData("1.2.4.0", "1.2.4.2", false)] // Remota più vecchia
    public void IsNewerVersion_ShouldCompareCorrectly(string remoteVersion, string localVersion, bool expectedResult)
    {
        // Arrange
        var service = new UpdateCheckerService(
            _loggerMock.Object,
            new HttpClient(),
            "https://example.com/version.json");

        // Act
        var result = service.IsNewerVersion(remoteVersion, localVersion);

        // Assert
        _output.WriteLine($"Remote: {remoteVersion}, Local: {localVersion}, Expected: {expectedResult}, Actual: {result}");
        result.Should().Be(expectedResult,
            $"versione remota {remoteVersion} confrontata con locale {localVersion} dovrebbe restituire {expectedResult}");
    }

    [Theory]
    [InlineData("v1.2.4.2", "1.2.4.2", false)] // Con prefisso 'v'
    [InlineData("1.2.4", "1.2.4.2", false)]    // Versione a 3 componenti vs 4
    [InlineData("1.2", "1.2.0.0", false)]      // Versione a 2 componenti
    public void IsNewerVersion_ShouldHandleDifferentFormats(string remoteVersion, string localVersion, bool expectedResult)
    {
        // Arrange
        var service = new UpdateCheckerService(
            _loggerMock.Object,
            new HttpClient(),
            "https://example.com/version.json");

        // Act
        var result = service.IsNewerVersion(remoteVersion, localVersion);

        // Assert
        _output.WriteLine($"Remote: {remoteVersion}, Local: {localVersion}, Expected: {expectedResult}, Actual: {result}");
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsNewerVersion_WithIdenticalVersions_ShouldReturnFalse()
    {
        // Arrange
        var service = new UpdateCheckerService(
            _loggerMock.Object,
            new HttpClient(),
            "https://example.com/version.json");

        var version = "1.2.4.2";

        // Act
        var result = service.IsNewerVersion(version, version);

        // Assert
        _output.WriteLine($"Confronto stesso valore: {version} > {version} = {result}");
        result.Should().BeFalse("la stessa versione non dovrebbe essere considerata più recente");
    }

    [Fact]
    public void DotNetVersionComparison_ShouldBehaviorCheck()
    {
        // Test di verifica del comportamento di System.Version
        var v1 = new Version("1.2.4.2");
        var v2 = new Version("1.2.4.2");
        var v3 = new Version("1.2.4.3");

        _output.WriteLine($"v1 (1.2.4.2) > v2 (1.2.4.2) = {v1 > v2}"); // Dovrebbe essere false
        _output.WriteLine($"v1 (1.2.4.2) == v2 (1.2.4.2) = {v1 == v2}"); // Dovrebbe essere true
        _output.WriteLine($"v3 (1.2.4.3) > v1 (1.2.4.2) = {v3 > v1}"); // Dovrebbe essere true

        (v1 > v2).Should().BeFalse();
        (v1 == v2).Should().BeTrue();
        (v3 > v1).Should().BeTrue();
    }
}

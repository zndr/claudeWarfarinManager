using WarfarinManager.Core.Services;
using WarfarinManager.Core.Interfaces;
using WarfarinManager.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace WarfarinManager.Tests.Services;

public class TTRCalculatorServiceTests
{
    private readonly ITTRCalculatorService _service;
    private readonly Mock<ILogger<TTRCalculatorService>> _mockLogger;

    public TTRCalculatorServiceTests()
    {
        _mockLogger = new Mock<ILogger<TTRCalculatorService>>();
        _service = new TTRCalculatorService(_mockLogger.Object);
    }

    #region CalculateTTR Tests

    [Fact]
    public void CalculateTTR_AllInRange_Returns100Percent()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 2.5m 
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 8), 
                INRValue = 2.5m 
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 15), 
                INRValue = 2.5m 
            }
        };

        // Act
        var result = _service.CalculateTTR(controls, 2.0m, 3.0m);

        // Assert
        result.Should().NotBeNull();
        result.TTRPercentage.Should().Be(100m);
        result.Quality.Should().Be(TTRQuality.Excellent);
        result.DaysInRange.Should().Be(result.TotalDays);
        result.DaysBelowRange.Should().Be(0);
        result.DaysAboveRange.Should().Be(0);
    }

    [Fact]
    public void CalculateTTR_AllBelowRange_Returns0Percent()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 1.5m 
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 8), 
                INRValue = 1.5m 
            }
        };

        // Act
        var result = _service.CalculateTTR(controls, 2.0m, 3.0m);

        // Assert
        result.TTRPercentage.Should().Be(0m);
        result.Quality.Should().Be(TTRQuality.Poor);
        result.DaysBelowRange.Should().Be(result.TotalDays);
        result.DaysAboveRange.Should().Be(0);
    }

    [Fact]
    public void CalculateTTR_MixedValues_ReturnsCorrectPercentage()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 1.8m  // Sotto range
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 8), 
                INRValue = 2.5m  // In range
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 15), 
                INRValue = 3.5m  // Sopra range
            }
        };

        // Act
        var result = _service.CalculateTTR(controls, 2.0m, 3.0m);

        // Assert - Con interpolazione lineare circa 50% sarà in range
        result.TTRPercentage.Should().BeGreaterThan(30m).And.BeLessThan(70m);
        result.DaysBelowRange.Should().BeGreaterThan(0);
        result.DaysAboveRange.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateTTR_SingleControl_ReturnsZero()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 2.5m 
            }
        };

        // Act
        var result = _service.CalculateTTR(controls, 2.0m, 3.0m);

        // Assert
        result.TTRPercentage.Should().Be(0m);
        result.TotalDays.Should().Be(0);
    }

    [Fact]
    public void CalculateTTR_EmptyList_ReturnsZero()
    {
        // Arrange
        var controls = new List<INRControl>();

        // Act
        var result = _service.CalculateTTR(controls, 2.0m, 3.0m);

        // Assert
        result.TTRPercentage.Should().Be(0m);
        result.TotalDays.Should().Be(0);
    }

    #endregion

    #region InterpolateINR Tests

    [Fact]
    public void InterpolateINR_TwoControls_CorrectlyInterpolates()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 2.0m 
            },
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 11), 
                INRValue = 3.0m 
            }
        };

        // Act
        var result = _service.InterpolateINR(controls);

        // Assert
        result.Should().HaveCount(11); // 1 gen + 9 giorni intermedi + 11 gen
        result[new DateTime(2024, 1, 1)].Should().Be(2.0m);
        result[new DateTime(2024, 1, 6)].Should().BeApproximately(2.5m, 0.1m);
        result[new DateTime(2024, 1, 11)].Should().Be(3.0m);
    }

    [Fact]
    public void InterpolateINR_SingleControl_ReturnsEmpty()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl 
            { 
                ControlDate = new DateTime(2024, 1, 1), 
                INRValue = 2.5m 
            }
        };

        // Act
        var result = _service.InterpolateINR(controls);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region EvaluateQuality Tests

    [Theory]
    [InlineData(75.0, TTRQuality.Excellent)]
    [InlineData(70.0, TTRQuality.Excellent)]
    [InlineData(67.0, TTRQuality.Good)]
    [InlineData(62.0, TTRQuality.Acceptable)]
    [InlineData(55.0, TTRQuality.Suboptimal)]
    [InlineData(45.0, TTRQuality.Poor)]
    public void EvaluateQuality_ReturnsCorrectQuality(double ttr, TTRQuality expectedQuality)
    {
        // Act
        var result = _service.EvaluateQuality((decimal)ttr);

        // Assert
        result.Should().Be(expectedQuality);
    }

    #endregion

    #region CalculateINRStatistics Tests

    [Fact]
    public void CalculateINRStatistics_ReturnsCorrectStats()
    {
        // Arrange
        var controls = new List<INRControl>
        {
            new INRControl { ControlDate = DateTime.Now.AddDays(-14), INRValue = 2.0m },
            new INRControl { ControlDate = DateTime.Now.AddDays(-7), INRValue = 2.5m },
            new INRControl { ControlDate = DateTime.Now, INRValue = 3.0m }
        };

        // Act
        var result = _service.CalculateINRStatistics(controls);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result.Mean.Should().BeApproximately(2.5m, 0.1m);
        result.Min.Should().Be(2.0m);
        result.Max.Should().Be(3.0m);
    }

    #endregion

    #region CalculateTTRTrend Tests

    [Fact]
    public void CalculateTTRTrend_ReturnsRollingWindowResults()
    {
        // Arrange
        var controls = new List<INRControl>();
        var startDate = new DateTime(2024, 1, 1);
        
        // Crea 6 mesi di dati (controlli ogni 2 settimane)
        for (int i = 0; i < 12; i++)
        {
            controls.Add(new INRControl
            {
                ControlDate = startDate.AddDays(i * 14),
                INRValue = 2.5m // Tutti in range per semplicità
            });
        }

        // Act
        var result = _service.CalculateTTRTrend(controls, 2.0m, 3.0m, 3);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Values.Should().AllSatisfy(ttr => ttr.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(100));
    }

    #endregion
}
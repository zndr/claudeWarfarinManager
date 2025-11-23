using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WarfarinManager.Core.Services;
using WarfarinManager.Core.Models;
using WarfarinManager.Shared.Enums;
using Xunit;

namespace WarfarinManager.Tests.Services;

public class DosageCalculatorServiceTests
{
    private readonly IDosageCalculatorService _service;
    private readonly Mock<ILogger<DosageCalculatorService>> _mockLogger;

    public DosageCalculatorServiceTests()
    {
        _mockLogger = new Mock<ILogger<DosageCalculatorService>>();
        _service = new DosageCalculatorService(_mockLogger.Object);
    }

    #region FCSA Tests - INR Basso (Target 2.0-3.0)

    [Theory]
    [InlineData(1.85, 2.0, 3.0, 35.0, 35.0, 14, false)] // INR 1.8-1.9: dose carico +25%, no cambio settimanale
    [InlineData(1.6, 2.0, 3.0, 35.0, 37.625, 7, false)] // INR 1.5-1.7: dose carico +50%, +7.5%
    [InlineData(1.2, 2.0, 3.0, 35.0, 38.5, 5, true)] // INR <1.5: dose carico ×2, +10%, EBPM se alto rischio
    public void CalculateFCSA_INRBelowRange_ReturnsCorrectDosage(
        double currentINR,
        double targetMin,
        double targetMax,
        double currentDose,
        double expectedNewDose,
        int expectedNextControlDays,
        bool expectEBPMWarning)
    {
        // Arrange
        var phase = TherapyPhase.Maintenance;
        var isCompliant = true;
        var isSlowMetabolizer = false;
        var risk = ThromboembolicRisk.High;

        // Act
        var result = _service.CalculateFCSA(
            (decimal)currentINR, (decimal)targetMin, (decimal)targetMax, (decimal)currentDose,
            phase, isCompliant, isSlowMetabolizer, risk);

        // Assert
        result.Should().NotBeNull();
        result.GuidelineUsed.Should().Be(GuidelineType.FCSA);
        result.SuggestedWeeklyDoseMg.Should().BeApproximately((decimal)expectedNewDose, 1.0m);
        result.NextControlDays.Should().Be(expectedNextControlDays);
        result.IsInRange.Should().BeFalse();
        result.INRStatus.Should().Be(INRStatus.BelowRange);

        if (expectEBPMWarning && currentINR < 1.5)
        {
            result.RequiresEBPM.Should().BeTrue();
            result.Warnings.Should().Contain(w => w.Contains("EBPM"));
        }
    }

    #endregion

    #region FCSA Tests - INR Alto

    [Theory]
    [InlineData(3.5, 2.0, 3.0, 35.0, 32.375, 7, false, false)] // INR 3-5: -7.5%
    [InlineData(5.5, 2.0, 3.0, 35.0, 32.375, 5, false, false)] // INR 5-6: stop 1g, -7.5%
    [InlineData(6.5, 2.0, 3.0, 35.0, 31.5, 1, true, true)] // INR >6: stop + Vit K, -10%
    public void CalculateFCSA_INRAboveRange_ReturnsCorrectDosage(
        double currentINR,
        double targetMin,
        double targetMax,
        double currentDose,
        double expectedNewDose,
        int expectedNextControlDays,
        bool expectVitKRequired,
        bool expectCriticalWarning)
    {
        // Arrange
        var phase = TherapyPhase.Maintenance;
        var isCompliant = true;
        var isSlowMetabolizer = false;

        // Act
        var result = _service.CalculateFCSA(
            (decimal)currentINR, (decimal)targetMin, (decimal)targetMax, (decimal)currentDose,
            phase, isCompliant, isSlowMetabolizer);

        // Assert
        result.Should().NotBeNull();
        result.GuidelineUsed.Should().Be(GuidelineType.FCSA);
        result.SuggestedWeeklyDoseMg.Should().BeApproximately((decimal)expectedNewDose, 1.0m);
        result.NextControlDays.Should().Be(expectedNextControlDays);
        result.IsInRange.Should().BeFalse();
        result.INRStatus.Should().Be(INRStatus.AboveRange);

        if (expectVitKRequired)
        {
            result.RequiresVitaminK.Should().BeTrue();
            result.Warnings.Should().Contain(w => w.Contains("Vitamina K") || w.Contains("critico"));
        }
    }

    #endregion

    #region FCSA Tests - INR in Range

    [Fact]
    public void CalculateFCSA_INRInRange_MaintainsDose()
    {
        // Arrange
        decimal currentINR = 2.5m;
        decimal targetMin = 2.0m;
        decimal targetMax = 3.0m;
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            currentINR, targetMin, targetMax, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.Should().NotBeNull();
        result.GuidelineUsed.Should().Be(GuidelineType.FCSA);
        result.IsInRange.Should().BeTrue();
        result.INRStatus.Should().Be(INRStatus.InRange);
        result.SuggestedWeeklyDoseMg.Should().Be(currentDose);
        result.NextControlDays.Should().BeGreaterOrEqualTo(28); // Manutenzione stabile
    }

    #endregion

    #region ACCP Tests

    [Fact]
    public void CalculateACCP_INRAboveRange_DifferentFromFCSA()
    {
        // ACCP è più conservativo con Vitamina K (soglia 10 vs 6 FCSA)
        decimal currentINR = 7.0m;
        decimal currentDose = 35m;

        // Act FCSA
        var resultFCSA = _service.CalculateFCSA(
            currentINR, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Act ACCP
        var resultACCP = _service.CalculateACCP(
            currentINR, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Assert - FCSA raccomanda Vit K, ACCP no
        resultFCSA.RequiresVitaminK.Should().BeTrue();
        resultACCP.RequiresVitaminK.Should().BeFalse();
    }

    #endregion

    #region GenerateWeeklySchedule Tests

    [Theory]
    [InlineData(17.5)] // 2.5mg × 7 giorni
    [InlineData(35.0)]   // 5mg × 7 giorni
    [InlineData(30.0)]   // 5mg × 6 + 0mg × 1
    [InlineData(37.5)] // 5mg × 6 + 7.5mg × 1
    [InlineData(52.5)] // 7.5mg × 7
    public void GenerateWeeklySchedule_ValidDose_CreatesCorrectSchedule(double weeklyDose)
    {
        // Act
        var schedule = _service.GenerateWeeklySchedule((decimal)weeklyDose);

        // Assert
        schedule.Should().NotBeNull();
        schedule.TotalWeeklyDose.Should().Be((decimal)weeklyDose);
        
        // Verifica che abbia dosi per tutti i 7 giorni
        schedule.DailyDoses.Should().HaveCount(7);
        
        // Tutte le dosi devono essere multipli di 2.5mg
        schedule.DailyDoses.Values.Should().OnlyContain(d => d % 2.5m == 0);

        // Nessuna dose negativa
        schedule.DailyDoses.Values.Should().OnlyContain(d => d >= 0);

        // Totale corretto
        schedule.DailyDoses.Values.Sum().Should().Be((decimal)weeklyDose);
        
        // Usa proprietà helper per verificare accesso diretto
        var totalViaProperties = schedule.Monday + schedule.Tuesday + schedule.Wednesday +
                                 schedule.Thursday + schedule.Friday + schedule.Saturday + schedule.Sunday;
        totalViaProperties.Should().Be((decimal)weeklyDose);
    }

    [Fact]
    public void GenerateWeeklySchedule_30mg_PreferWholeTablets()
    {
        // 30mg/settimana = preferire 5mg × 6 giorni (no dose 1 giorno)
        // Alternativa: 5mg × 5 + 2.5mg × 2

        // Act
        var schedule = _service.GenerateWeeklySchedule(30m);

        // Assert
        schedule.TotalWeeklyDose.Should().Be(30m);
        
        var doses = schedule.DailyDoses.Values.ToList();
        
        // Deve usare solo 0mg, 2.5mg, 5mg, 7.5mg (no frazioni complesse)
        doses.Should().OnlyContain(d => d == 0 || d == 2.5m || d == 5.0m || d == 7.5m);

        // Verifica che abbia descrizioni per ogni giorno
        schedule.DailyDescriptions.Should().HaveCount(7);
        
        // Le descrizioni devono contenere formati leggibili
        foreach (var kvp in schedule.DailyDoses)
        {
            var dayOfWeek = kvp.Key;
            var dose = kvp.Value;
            
            // Dovrebbe avere una descrizione per questo giorno
            schedule.DailyDescriptions.Should().ContainKey(dayOfWeek);
            
            if (dose == 5.0m)
            {
                schedule.DailyDescriptions[dayOfWeek].Should().Contain("1 cp");
            }
            else if (dose == 2.5m)
            {
                schedule.DailyDescriptions[dayOfWeek].Should().Contain("1/2 cp");
            }
        }
    }

    #endregion

    #region Metabolizzatore Lento Tests

    [Fact]
    public void CalculateFCSA_SlowMetabolizer_AddsWarning()
    {
        // Arrange
        decimal currentINR = 2.5m;
        decimal currentDose = 12.5m; // Dose bassa = metabolizzatore lento

        // Act
        var result = _service.CalculateFCSA(
            currentINR, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, isSlowMetabolizer: true);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("METABOLIZZATORE LENTO") || 
                                             w.Contains("metabolizzatore lento"));
    }

    #endregion

    #region Non-Compliance Tests

    [Fact]
    public void CalculateFCSA_NonCompliant_AddsWarning()
    {
        // Act
        var result = _service.CalculateFCSA(
            2.5m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, isCompliant: false, false);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("compliance") || 
                                             w.Contains("4D") ||
                                             w.Contains("Verificare"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateFCSA_InvalidINR_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _service.CalculateFCSA(-1m, 2.0m, 3.0m, 35m, TherapyPhase.Maintenance, true, false));
    }

    [Fact]
    public void GenerateWeeklySchedule_ZeroDose_ReturnsValidSchedule()
    {
        // Act
        var schedule = _service.GenerateWeeklySchedule(0m);

        // Assert
        schedule.Should().NotBeNull();
        schedule.TotalWeeklyDose.Should().Be(0m);
        schedule.DailyDoses.Values.Should().OnlyContain(d => d == 0);
    }

    #endregion
}
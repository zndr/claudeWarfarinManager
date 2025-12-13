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

    #region Classificazione Fascia INR Tests

    [Theory]
    [InlineData(1.3, 2.0, 3.0, FasciaINR.SubCritico)]
    [InlineData(1.6, 2.0, 3.0, FasciaINR.SubModerato)]
    [InlineData(1.85, 2.0, 3.0, FasciaINR.SubLieve)]
    [InlineData(2.5, 2.0, 3.0, FasciaINR.InRange)]
    [InlineData(3.2, 2.0, 3.0, FasciaINR.SovraLieve)]
    [InlineData(3.7, 2.0, 3.0, FasciaINR.SovraModerato)]
    [InlineData(4.5, 2.0, 3.0, FasciaINR.SovraAlto)]
    [InlineData(5.5, 2.0, 3.0, FasciaINR.SovraMoltoAlto)]
    [InlineData(7.0, 2.0, 3.0, FasciaINR.SovraCritico)]
    [InlineData(9.0, 2.0, 3.0, FasciaINR.SovraEstremo)]
    public void CalculateFCSA_ClassificaFasciaINRCorretta_Target2_3(
        double currentINR, double targetMin, double targetMax, FasciaINR expectedFascia)
    {
        // Act
        var result = _service.CalculateFCSA(
            (decimal)currentINR, (decimal)targetMin, (decimal)targetMax, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(expectedFascia);
    }

    [Theory]
    [InlineData(1.9, 2.5, 3.5, FasciaINR.SubCritico)]
    [InlineData(2.1, 2.5, 3.5, FasciaINR.SubModerato)]
    [InlineData(2.4, 2.5, 3.5, FasciaINR.SubLieve)]
    [InlineData(3.0, 2.5, 3.5, FasciaINR.InRange)]
    [InlineData(3.7, 2.5, 3.5, FasciaINR.SovraLieve)]
    [InlineData(4.2, 2.5, 3.5, FasciaINR.SovraModerato)]
    [InlineData(5.0, 2.5, 3.5, FasciaINR.SovraAlto)]
    [InlineData(6.0, 2.5, 3.5, FasciaINR.SovraMoltoAlto)]
    [InlineData(7.5, 2.5, 3.5, FasciaINR.SovraCritico)]
    [InlineData(9.5, 2.5, 3.5, FasciaINR.SovraEstremo)]
    public void CalculateFCSA_ClassificaFasciaINRCorretta_Target2_5_3_5(
        double currentINR, double targetMin, double targetMax, FasciaINR expectedFascia)
    {
        // Act
        var result = _service.CalculateFCSA(
            (decimal)currentINR, (decimal)targetMin, (decimal)targetMax, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(expectedFascia);
    }

    #endregion

    #region FCSA Tests - INR Sottoterapeutico

    [Fact]
    public void CalculateFCSA_SubLieve_IncrementoCorretto()
    {
        // Arrange - INR 1.85 (SubLieve target 2-3)
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            1.85m, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SubLieve);
        result.PercentageAdjustment.Should().Be(7.5m); // +5-10%
        result.SuggestedWeeklyDoseMg.Should().BeApproximately(37.5m, 0.5m); // 35 * 1.075
        result.DoseSupplementarePrimoGiorno.Should().BeNull();
        result.NextControlDays.Should().Be(12);
        result.RequiresEBPM.Should().BeFalse();
        result.UrgencyLevel.Should().Be(UrgencyLevel.Routine);
    }

    [Fact]
    public void CalculateFCSA_SubModerato_ConDoseCarico()
    {
        // Arrange - INR 1.6 (SubModerato)
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            1.6m, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, isSlowMetabolizer: false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SubModerato);
        result.PercentageAdjustment.Should().Be(11.25m); // 7.5-15% (11.25 se non slow)
        result.DoseSupplementarePrimoGiorno.Should().BeApproximately(1.75m, 0.01m); // 5% di 35 = 1.75
        result.SuggestedWeeklyDoseMg.Should().Be(40.0m); // 35 * 1.1125 = 38.9375 → arrotondato a 40mg (multiplo di 2.5)
        result.NextControlDays.Should().Be(8);
        result.UrgencyLevel.Should().Be(UrgencyLevel.Routine);
    }

    [Fact]
    public void CalculateFCSA_SubModerato_SlowMetabolizer_IncrementoConservativo()
    {
        // Arrange
        decimal currentDose = 12.5m; // Dose bassa

        // Act
        var result = _service.CalculateFCSA(
            1.6m, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, isSlowMetabolizer: true);

        // Assert
        result.PercentageAdjustment.Should().Be(7.5m); // Più conservativo per slow metabolizer
        result.Warnings.Should().Contain(w => w.Contains("METABOLIZZATORE LENTO"));
    }

    [Fact]
    public void CalculateFCSA_SubCritico_RischioAlto_RichiedeEBPM()
    {
        // Arrange - INR <1.5, rischio alto (protesi meccanica)
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            1.3m, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Nessuna, SedeEmorragia.Nessuna,
            hasProtesiMeccanica: true); // Rischio alto automatico

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SubCritico);
        result.PercentageAdjustment.Should().Be(17.5m); // 15-20%
        result.DoseSupplementarePrimoGiorno.Should().BeApproximately(2.625m, 0.1m); // 7.5% di 35
        result.RequiresEBPM.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("EBPM"));
        result.NextControlDays.Should().Be(6);
        result.UrgencyLevel.Should().Be(UrgencyLevel.Urgente);
    }

    [Fact]
    public void CalculateFCSA_SubCritico_TEVRecente_RischioAlto()
    {
        // Arrange - TEV <3 mesi = rischio alto
        var dataUltimoTEV = DateTime.Now.AddDays(-60); // 2 mesi fa

        // Act
        var result = _service.CalculateFCSA(
            1.3m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Nessuna, SedeEmorragia.Nessuna,
            hasProtesiMeccanica: false,
            dataUltimoTEV: dataUltimoTEV,
            indicazioneTAO: "TEV");

        // Assert
        result.RequiresEBPM.Should().BeTrue("TEV recente <90 giorni dovrebbe essere rischio alto");
    }

    #endregion

    #region FCSA Tests - INR Sovraterapeutico SENZA Emorragia

    [Fact]
    public void CalculateFCSA_SovraLieve_RiduzioneModesta()
    {
        // Arrange - INR 3.2 (SovraLieve)
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            3.2m, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SovraLieve);
        result.PercentageAdjustment.Should().Be(-7.5m);
        result.SuggestedWeeklyDoseMg.Should().BeApproximately(32.5m, 0.5m); // 35 * 0.925
        result.SospensioneDosi.Should().Be(0);
        result.RequiresVitaminK.Should().BeFalse();
        result.NextControlDays.Should().Be(10);
        result.UrgencyLevel.Should().Be(UrgencyLevel.Routine);
    }

    [Fact]
    public void CalculateFCSA_SovraMoltoAlto_VitaminaKOpzionale()
    {
        // Arrange - INR 5.5 (SovraMoltoAlto)
        var result = _service.CalculateFCSA(
            5.5m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SovraMoltoAlto);
        result.SospensioneDosi.Should().Be(2); // 1-2 dosi
        result.RequiresVitaminK.Should().BeTrue(); // Opzionale ma raccomandato
        result.VitaminKDoseMg.Should().Be(2m);
        result.VitaminKRoute.Should().Be("Orale");
        result.UrgencyLevel.Should().Be(UrgencyLevel.Urgente);
        result.Warnings.Should().Contain(w => w.Contains("24h"));
    }

    [Fact]
    public void CalculateFCSA_SovraCritico_VitaminaKObbligatoria()
    {
        // Arrange - INR 7.0 (SovraCritico)
        var result = _service.CalculateFCSA(
            7.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SovraCritico);
        result.SospensioneDosi.Should().Be(3);
        result.RequiresVitaminK.Should().BeTrue(); // FCSA raccomanda
        result.VitaminKDoseMg.Should().Be(2.5m);
        result.NextControlDays.Should().Be(1);
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
    }

    [Fact]
    public void CalculateFCSA_SovraEstremo_Emergenza()
    {
        // Arrange - INR 9.0 (SovraEstremo)
        var result = _service.CalculateFCSA(
            9.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.SovraEstremo);
        result.SospensioneDosi.Should().BeNull(); // Sospendi fino a INR OK
        result.PercentageAdjustment.Should().Be(-35m);
        result.RequiresVitaminK.Should().BeTrue();
        result.VitaminKDoseMg.Should().Be(4m); // 3-5mg FCSA
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
        result.Warnings.Should().Contain(w => w.Contains("EMERGENZA"));
    }

    #endregion

    #region FCSA Tests - INR Sovraterapeutico CON Emorragia

    [Fact]
    public void CalculateFCSA_EmorragiaMinore_STOPPlusVitK()
    {
        // Arrange - INR 6.0 con emorragia minore
        var result = _service.CalculateFCSA(
            6.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Minore,
            SedeEmorragia.Nasale);

        // Assert
        result.TipoEmorragia.Should().Be(TipoEmorragia.Minore);
        result.LoadingDoseAction.Should().Contain("STOP");
        result.RequiresVitaminK.Should().BeTrue();
        result.VitaminKDoseMg.Should().Be(2m); // INR 5-8
        result.VitaminKRoute.Should().Be("Orale");
        result.RequiresHospitalization.Should().BeFalse(); // Valutare
        result.RequiresPCC.Should().BeFalse();
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
        result.NextControlDays.Should().Be(1);
    }

    [Fact]
    public void CalculateFCSA_EmorragiaMaggiore_PCCObbligatorio()
    {
        // Arrange - INR 7.0 con emorragia maggiore GI
        var result = _service.CalculateFCSA(
            7.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Maggiore,
            SedeEmorragia.Gastrointestinale);

        // Assert
        result.TipoEmorragia.Should().Be(TipoEmorragia.Maggiore);
        result.RequiresVitaminK.Should().BeTrue();
        result.VitaminKDoseMg.Should().Be(10m);
        result.VitaminKRoute.Should().Be("EV lenta (10-20 min)");
        result.RequiresPCC.Should().BeTrue();
        result.DosePCC.Should().Contain("20-50 UI/kg");
        result.RequiresPlasma.Should().BeTrue(); // Alternativa
        result.DosePlasma.Should().Contain("15 mL/kg");
        result.RequiresHospitalization.Should().BeTrue();
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
        result.Warnings.Should().Contain(w => w.Contains("RICOVERO"));
    }

    [Fact]
    public void CalculateFCSA_EmorragiaRischioVitale_Intracranica_TerapiaIntensiva()
    {
        // Arrange - INR 5.0 con emorragia intracranica
        var result = _service.CalculateFCSA(
            5.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.RischioVitale,
            SedeEmorragia.Intracranica);

        // Assert
        result.TipoEmorragia.Should().Be(TipoEmorragia.RischioVitale);
        result.RequiresPCC.Should().BeTrue();
        result.DosePCC.Should().Contain("PRIMA SCELTA");
        result.RequiresHospitalization.Should().BeTrue();
        result.SuggestedWeeklyDoseMg.Should().Be(0m); // STOP indefinito
        result.PercentageAdjustment.Should().Be(-100m);
        result.NextControlDays.Should().Be(0); // Monitoraggio continuo
        result.Warnings.Should().Contain(w => w.Contains("RICOVERO TERAPIA INTENSIVA"));
        result.Warnings.Should().Contain(w => w.Contains("Imaging cerebrale"));
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
    }

    #endregion

    #region ACCP Tests - Differenze con FCSA

    [Fact]
    public void CalculateACCP_INR7_NoVitaminaK_DifferenzaFCSA()
    {
        // ACCP: Vitamina K solo se INR ≥10
        // FCSA: Vitamina K se INR >6

        // Act FCSA
        var resultFCSA = _service.CalculateFCSA(
            7.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Act ACCP
        var resultACCP = _service.CalculateACCP(
            7.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert - FCSA richiede Vit K, ACCP NO
        resultFCSA.RequiresVitaminK.Should().BeTrue("FCSA richiede Vit K per INR >6");
        resultACCP.RequiresVitaminK.Should().BeFalse("ACCP NO Vit K sotto INR 10");
        resultACCP.ClinicalNotes.Should().Contain("NO routine Vitamin K");
    }

    [Fact]
    public void CalculateACCP_INR10_RichiedeVitaminaK()
    {
        // Arrange - INR ≥10: anche ACCP richiede Vit K
        var result = _service.CalculateACCP(
            10.5m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.RequiresVitaminK.Should().BeTrue();
        result.VitaminKDoseMg.Should().Be(5m); // 2.5-5mg ACCP
        result.UrgencyLevel.Should().Be(UrgencyLevel.Emergenza);
    }

    [Fact]
    public void CalculateACCP_AggiustamentiPiuConservativi()
    {
        // ACCP usa aggiustamenti più conservativi

        // Act
        var resultFCSA = _service.CalculateFCSA(
            3.2m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        var resultACCP = _service.CalculateACCP(
            3.2m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert - ACCP riduzioni minori
        Math.Abs(resultACCP.PercentageAdjustment).Should().BeLessThan(
            Math.Abs(resultFCSA.PercentageAdjustment),
            "ACCP dovrebbe essere più conservativo");
    }

    [Fact]
    public void CalculateACCP_EmorragiaMaggiore_ConvergeConFCSA()
    {
        // Con emorragia, FCSA e ACCP convergono

        // Act
        var resultFCSA = _service.CalculateFCSA(
            6.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Maggiore);

        var resultACCP = _service.CalculateACCP(
            6.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Maggiore);

        // Assert - Stessa gestione
        resultFCSA.RequiresPCC.Should().Be(resultACCP.RequiresPCC);
        resultFCSA.VitaminKDoseMg.Should().Be(resultACCP.VitaminKDoseMg);
        resultACCP.FonteRaccomandazione.Should().Contain("ACCP/FCSA");
    }

    #endregion

    #region FCSA Tests - INR in Range

    [Fact]
    public void CalculateFCSA_INRInRange_MantieneDose()
    {
        // Arrange
        decimal currentINR = 2.5m;
        decimal currentDose = 35m;

        // Act
        var result = _service.CalculateFCSA(
            currentINR, 2.0m, 3.0m, currentDose,
            TherapyPhase.Maintenance, true, false);

        // Assert
        result.FasciaINR.Should().Be(FasciaINR.InRange);
        result.IsInRange.Should().BeTrue();
        result.SuggestedWeeklyDoseMg.Should().Be(currentDose);
        result.PercentageAdjustment.Should().Be(0);
        result.NextControlDays.Should().BeGreaterOrEqualTo(28); // Manutenzione stabile
        result.UrgencyLevel.Should().Be(UrgencyLevel.Routine);
    }

    #endregion

    #region GenerateWeeklySchedule Tests

    [Theory]
    [InlineData(17.5)] // 2.5mg × 7 giorni
    [InlineData(35.0)] // 5mg × 7 giorni
    [InlineData(30.0)] // Mix
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
                                             w.Contains("4D"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateFCSA_InvalidINR_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.CalculateFCSA(-1m, 2.0m, 3.0m, 35m, TherapyPhase.Maintenance, true, false));
    }

    [Fact]
    public void CalculateFCSA_InvalidTargetRange_ThrowsException()
    {
        // Act & Assert - Target max < target min
        Assert.Throws<ArgumentException>(() =>
            _service.CalculateFCSA(2.5m, 3.0m, 2.0m, 35m, TherapyPhase.Maintenance, true, false));
    }

    [Fact]
    public void GenerateWeeklySchedule_ZeroDose_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _service.GenerateWeeklySchedule(0m));
    }

    #endregion

    #region Calcolo Rischio Tromboembolico Tests

    [Fact]
    public void CalculateFCSA_CHA2DS2VAScAlto_RischioAlto()
    {
        // Arrange - FA con CHA2DS2-VASc ≥4
        var result = _service.CalculateFCSA(
            1.3m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false,
            ThromboembolicRisk.Moderate,
            TipoEmorragia.Nessuna, SedeEmorragia.Nessuna,
            hasProtesiMeccanica: false,
            dataUltimoTEV: null,
            indicazioneTAO: "FA",
            cha2ds2vasc: 5); // ≥4 = alto rischio

        // Assert
        result.RequiresEBPM.Should().BeTrue("CHA2DS2-VASc ≥4 dovrebbe dare rischio alto");
    }

    #endregion

    #region Fonte Raccomandazione Tests

    [Fact]
    public void CalculateFCSA_SetsFonteRaccomandazione()
    {
        var result = _service.CalculateFCSA(
            2.5m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        result.FonteRaccomandazione.Should().Be("FCSA");
    }

    [Fact]
    public void CalculateACCP_SetsFonteRaccomandazione()
    {
        var result = _service.CalculateACCP(
            2.5m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        result.FonteRaccomandazione.Should().Be("ACCP");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void CalculateFCSA_LogsWithFasciaAndUrgency()
    {
        // Arrange & Act
        var result = _service.CalculateFCSA(
            7.0m, 2.0m, 3.0m, 35m,
            TherapyPhase.Maintenance, true, false);

        // Assert - Verifica che il logging sia stato chiamato
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Fascia")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}

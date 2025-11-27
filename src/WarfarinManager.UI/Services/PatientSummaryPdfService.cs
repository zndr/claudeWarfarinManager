using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WarfarinManager.Data.Entities;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per la generazione di PDF del riassunto paziente
/// </summary>
public class PatientSummaryPdfService
{
    // Colori corporate
    private static readonly string PrimaryBlue = "#0078D4";
    private static readonly string SuccessGreen = "#107C10";
    private static readonly string WarningOrange = "#FFB900";
    private static readonly string DangerRed = "#D13438";
    private static readonly string LightGray = "#F5F5F5";
    private static readonly string DarkGray = "#333333";

    /// <summary>
    /// Genera il PDF del riassunto paziente
    /// </summary>
    public void GeneratePdf(
        string filePath,
        Patient patient,
        PreTaoAssessment? assessment,
        Indication? indication,
        List<INRControlSummary> recentControls,
        List<BridgeTherapySummary> bridgeTherapies,
        double currentTTR,
        string ttrQualityLevel,
        int totalControlsCount,
        int daysInTherapy)
    {
        // Configura QuestPDF per uso community (gratuito)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                page.Header().Element(c => ComposeHeader(c, patient));
                page.Content().Element(c => ComposeContent(c, patient, assessment, indication,
                    recentControls, bridgeTherapies, currentTTR, ttrQualityLevel,
                    totalControlsCount, daysInTherapy));
                page.Footer().Element(ComposeFooter);
            });
        });

        document.GeneratePdf(filePath);
    }

    private void ComposeHeader(IContainer container, Patient patient)
    {
        container.Column(column =>
        {
            // Titolo principale
            column.Item().Background(PrimaryBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("RIASSUNTO CLINICO PAZIENTE")
                        .FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("Terapia Anticoagulante Orale con Warfarin")
                        .FontSize(11).FontColor(Colors.White).Italic();
                });

                row.ConstantItem(100).AlignRight().AlignMiddle().Text("TaoGEST")
                    .FontSize(10).FontColor(Colors.White);
            });

            column.Item().Height(15);

            // Info paziente
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Paziente: ").SemiBold();
                        text.Span($"{patient.LastName} {patient.FirstName}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Codice Fiscale: ").SemiBold();
                        text.Span(patient.FiscalCode ?? "N/D");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Data di nascita: ").SemiBold();
                        text.Span($"{patient.BirthDate:dd/MM/yyyy} ({CalculateAge(patient.BirthDate)} anni)");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Sesso: ").SemiBold();
                        text.Span(patient.Gender == Shared.Enums.Gender.Male ? "M" : "F");
                    });
                });
            });
        });
    }

    private void ComposeContent(
        IContainer container,
        Patient patient,
        PreTaoAssessment? assessment,
        Indication? indication,
        List<INRControlSummary> recentControls,
        List<BridgeTherapySummary> bridgeTherapies,
        double currentTTR,
        string ttrQualityLevel,
        int totalControlsCount,
        int daysInTherapy)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Sezione Valutazione Pre-TAO
            if (assessment != null)
            {
                column.Item().Element(c => ComposePreTaoAssessment(c, assessment));
                column.Item().Height(15);
            }

            // Sezione Indicazione e TTR
            if (indication != null)
            {
                column.Item().Element(c => ComposeIndicationAndTTR(c, indication,
                    currentTTR, ttrQualityLevel, totalControlsCount, daysInTherapy));
                column.Item().Height(15);
            }

            // Sezione Ultimi Controlli INR
            if (recentControls.Any())
            {
                column.Item().Element(c => ComposeRecentControls(c, recentControls));
                column.Item().Height(15);
            }

            // Sezione Bridge Therapy
            if (bridgeTherapies.Any())
            {
                column.Item().Element(c => ComposeBridgeTherapies(c, bridgeTherapies));
            }
        });
    }

    private void ComposePreTaoAssessment(IContainer container, PreTaoAssessment assessment)
    {
        container.Column(column =>
        {
            // Titolo sezione
            column.Item().Background(LightGray).Padding(8).Text("🩺 VALUTAZIONE PRE-TAO")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(content =>
            {
                // Scores
                content.Item().Row(row =>
                {
                    row.RelativeItem().Border(2).BorderColor(PrimaryBlue)
                        .Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().AlignCenter().Text("CHA₂DS₂-VASc")
                                .FontSize(10).SemiBold().FontColor(PrimaryBlue);
                            col.Item().AlignCenter().Text(assessment.CHA2DS2VAScScore.ToString())
                                .FontSize(24).Bold().FontColor(PrimaryBlue);
                        });

                    row.ConstantItem(15);

                    row.RelativeItem().Border(2).BorderColor(DangerRed)
                        .Background(Colors.Red.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().AlignCenter().Text("HAS-BLED")
                                .FontSize(10).SemiBold().FontColor(DangerRed);
                            col.Item().AlignCenter().Text(assessment.HASBLEDScore.ToString())
                                .FontSize(24).Bold().FontColor(DangerRed);
                        });
                });

                content.Item().PaddingVertical(10).Text(text =>
                {
                    text.Span("Valutazione complessiva: ").SemiBold();
                    text.Span(assessment.OverallAssessment);
                });

                if (!string.IsNullOrEmpty(assessment.Recommendations))
                {
                    content.Item().Background(Colors.Yellow.Lighten4)
                        .Padding(8).Text(text =>
                        {
                            text.Span("Raccomandazioni: ").SemiBold().FontColor(WarningOrange);
                            text.Span(assessment.Recommendations).Italic();
                        });
                }
            });
        });
    }

    private void ComposeIndicationAndTTR(
        IContainer container,
        Indication indication,
        double currentTTR,
        string ttrQualityLevel,
        int totalControlsCount,
        int daysInTherapy)
    {
        container.Column(column =>
        {
            // Titolo sezione
            column.Item().Background(LightGray).Padding(8).Text("🎯 TERAPIA ANTICOAGULANTE")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(content =>
            {
                // Indicazione
                content.Item().Text(text =>
                {
                    text.Span("Indicazione: ").SemiBold();
                    text.Span(indication.IndicationType?.Description ?? "Non specificata");
                });

                content.Item().Text(text =>
                {
                    text.Span("Inizio terapia: ").SemiBold();
                    text.Span($"{indication.StartDate:dd/MM/yyyy}");
                });

                content.Item().Text(text =>
                {
                    text.Span("Range INR target: ").SemiBold();
                    text.Span($"{indication.TargetINRMin:F1} - {indication.TargetINRMax:F1}");
                });

                content.Item().PaddingTop(10).Border(2).BorderColor(PrimaryBlue)
                    .Background(Colors.Blue.Lighten4).Padding(12).Column(ttrCol =>
                    {
                        ttrCol.Item().Text("TTR (Time in Therapeutic Range)")
                            .FontSize(12).SemiBold().FontColor(PrimaryBlue);

                        ttrCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"{currentTTR:F1}%")
                                .FontSize(24).Bold().FontColor(PrimaryBlue);

                            row.ConstantItem(10);

                            var qualityColor = ttrQualityLevel switch
                            {
                                "Ottimo" => SuccessGreen,
                                "Buono" => PrimaryBlue,
                                "Accettabile" => WarningOrange,
                                _ => DangerRed
                            };

                            row.AutoItem().Background(qualityColor).PaddingVertical(3).PaddingHorizontal(6)
                                .AlignMiddle().Text(ttrQualityLevel)
                                .FontSize(11).SemiBold().FontColor(Colors.White);
                        });

                        ttrCol.Item().Text($"{totalControlsCount} controlli in {daysInTherapy} giorni di terapia")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
            });
        });
    }

    private void ComposeRecentControls(IContainer container, List<INRControlSummary> controls)
    {
        container.Column(column =>
        {
            // Titolo sezione
            column.Item().Background(LightGray).Padding(8).Text("📊 ULTIMI CONTROLLI INR")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                // Definisci colonne
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Data
                    columns.ConstantColumn(50);  // INR
                    columns.RelativeColumn();    // Dose
                    columns.RelativeColumn();    // Variazione
                    columns.ConstantColumn(40);  // In range
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Data").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("INR").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Dose sett. (mg)").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Variazione").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter()
                        .Text("Range").FontSize(10).SemiBold();
                });

                // Righe dati
                foreach (var control in controls.Take(10))
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{control.ControlDate:dd/MM/yy}").FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{control.INRValue:F2}").FontSize(9).SemiBold().FontColor(PrimaryBlue);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{control.WeeklyDose:F2}").FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text(control.DosageChangeText).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter()
                        .Text(control.IsInRange ? "✓" : "✗").FontSize(11)
                        .FontColor(control.IsInRange ? SuccessGreen : DangerRed);
                }
            });
        });
    }

    private void ComposeBridgeTherapies(IContainer container, List<BridgeTherapySummary> bridges)
    {
        container.Column(column =>
        {
            // Titolo sezione
            column.Item().Background(LightGray).Padding(8).Text("💉 BRIDGE THERAPY ESEGUITE")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                // Definisci colonne
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Data
                    columns.RelativeColumn(2);   // Tipo intervento
                    columns.RelativeColumn();    // Rischio
                    columns.ConstantColumn(60);  // Bridge
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Data").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Tipo intervento").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                        .Text("Rischio").FontSize(10).SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter()
                        .Text("Bridge").FontSize(10).SemiBold();
                });

                // Righe dati
                foreach (var bridge in bridges)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text($"{bridge.SurgeryDate:dd/MM/yy}").FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text(bridge.SurgeryType).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                        .Text(bridge.ThromboembolicRisk).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter()
                        .Text(bridge.BridgeRecommended ? "Sì" : "No").FontSize(9)
                        .FontColor(bridge.BridgeRecommended ? SuccessGreen : DarkGray);
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Documento generato il ").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            text.Span(" - TaoGEST").FontSize(9).FontColor(Colors.Grey.Medium).Italic();
        });
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }
}

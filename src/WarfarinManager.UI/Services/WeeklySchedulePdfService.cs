using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WarfarinManager.Data.Context;
using WarfarinManager.Data.Entities;
using WarfarinManager.Core.Models;
using WarfarinManager.UI.Helpers;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per la generazione di PDF dello schema posologico settimanale
/// </summary>
public class WeeklySchedulePdfService
{
    private readonly WarfarinDbContext _context;

    // Colori corporate
    private static readonly string PrimaryBlue = "#0078D4";
    private static readonly string SuccessGreen = "#107C10";
    private static readonly string WarningOrange = "#FFB900";
    private static readonly string DangerRed = "#D13438";
    private static readonly string LightGray = "#F5F5F5";
    private static readonly string DarkGray = "#333333";

    public WeeklySchedulePdfService(WarfarinDbContext context)
    {
        _context = context;
    }

    public async Task GeneratePdfAsync(
        string filePath,
        Patient patient,
        string patientFiscalCode,
        string activeIndication,
        decimal targetINRMin,
        decimal targetINRMax,
        DateTime controlDate,
        decimal inrValue,
        string inrStatusText,
        decimal currentWeeklyDose,
        decimal[] currentSchedule,
        string[] currentScheduleDescriptions,
        DosageSuggestionResult suggestion,
        decimal[] suggestedSchedule,
        string suggestedScheduleText,
        string selectedGuideline,
        DosageSuggestionResult? fcsaSuggestion,
        DosageSuggestionResult? accpSuggestion,
        bool isWarningIgnored = false)
    {
        // Assicurati che la directory di destinazione esista
        var directory = System.IO.Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Carica i dati del medico
        var doctorData = await _context.DoctorData.FirstOrDefaultAsync();

        // Configura QuestPDF per uso community (gratuito)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                page.Header().Element(c => ComposeHeader(c, patient, patientFiscalCode, activeIndication, targetINRMin, targetINRMax));
                page.Content().Element(c => ComposeContent(c, controlDate, inrValue, inrStatusText,
                    targetINRMin, targetINRMax, currentWeeklyDose, currentSchedule, currentScheduleDescriptions,
                    suggestion, suggestedSchedule, suggestedScheduleText, selectedGuideline,
                    fcsaSuggestion, accpSuggestion, isWarningIgnored));
                page.Footer().Element(c => ComposeFooter(c, doctorData));
            });
        });

        document.GeneratePdf(filePath);
    }

    private void ComposeHeader(IContainer container, Patient patient, string patientFiscalCode,
        string activeIndication, decimal targetINRMin, decimal targetINRMax)
    {
        container.Column(column =>
        {
            // Titolo principale
            column.Item().Background(PrimaryBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("VALUTAZIONE INR E DOSAGGIO")
                        .FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("Warfarin (Coumadin) - Terapia Anticoagulante Orale")
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
                        text.Span(patientFiscalCode ?? "N/D");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Indicazione: ").SemiBold();
                        text.Span(activeIndication);
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Target INR: ").SemiBold();
                        text.Span($"{targetINRMin:F1} - {targetINRMax:F1}");
                    });
                });
            });
        });
    }

    private void ComposeContent(IContainer container, DateTime controlDate, decimal inrValue,
        string inrStatusText, decimal targetINRMin, decimal targetINRMax, decimal currentWeeklyDose,
        decimal[] currentSchedule, string[] currentScheduleDescriptions, DosageSuggestionResult suggestion,
        decimal[] suggestedSchedule, string suggestedScheduleText, string selectedGuideline,
        DosageSuggestionResult? fcsaSuggestion, DosageSuggestionResult? accpSuggestion, bool isWarningIgnored)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Controllo INR
            column.Item().Background(LightGray).Padding(8).Text($"CONTROLLO INR - {controlDate:dd/MM/yyyy}")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);

            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(content =>
            {
                content.Item().Text(text =>
                {
                    text.Span("INR Rilevato: ").SemiBold();
                    text.Span($"{inrValue:F1}").FontSize(16).Bold().FontColor(PrimaryBlue);
                    text.Span($"  ({inrStatusText})").FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                var targetMid = (targetINRMin + targetINRMax) / 2;
                var deviation = inrValue - targetMid;
                content.Item().Text(text =>
                {
                    text.Span("Scostamento rispetto all'INR target: ").SemiBold();
                    text.Span($"{deviation:+0.0;-0.0}").FontColor(deviation > 0 ? DangerRed : SuccessGreen);
                });

                content.Item().Height(10);
            });

            column.Item().Height(15);

            // Suggerimento dosaggio
            column.Item().Background(PrimaryBlue).Padding(8).Text($"NUOVO DOSAGGIO SUGGERITO (Linee Guida {selectedGuideline})")
                .FontSize(14).SemiBold().FontColor(Colors.White);

            column.Item().Border(2).BorderColor(PrimaryBlue).Padding(12).Column(content =>
            {
                // Avviso sospensione dose (solo se NON ignorati i warning)
                if (!isWarningIgnored && (suggestion.SospensioneDosi != null || suggestion.SospensioneDosi > 0))
                {
                    var suspensionText = suggestion.SospensioneDosi == null
                        ? "Sospendere warfarin fino a INR rientrato nel range"
                        : suggestion.SospensioneDosi == 1
                            ? "Considerare saltare 1 dose"
                            : $"Saltare {suggestion.SospensioneDosi} dosi";

                    content.Item().Background(Colors.Yellow.Lighten2).Padding(8).Text(text =>
                    {
                        text.Span("⚠ AZIONE IMMEDIATA: ").SemiBold().FontColor(WarningOrange);
                        text.Span(suspensionText);
                    });
                    content.Item().Height(10);
                }

                // Azione immediata (dose di carico) - solo se NON ignorati i warning
                if (!isWarningIgnored && !string.IsNullOrEmpty(suggestion.LoadingDoseAction))
                {
                    content.Item().Background(Colors.Yellow.Lighten3).Padding(8).Text(text =>
                    {
                        text.Span("⚠ AZIONE IMMEDIATA: ").SemiBold().FontColor(WarningOrange);
                        text.Span(suggestion.LoadingDoseAction);
                    });
                    content.Item().Height(10);
                }

                // Verifica se il dosaggio è cambiato
                bool doseChanged = Math.Abs(currentWeeklyDose - suggestion.SuggestedWeeklyDoseMg) > 0.1m;

                if (doseChanged)
                {
                    // Calcola la dose settimanale effettiva dallo schema suggerito
                    decimal actualSuggestedWeeklyDose = suggestedSchedule.Sum();

                    // Calcola la percentuale di aggiustamento rispetto al dosaggio corrente
                    decimal actualPercentageAdjustment = currentWeeklyDose > 0
                        ? ((actualSuggestedWeeklyDose - currentWeeklyDose) / currentWeeklyDose * 100)
                        : 0;

                    content.Item().Text(text =>
                    {
                        text.Span("Nuova dose settimanale: ").SemiBold();
                        text.Span($"{actualSuggestedWeeklyDose:F1} mg").FontSize(16).Bold().FontColor(PrimaryBlue);
                        text.Span($" ({actualPercentageAdjustment:+0.0;-0.0}%)").FontColor(Colors.Grey.Darken1);
                    });

                    content.Item().Height(15);

                    // Prova ad affiancare gli schemi (nuovo a sinistra, precedente a destra)
                    content.Item().Row(row =>
                    {
                        // Schema NUOVO (sinistra)
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("NUOVO SCHEMA SETTIMANALE")
                                .SemiBold().FontSize(12).FontColor(SuccessGreen);
                            col.Item().Height(5);
                            col.Item().Element(c => ComposeWeeklyScheduleTable(c, suggestedSchedule));
                        });

                        row.ConstantItem(15); // Spaziatura tra le colonne

                        // Schema PRECEDENTE (destra)
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SCHEMA PRECEDENTE")
                                .SemiBold().FontSize(12).FontColor(Colors.Grey.Darken1);
                            col.Item().Height(5);
                            col.Item().Element(c => ComposeWeeklyScheduleTable(c, currentSchedule));
                        });
                    });
                }
                else
                {
                    // Dosaggio invariato - mostra solo schema nuovo con messaggio verde
                    content.Item().Background(Colors.Green.Lighten4).Padding(12).Text(text =>
                    {
                        text.Span("✓ NESSUNA MODIFICA DEL DOSAGGIO È NECESSARIA")
                            .Bold().FontSize(14).FontColor(SuccessGreen);
                    });

                    content.Item().Height(15);
                    content.Item().Text("Schema settimanale (mantenere invariato):")
                        .SemiBold().FontSize(12);
                    content.Item().Height(5);

                    content.Item().Element(c => ComposeWeeklyScheduleTable(c, suggestedSchedule));
                }

                content.Item().Height(15);
                content.Item().Text(text =>
                {
                    text.Span("Prossimo controllo: ").SemiBold().FontColor(PrimaryBlue).FontSize(12);
                    text.Span($"{controlDate.AddDays(suggestion.NextControlDays):dd/MM/yyyy}").FontColor(PrimaryBlue).FontSize(12);
                    text.Span($" (tra {suggestion.NextControlDays} giorni)").FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().Height(15);

            // Note cliniche
            if (!string.IsNullOrEmpty(suggestion.ClinicalNotes))
            {
                column.Item().Background(LightGray).Padding(8).Text("NOTE CLINICHE")
                    .FontSize(14).SemiBold().FontColor(PrimaryBlue);

                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12)
                    .Text(suggestion.ClinicalNotes).FontSize(11);

                column.Item().Height(15);
            }
        });
    }

    private void ComposeWeeklyScheduleTable(IContainer container, decimal[] schedule)
    {
        var days = new[] { "Lunedì", "Martedì", "Mercoledì", "Giovedì", "Venerdì", "Sabato", "Domenica" };

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);  // Giorno
                columns.ConstantColumn(60);  // Dose mg
                columns.RelativeColumn();    // Compresse (sempre presente)
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text("Giorno").FontSize(10).SemiBold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text("Dose (mg)").FontSize(10).SemiBold();
                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text("Compresse").FontSize(10).SemiBold();
            });

            // Righe
            for (int i = 0; i < 7; i++)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                    .Text(days[i]).FontSize(10);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                    .Text($"{FormatDoseMg(schedule[i])}").FontSize(10).SemiBold().FontColor(PrimaryBlue);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                    .Text($"{DoseDistributionHelper.FormatAsTablets(schedule[i])} cp").FontSize(9).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    /// <summary>
    /// Formatta il dosaggio in mg mostrando solo i decimali necessari
    /// </summary>
    private static string FormatDoseMg(decimal doseMg)
    {
        // Se è un numero intero, mostra senza decimali
        if (doseMg % 1 == 0)
            return doseMg.ToString("F0");

        // Se ha solo un decimale significativo (es: 2.5), mostra 1 decimale
        if ((doseMg * 10) % 10 == 0)
            return doseMg.ToString("F1");

        // Altrimenti mostra 2 decimali (es: 1.25)
        return doseMg.ToString("F2");
    }

    private void ComposeFooter(IContainer container, DoctorData? doctorData)
    {
        container.Column(column =>
        {
            // Dati del medico (se presenti)
            if (doctorData != null)
            {
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
                    .Background(LightGray).Padding(10).Column(doctorCol =>
                    {
                        doctorCol.Item().Text(text =>
                        {
                            text.Span("Medico: ").FontSize(9).SemiBold().FontColor(DarkGray);
                            text.Span($"dr. {doctorData.FullName}").FontSize(9).FontColor(DarkGray);
                        });

                        if (!string.IsNullOrWhiteSpace(doctorData.Street) ||
                            !string.IsNullOrWhiteSpace(doctorData.City))
                        {
                            doctorCol.Item().Text(text =>
                            {
                                if (!string.IsNullOrWhiteSpace(doctorData.Street))
                                {
                                    text.Span(doctorData.Street).FontSize(8).FontColor(Colors.Grey.Darken1);
                                    if (!string.IsNullOrWhiteSpace(doctorData.City))
                                        text.Span(", ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                                if (!string.IsNullOrWhiteSpace(doctorData.City))
                                {
                                    if (!string.IsNullOrWhiteSpace(doctorData.PostalCode))
                                        text.Span($"{doctorData.PostalCode} ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    text.Span(doctorData.City).FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(doctorData.Phone) ||
                            !string.IsNullOrWhiteSpace(doctorData.Email))
                        {
                            doctorCol.Item().Text(text =>
                            {
                                if (!string.IsNullOrWhiteSpace(doctorData.Phone))
                                {
                                    text.Span($"Tel: {doctorData.Phone}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    if (!string.IsNullOrWhiteSpace(doctorData.Email))
                                        text.Span(" - ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                                if (!string.IsNullOrWhiteSpace(doctorData.Email))
                                    text.Span($"Email: {doctorData.Email}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        }
                    });

                column.Item().Height(8);
            }

            // Info generazione documento
            column.Item().AlignCenter().Text(text =>
            {
                text.Span("Documento generato il ").FontSize(9).FontColor(Colors.Grey.Medium);
                text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                text.Span(" - TaoGEST").FontSize(9).FontColor(Colors.Grey.Medium).Italic();
            });
        });
    }

    /// <summary>
    /// Genera PDF per strumenti standalone (senza paziente)
    /// </summary>
    public async Task GenerateStandalonePdfAsync(
        string filePath,
        string title,
        decimal[] weeklySchedule,
        string? metadata = null)
    {
        // DEBUG: Mostra il percorso ricevuto
        System.Diagnostics.Debug.WriteLine($"PDF Path ricevuto: {filePath}");
        System.Diagnostics.Debug.WriteLine($"File path completo: {System.IO.Path.GetFullPath(filePath)}");

        // Assicurati che la directory di destinazione esista
        var directory = System.IO.Path.GetDirectoryName(filePath);
        System.Diagnostics.Debug.WriteLine($"Directory estratta: {directory}");
        System.Diagnostics.Debug.WriteLine($"Directory esiste: {System.IO.Directory.Exists(directory)}");

        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.Diagnostics.Debug.WriteLine($"Creazione directory: {directory}");
            System.IO.Directory.CreateDirectory(directory);
        }

        // Carica i dati del medico prima di creare il documento
        var doctorData = await _context.DoctorData.FirstOrDefaultAsync();

        QuestPDF.Settings.License = LicenseType.Community;

        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));  // Rimosso FontFamily per evitare problemi

                    page.Header().Element(c => ComposeStandaloneHeader(c, title));
                    page.Content().Element(c => ComposeStandaloneContent(c, weeklySchedule, metadata));
                    page.Footer().Element(c => ComposeFooter(c, doctorData));
                });
            });

            // Genera PDF usando FileStream per controllo totale
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                document.GeneratePdf(stream);
            }
        }
        catch (Exception ex)
        {
            // Rilancia con più dettagli per debug
            throw new Exception($"Errore durante la generazione del PDF '{filePath}': {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStackTrace: {ex.StackTrace}", ex);
        }
    }

    private void ComposeStandaloneHeader(IContainer container, string title)
    {
        container.Column(column =>
        {
            column.Item().Background(PrimaryBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Text(title)
                    .FontSize(18).Bold().FontColor(Colors.White);
                row.ConstantItem(100).AlignRight().Text("TaoGEST")
                    .FontSize(10).FontColor(Colors.White);
            });
        });
    }

    private void ComposeStandaloneContent(IContainer container, decimal[] schedule, string? metadata)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Metadata (parametri input)
            if (!string.IsNullOrEmpty(metadata))
            {
                column.Item().Background(LightGray).Padding(8).Text("PARAMETRI")
                    .FontSize(14).SemiBold().FontColor(PrimaryBlue);
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12)
                    .Text(metadata).FontSize(11);
                column.Item().Height(15);
            }

            // Tabella schema settimanale
            column.Item().Background(LightGray).Padding(8).Text("SCHEMA SETTIMANALE")
                .FontSize(14).SemiBold().FontColor(PrimaryBlue);
            column.Item().Element(c => ComposeWeeklyScheduleTable(c, schedule));

            // Totale
            decimal total = schedule.Sum();
            column.Item().Height(15);
            column.Item().Background(Colors.Green.Lighten4).Padding(10).Text(text =>
            {
                text.Span("Dose settimanale totale: ").SemiBold();
                text.Span($"{total:F1} mg").FontSize(16).Bold().FontColor(PrimaryBlue);
            });

            // Short schedule
            string shortSchedule = DoseDistributionHelper.GenerateShortSchedule(schedule);
            column.Item().Height(10);
            column.Item().Text(shortSchedule).FontSize(11).Italic();
        });
    }

}

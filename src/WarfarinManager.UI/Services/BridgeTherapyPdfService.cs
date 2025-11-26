using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WarfarinManager.Core.Services;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Servizio per la generazione di PDF per il protocollo Bridge Therapy
/// </summary>
public class BridgeTherapyPdfService
{
    // Colori corporate
    private static readonly string PrimaryBlue = "#0078D4";
    private static readonly string SuccessGreen = "#107C10";
    private static readonly string WarningOrange = "#FFB900";
    private static readonly string DangerRed = "#D13438";
    private static readonly string LightGray = "#F5F5F5";
    private static readonly string DarkGray = "#333333";
    
    public void GeneratePdf(string filePath, BridgeProtocol protocol, Patient patient, BridgeRecommendation recommendation)
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
                
                page.Header().Element(c => ComposeHeader(c, patient, protocol));
                page.Content().Element(c => ComposeContent(c, protocol, recommendation));
                page.Footer().Element(ComposeFooter);
            });
        });
        
        document.GeneratePdf(filePath);
    }
    
    private void ComposeHeader(IContainer container, Patient patient, BridgeProtocol protocol)
    {
        container.Column(column =>
        {
            // Titolo principale
            column.Item().Background(PrimaryBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PROTOCOLLO BRIDGE THERAPY")
                        .FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("Gestione perioperatoria terapia anticoagulante")
                        .FontSize(11).FontColor(Colors.White).Italic();
                });
                
                row.ConstantItem(120).AlignRight().AlignMiddle().Text("WarfarinManager Pro")
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
                    col.Item().Text(text =>
                    {
                        text.Span("Data di nascita: ").SemiBold();
                        text.Span($"{patient.BirthDate:dd/MM/yyyy} ({CalculateAge(patient.BirthDate)} anni)");
                    });
                });
                
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Data intervento: ").SemiBold();
                        text.Span($"{protocol.SurgeryDate:dd/MM/yyyy}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Tipo intervento: ").SemiBold();
                        text.Span(GetSurgeryTypeDescription(protocol.SurgeryType));
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Documento generato: ").SemiBold();
                        text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });
            });
            
            column.Item().Height(10);
        });
    }
    
    private void ComposeContent(IContainer container, BridgeProtocol protocol, BridgeRecommendation recommendation)
    {
        container.Column(column =>
        {
            // Box raccomandazione principale
            var bgColor = protocol.BridgeRecommended ? WarningOrange : SuccessGreen;
            var title = protocol.BridgeRecommended 
                ? "⚠ BRIDGE THERAPY RACCOMANDATO" 
                : "✓ BRIDGE THERAPY NON NECESSARIO";
            
            column.Item().Background(bgColor).Padding(15).Column(col =>
            {
                col.Item().AlignCenter().Text(title)
                    .FontSize(16).Bold().FontColor(Colors.White);
                col.Item().Height(5);
                col.Item().AlignCenter().Text(recommendation.Rationale)
                    .FontSize(11).FontColor(Colors.White);
            });
            
            column.Item().Height(15);
            
            // Valutazione rischio
            column.Item().Text("VALUTAZIONE DEL RISCHIO").FontSize(13).Bold().FontColor(PrimaryBlue);
            column.Item().Height(8);
            
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                });
                
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("Rischio Tromboembolico").SemiBold();
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.AutoItem().Height(20).Width(20).Background(GetRiskColor(protocol.TERisk.ToString()));
                        r.AutoItem().PaddingLeft(8).AlignMiddle().Text(GetRiskLabel(protocol.TERisk.ToString()));
                    });
                });
                
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("Rischio Emorragico").SemiBold();
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.AutoItem().Height(20).Width(20).Background(GetRiskColor(protocol.BleedingRisk.ToString()));
                        r.AutoItem().PaddingLeft(8).AlignMiddle().Text(GetRiskLabel(protocol.BleedingRisk.ToString()));
                    });
                });
            });
            
            column.Item().Height(15);
            
            // Timeline
            column.Item().Text("TIMELINE PERIOPERATORIA").FontSize(13).Bold().FontColor(PrimaryBlue);
            column.Item().Height(8);
            
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(timelineCol =>
            {
                // Pre-operatorio
                timelineCol.Item().Text("PRE-OPERATORIO").SemiBold().FontColor(DarkGray);
                timelineCol.Item().Height(8);
                
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "📌", $"Giorno -5 ({protocol.WarfarinStopDate:dd/MM})", "STOP Warfarin", false));
                
                if (protocol.BridgeRecommended && protocol.EBPMStartDate.HasValue)
                {
                    timelineCol.Item().Row(r => ComposeTimelineItem(r, "💉", $"Giorno -3 ({protocol.EBPMStartDate:dd/MM})", "Inizio EBPM", false));
                }
                
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "🔬", $"Giorno -1 ({protocol.SurgeryDate.AddDays(-1):dd/MM})", "Controllo INR (target <1.5)", false));
                
                if (protocol.BridgeRecommended && protocol.EBPMLastDoseDate.HasValue)
                {
                    timelineCol.Item().Row(r => ComposeTimelineItem(r, "💉", $"Giorno -1 ({protocol.EBPMLastDoseDate:dd/MM})", "Ultima dose EBPM (24h prima)", false));
                }
                
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "🏥", $"Giorno 0 ({protocol.SurgeryDate:dd/MM})", "*** INTERVENTO ***", true));
                
                timelineCol.Item().Height(12);
                
                // Post-operatorio
                timelineCol.Item().Text("POST-OPERATORIO").SemiBold().FontColor(DarkGray);
                timelineCol.Item().Height(8);
                
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "💊", $"Giorno 0 ({protocol.WarfarinResumeDate:dd/MM})", "Ripresa Warfarin (12-24h post)", false));
                
                if (protocol.BridgeRecommended && protocol.EBPMResumeDate.HasValue)
                {
                    timelineCol.Item().Row(r => ComposeTimelineItem(r, "💉", $"Giorno +1-2 ({protocol.EBPMResumeDate:dd/MM})", "Ripresa EBPM se emostasi OK", false));
                }
                
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "🔬", $"Giorno +5 ({protocol.SurgeryDate.AddDays(5):dd/MM})", "Controllo INR", false));
                timelineCol.Item().Row(r => ComposeTimelineItem(r, "✓", "INR ≥2.0 per 24h", "STOP EBPM", false));
            });
            
            // Schema EBPM (se raccomandato)
            if (protocol.BridgeRecommended)
            {
                column.Item().Height(15);
                column.Item().Text("SCHEMA EBPM").FontSize(13).Bold().FontColor("#F57C00");
                column.Item().Height(8);
                
                column.Item().Background("#FFF8E1").Border(1).BorderColor("#FFE082").Padding(12).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(100);
                        cols.RelativeColumn();
                    });
                    
                    table.Cell().Text("Farmaco:").SemiBold();
                    table.Cell().Text(protocol.EBPMDrug ?? "Enoxaparina");
                    
                    table.Cell().Text("Dosaggio:").SemiBold();
                    table.Cell().Text(protocol.EBPMDosage ?? "Da calcolare in base al peso");
                    
                    table.Cell().Text("Frequenza:").SemiBold();
                    table.Cell().Text(protocol.EBPMFrequency ?? "Ogni 12 ore");
                });
            }
            
            // Warnings
            if (protocol.Warnings?.Any() == true)
            {
                column.Item().Height(15);
                column.Item().Text("⚠ AVVERTENZE").FontSize(13).Bold().FontColor(DangerRed);
                column.Item().Height(8);
                
                foreach (var warning in protocol.Warnings)
                {
                    column.Item().Background("#FFEBEE").Border(1).BorderColor("#FFCDD2")
                        .Padding(10).Text(warning).FontSize(10).FontColor("#C62828");
                    column.Item().Height(5);
                }
            }
            
            // Note cliniche
            if (protocol.ClinicalNotes?.Any() == true)
            {
                column.Item().Height(15);
                column.Item().Text("NOTE CLINICHE").FontSize(13).Bold().FontColor(PrimaryBlue);
                column.Item().Height(8);
                
                column.Item().Background(LightGray).Padding(10).Column(notesCol =>
                {
                    foreach (var note in protocol.ClinicalNotes)
                    {
                        notesCol.Item().Text($"• {note}").FontSize(10);
                        notesCol.Item().Height(3);
                    }
                });
            }
        });
    }
    
    private void ComposeTimelineItem(RowDescriptor row, string icon, string date, string description, bool isHighlight)
    {
        row.ConstantItem(25).Text(icon).FontSize(12);
        row.ConstantItem(120).Text(date).FontSize(10).SemiBold();
        
        if (isHighlight)
        {
            row.RelativeItem().Text(description).FontSize(10).Bold().FontColor(DangerRed);
        }
        else
        {
            row.RelativeItem().Text(description).FontSize(10);
        }
    }
    
    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Height(10);
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().Height(10);
            
            // Disclaimer
            column.Item().Background("#FFF3E0").Padding(10).Column(col =>
            {
                col.Item().Text("DISCLAIMER").FontSize(9).Bold().FontColor("#E65100");
                col.Item().Height(3);
                col.Item().Text("Questo documento è uno strumento di supporto decisionale. " +
                    "Il medico prescrittore è SEMPRE responsabile della valutazione clinica finale e della decisione terapeutica. " +
                    "I calcoli devono essere verificati dal clinico prima dell'applicazione.")
                    .FontSize(8).FontColor("#E65100");
            });
            
            column.Item().Height(8);
            
            // Footer info
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Generato da ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span("WarfarinManager Pro").FontSize(8).Bold().FontColor(PrimaryBlue);
                    text.Span($" | {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                
                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" / ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });
    }
    
    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
    
    private static string GetRiskColor(string risk)
    {
        return risk.ToLower() switch
        {
            "low" => SuccessGreen,
            "moderate" => WarningOrange,
            "high" => DangerRed,
            _ => Colors.Grey.Medium
        };
    }
    
    private static string GetRiskLabel(string risk)
    {
        return risk.ToLower() switch
        {
            "low" => "Basso",
            "moderate" => "Moderato",
            "high" => "Alto",
            _ => risk
        };
    }
    
    private static string GetSurgeryTypeDescription(SurgeryType surgeryType)
    {
        return surgeryType switch
        {
            SurgeryType.DiagnosticEndoscopy => "Endoscopia diagnostica",
            SurgeryType.DiagnosticColonoscopy => "Colonscopia diagnostica",
            SurgeryType.CardiacCatheterization => "Cateterismo cardiaco",
            SurgeryType.Transesophageal => "Eco transesofagea",
            SurgeryType.DermatologySingleExcision => "Escissione cutanea minore",
            SurgeryType.Ophthalmology => "Cataratta/Laser oculare",
            SurgeryType.DentalSingleExtraction => "Estrazione dentaria singola",
            SurgeryType.EndoscopyWithBiopsy => "Endoscopia con biopsia",
            SurgeryType.Polypectomy => "Polipectomia",
            SurgeryType.Cardioversion => "Cardioversione elettrica",
            SurgeryType.PacemakerImplant => "Impianto pacemaker/ICD",
            SurgeryType.ArthroscopyMinor => "Artroscopia minore",
            SurgeryType.LaparoscopicCholecystectomy => "Colecistectomia laparoscopica",
            SurgeryType.TURP => "TURP/Cistoscopia con biopsia",
            SurgeryType.Neurosurgery => "Neurochirurgia",
            SurgeryType.CardiacSurgery => "Cardiochirurgia",
            SurgeryType.VascularSurgery => "Chirurgia vascolare maggiore",
            SurgeryType.ThoracicSurgery => "Chirurgia toracica",
            SurgeryType.AbdominalSurgery => "Chirurgia addominale maggiore",
            SurgeryType.HepaticSurgery => "Chirurgia epatica",
            SurgeryType.PancreaticSurgery => "Chirurgia pancreatica",
            SurgeryType.ProstateSurgery => "Chirurgia prostatica",
            SurgeryType.RenalSurgery => "Chirurgia renale",
            SurgeryType.MajorOrthopedic => "Chirurgia ortopedica maggiore",
            SurgeryType.EpiduralAnesthesia => "Anestesia neuroassiale",
            SurgeryType.OphthalmologySurgery => "Chirurgia oculistica maggiore",
            SurgeryType.DentalMajor => "Odontoiatria maggiore",
            _ => surgeryType.ToString()
        };
    }
}

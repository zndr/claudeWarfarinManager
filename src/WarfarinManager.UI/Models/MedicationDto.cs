using System;

namespace WarfarinManager.UI.Models
{
    /// <summary>
    /// DTO per rappresentare un farmaco concomitante nell'interfaccia utente
    /// </summary>
    public class MedicationDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string InteractionLevel { get; set; } = "None"; // None, Low, Moderate, High
        public string InteractionDetails { get; set; } = string.Empty;

        // Proprietà calcolate per UI
        public string StartDateFormatted => StartDate.ToString("dd/MM/yyyy");
        
        public string EndDateFormatted => EndDate?.ToString("dd/MM/yyyy") ?? "-";
        
        public string DurationText
        {
            get
            {
                var endDate = EndDate ?? DateTime.Today;
                var duration = (endDate - StartDate).Days;
                return duration == 1 ? "1 giorno" : $"{duration} giorni";
            }
        }

        public string InteractionBadgeColor => InteractionLevel switch
        {
            "High" => "#E81123",     // Rosso
            "Moderate" => "#FFB900", // Giallo
            "Low" => "#0078D4",      // Blu
            _ => "#107C10"           // Verde
        };

        public string InteractionBadgeText => InteractionLevel switch
        {
            "High" => "ALTO RISCHIO",
            "Moderate" => "MEDIO RISCHIO",
            "Low" => "BASSO RISCHIO",
            _ => "NESSUN RISCHIO"
        };

        public bool HasInteraction => InteractionLevel != "None";
        
        public bool IsHighRisk => InteractionLevel == "High";
    }
}

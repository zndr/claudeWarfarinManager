namespace WarfarinManager.Core.Models
    {
    /// <summary>
    /// Rappresenta uno schema posologico settimanale per warfarin
    /// </summary>
    public class WeeklyDoseSchedule
        {
        /// <summary>
        /// Dose totale settimanale in mg
        /// </summary>
        public decimal TotalWeeklyDose { get; set; }

        /// <summary>
        /// Dosi giornaliere per ogni giorno della settimana
        /// </summary>
        public Dictionary<DayOfWeek, decimal> DailyDoses { get; set; } = new();

        /// <summary>
        /// Schema descrittivo in formato testo
        /// Esempio: "Lunedì: 1 cp 5mg, Martedì: 1 cp 5mg, ..."
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Schema dettagliato per ogni giorno con formato compresse
        /// </summary>
        public Dictionary<DayOfWeek, string> DailyDescriptions { get; set; } = new();

        // Proprietà helper per accesso diretto ai giorni (per compatibilità con test)
        public decimal Monday => DailyDoses.GetValueOrDefault(DayOfWeek.Monday, 0m);
        public decimal Tuesday => DailyDoses.GetValueOrDefault(DayOfWeek.Tuesday, 0m);
        public decimal Wednesday => DailyDoses.GetValueOrDefault(DayOfWeek.Wednesday, 0m);
        public decimal Thursday => DailyDoses.GetValueOrDefault(DayOfWeek.Thursday, 0m);
        public decimal Friday => DailyDoses.GetValueOrDefault(DayOfWeek.Friday, 0m);
        public decimal Saturday => DailyDoses.GetValueOrDefault(DayOfWeek.Saturday, 0m);
        public decimal Sunday => DailyDoses.GetValueOrDefault(DayOfWeek.Sunday, 0m);

        /// <summary>
        /// Genera descrizione testuale completa dello schema
        /// </summary>
        public string GetFullDescription()
            {
            var days = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
            };

            var dayNames = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday, "Lunedì" },
                { DayOfWeek.Tuesday, "Martedì" },
                { DayOfWeek.Wednesday, "Mercoledì" },
                { DayOfWeek.Thursday, "Giovedì" },
                { DayOfWeek.Friday, "Venerdì" },
                { DayOfWeek.Saturday, "Sabato" },
                { DayOfWeek.Sunday, "Domenica" }
            };

            var lines = new List<string>();
            foreach (var day in days)
                {
                if (DailyDescriptions.TryGetValue(day, out var desc))
                    {
                    lines.Add($"{dayNames[day]}: {desc}");
                    }
                else if (DailyDoses.TryGetValue(day, out var dose))
                    {
                    lines.Add($"{dayNames[day]}: {FormatDose(dose)}");
                    }
                }

            return string.Join(Environment.NewLine, lines);
            }

        /// <summary>
        /// Formatta una dose in mg come descrizione compresse
        /// </summary>
        private string FormatDose(decimal doseMg)
            {
            if (doseMg == 0m) return "Non assumere";
            if (doseMg == 2.5m) return "1/2 cp (2.5mg)";
            if (doseMg == 5m) return "1 cp 5mg";
            if (doseMg == 7.5m) return "1 cp + 1/2 cp (7.5mg)";
            if (doseMg == 10m) return "2 cp 5mg";

            // Formato generico per altre dosi
            return $"{doseMg}mg";
            }

        /// <summary>
        /// Verifica se lo schema è valido
        /// </summary>
        public bool IsValid()
            {
            // Deve avere dosi per tutti i 7 giorni
            if (DailyDoses.Count != 7) return false;

            // La somma delle dosi giornaliere deve corrispondere al totale
            var sum = DailyDoses.Values.Sum();
            return Math.Abs(sum - TotalWeeklyDose) < 0.01m;
            }

        /// <summary>
        /// Crea uno schema uniforme (stessa dose ogni giorno)
        /// </summary>
        public static WeeklyDoseSchedule CreateUniform(decimal dailyDose)
            {
            var schedule = new WeeklyDoseSchedule
                {
                TotalWeeklyDose = dailyDose * 7
                };

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                schedule.DailyDoses[day] = dailyDose;
                }

            return schedule;
            }
        }
    }
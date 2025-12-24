using System;
using System.Collections.Generic;
using System.Linq;

namespace WarfarinManager.UI.Helpers
{
    /// <summary>
    /// Helper per la distribuzione equilibrata dei dosaggi settimanali di warfarin.
    /// Evita più di 3 giorni consecutivi con la stessa dose.
    /// </summary>
    public static class DoseDistributionHelper
    {
        private const decimal MG_PER_TABLET = 5.0m;

        /// <summary>
        /// Distribuisce una dose settimanale in modo equilibrato sui 7 giorni.
        /// </summary>
        /// <param name="weeklyDoseMg">Dose settimanale totale in mg</param>
        /// <param name="excludeQuarters">Se true, usa step 2.5mg; se false, usa step 1.25mg</param>
        /// <returns>Array di 7 dosi (Lun-Dom) distribuite in modo equilibrato</returns>
        public static decimal[] DistributeWeeklyDose(decimal weeklyDoseMg, bool excludeQuarters = true)
        {
            decimal step = excludeQuarters ? 2.5m : 1.25m;
            
            // Calcola dose base giornaliera
            decimal dailyAverage = weeklyDoseMg / 7m;
            
            // Arrotonda al multiplo di step più vicino (per difetto e per eccesso)
            decimal lowerDose = Math.Floor(dailyAverage / step) * step;
            decimal upperDose = lowerDose + step;
            
            // Se la dose è già perfettamente divisibile
            if (Math.Abs(dailyAverage - lowerDose) < 0.001m)
            {
                return Enumerable.Repeat(lowerDose, 7).ToArray();
            }
            
            // Calcola quanti giorni servono con dose alta per raggiungere il totale
            // weeklyDose = (7 - n) * lowerDose + n * upperDose
            // weeklyDose = 7 * lowerDose + n * step
            // n = (weeklyDose - 7 * lowerDose) / step
            decimal remainder = weeklyDoseMg - (7 * lowerDose);
            int daysWithHigherDose = (int)Math.Round(remainder / step);
            
            // Limita ai valori validi
            daysWithHigherDose = Math.Max(0, Math.Min(7, daysWithHigherDose));
            
            // Crea array con dosi base
            var doses = Enumerable.Repeat(lowerDose, 7).ToArray();
            
            // Distribuisci le dosi maggiori in modo equilibrato
            var highDoseIndices = GetBalancedIndices(daysWithHigherDose);
            foreach (var index in highDoseIndices)
            {
                doses[index] = upperDose;
            }
            
            // Verifica e aggiusta se necessario per evitare >3 giorni consecutivi uguali
            doses = EnsureNoMoreThanThreeConsecutive(doses, lowerDose, upperDose);
            
            return doses;
        }

        /// <summary>
        /// Restituisce gli indici distribuiti in modo equilibrato per le dosi maggiori.
        /// </summary>
        private static int[] GetBalancedIndices(int count)
        {
            if (count <= 0) return Array.Empty<int>();
            if (count >= 7) return new[] { 0, 1, 2, 3, 4, 5, 6 };

            // Pattern ottimali per distribuire n giorni su 7
            return count switch
            {
                1 => new[] { 3 },                           // Giovedì (metà settimana)
                2 => new[] { 2, 5 },                        // Mer, Sab (distanziati)
                3 => new[] { 1, 3, 5 },                     // Mar, Gio, Sab (alternati)
                4 => new[] { 0, 2, 4, 6 },                  // Lun, Mer, Ven, Dom (alternati)
                5 => new[] { 0, 1, 3, 4, 6 },               // Lun, Mar, Gio, Ven, Dom
                6 => new[] { 0, 1, 2, 4, 5, 6 },            // Tutti tranne Gio
                _ => Array.Empty<int>()
            };
        }

        /// <summary>
        /// Assicura che non ci siano più di 3 giorni consecutivi con la stessa dose.
        /// Considera anche il wrap-around (Dom-Lun).
        /// </summary>
        private static decimal[] EnsureNoMoreThanThreeConsecutive(decimal[] doses, decimal lowDose, decimal highDose)
        {
            // Trova sequenze di >3 giorni consecutivi uguali
            bool needsRebalancing = false;
            
            for (int start = 0; start < 7; start++)
            {
                int consecutiveCount = 1;
                decimal currentDose = doses[start];
                
                for (int offset = 1; offset < 7; offset++)
                {
                    int idx = (start + offset) % 7;
                    if (doses[idx] == currentDose)
                    {
                        consecutiveCount++;
                        if (consecutiveCount > 3)
                        {
                            needsRebalancing = true;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (needsRebalancing) break;
            }
            
            if (!needsRebalancing) return doses;
            
            // Riequilibra: alterna le dosi il più possibile
            int highCount = doses.Count(d => d == highDose);
            int lowCount = 7 - highCount;
            
            var result = new decimal[7];
            
            if (highCount == 0 || lowCount == 0)
            {
                // Tutte le dosi uguali, non si può fare nulla
                return doses;
            }
            
            // Strategia: distribuisci in modo che le dosi si alternino il più possibile
            // Usa un pattern che massimizza la distanza tra dosi uguali
            if (highCount <= lowCount)
            {
                // Più dosi basse, inserisci le alte in posizioni distribuite
                for (int i = 0; i < 7; i++) result[i] = lowDose;
                var indices = GetBalancedIndices(highCount);
                foreach (var idx in indices) result[idx] = highDose;
            }
            else
            {
                // Più dosi alte, inserisci le basse in posizioni distribuite
                for (int i = 0; i < 7; i++) result[i] = highDose;
                var indices = GetBalancedIndices(lowCount);
                foreach (var idx in indices) result[idx] = lowDose;
            }
            
            return result;
        }

        /// <summary>
        /// Formatta una dose in compresse in modo leggibile.
        /// </summary>
        public static string FormatAsTablets(decimal doseMg)
        {
            if (doseMg == 0) return "—";

            decimal tablets = doseMg / MG_PER_TABLET;
            int wholeTablets = (int)tablets;
            decimal fraction = tablets - wholeTablets;

            string fractionStr = fraction switch
            {
                0m => "",
                0.25m => "¼",
                0.5m => "½",
                0.75m => "¾",
                _ => $"+{fraction:F2}"
            };

            if (wholeTablets == 0)
                return fractionStr;
            else if (string.IsNullOrEmpty(fractionStr))
                return wholeTablets.ToString();
            else
                return $"{wholeTablets}{fractionStr}";
        }

        /// <summary>
        /// Genera una descrizione breve dello schema settimanale.
        /// Es: "Lun 2.5 mg (½ cp), Mar 5 mg (1 cp), Mer 2.5 mg (½ cp)..."
        /// </summary>
        public static string GenerateShortSchedule(decimal[] doses)
        {
            if (doses == null || doses.Length != 7)
                return string.Empty;

            string[] days = { "Lun", "Mar", "Mer", "Gio", "Ven", "Sab", "Dom" };
            var parts = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                string mgFormatted = FormatDoseMg(doses[i]);
                string tablets = FormatAsTablets(doses[i]);
                parts.Add($"{days[i]} {mgFormatted} mg ({tablets} cp)");
            }

            return string.Join(", ", parts);
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

        /// <summary>
        /// Verifica se due schemi settimanali sono equivalenti (stesso totale, stessa distribuzione).
        /// </summary>
        public static bool AreSchedulesEqual(decimal[] schedule1, decimal[] schedule2)
        {
            if (schedule1 == null || schedule2 == null) return false;
            if (schedule1.Length != 7 || schedule2.Length != 7) return false;
            
            for (int i = 0; i < 7; i++)
            {
                if (schedule1[i] != schedule2[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Calcola il totale settimanale da un array di dosi giornaliere.
        /// </summary>
        public static decimal CalculateWeeklyTotal(decimal[] doses)
        {
            return doses?.Sum() ?? 0;
        }
    }
}

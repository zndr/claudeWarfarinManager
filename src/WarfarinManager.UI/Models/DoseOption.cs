using System;

namespace WarfarinManager.UI.Models
{
    /// <summary>
    /// Modello per rappresentare una dose di warfarin con visualizzazione in mg e compresse
    /// Warfarin standard: 1 compressa = 5 mg
    /// </summary>
    public class DoseOption : IEquatable<DoseOption>
    {
        private const decimal MG_PER_TABLET = 5.0m;

        /// <summary>
        /// Dose in milligrammi
        /// </summary>
        public decimal DoseMg { get; }

        /// <summary>
        /// Numero di compresse (calcolato)
        /// </summary>
        public decimal Tablets => DoseMg / MG_PER_TABLET;

        /// <summary>
        /// Descrizione formattata per visualizzazione (es: "5.0 mg (1 cp)")
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// Descrizione breve solo compresse (es: "1 cp", "½ cp")
        /// </summary>
        public string TabletDescription { get; }

        public DoseOption(decimal doseMg)
        {
            DoseMg = doseMg;
            TabletDescription = FormatTablets(doseMg);
            DisplayText = doseMg == 0
                ? "0 mg (nessuna)"
                : $"{FormatDoseMg(doseMg)} mg ({TabletDescription})";
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
        /// Formatta il numero di compresse in modo leggibile
        /// </summary>
        private static string FormatTablets(decimal doseMg)
        {
            if (doseMg == 0) return "—";

            decimal tablets = doseMg / MG_PER_TABLET;
            
            // Separa parte intera e decimale
            int wholeTablets = (int)tablets;
            decimal fraction = tablets - wholeTablets;

            // Converti frazione in simbolo
            string fractionStr = fraction switch
            {
                0m => "",
                0.25m => "¼",
                0.5m => "½",
                0.75m => "¾",
                _ => $"+{fraction:F2}" // Fallback per frazioni non standard
            };

            // Costruisci stringa finale
            if (wholeTablets == 0)
            {
                // Solo frazione (es: "½ cp")
                return $"{fractionStr} cp";
            }
            else if (string.IsNullOrEmpty(fractionStr))
            {
                // Solo intero (es: "1 cp", "2 cp")
                return $"{wholeTablets} cp";
            }
            else
            {
                // Intero + frazione (es: "1½ cp", "2¼ cp")
                return $"{wholeTablets}{fractionStr} cp";
            }
        }

        /// <summary>
        /// Crea una lista di DoseOption basata sullo step specificato
        /// </summary>
        /// <param name="excludeQuarters">Se true, usa step 2.5mg; se false, usa step 1.25mg</param>
        /// <param name="maxDose">Dose massima da includere (default 20mg)</param>
        public static DoseOption[] CreateOptions(bool excludeQuarters, decimal maxDose = 20.0m)
        {
            decimal step = excludeQuarters ? 2.5m : 1.25m;
            int count = (int)(maxDose / step) + 1;
            var options = new DoseOption[count];

            for (int i = 0; i < count; i++)
            {
                options[i] = new DoseOption(i * step);
            }

            return options;
        }

        /// <summary>
        /// Trova l'opzione più vicina a un valore dato
        /// </summary>
        public static DoseOption FindNearest(DoseOption[] options, decimal targetDose)
        {
            if (options == null || options.Length == 0)
                return new DoseOption(targetDose);

            DoseOption nearest = options[0];
            decimal minDiff = Math.Abs(options[0].DoseMg - targetDose);

            foreach (var option in options)
            {
                decimal diff = Math.Abs(option.DoseMg - targetDose);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearest = option;
                }
            }

            return nearest;
        }

        // Override per binding corretto
        public override string ToString() => DisplayText;

        public override bool Equals(object? obj) => Equals(obj as DoseOption);

        public bool Equals(DoseOption? other)
        {
            if (other is null) return false;
            return DoseMg == other.DoseMg;
        }

        public override int GetHashCode() => DoseMg.GetHashCode();

        public static bool operator ==(DoseOption? left, DoseOption? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(DoseOption? left, DoseOption? right) => !(left == right);

        // Conversione implicita da decimal per retrocompatibilità
        public static implicit operator decimal(DoseOption option) => option?.DoseMg ?? 0;
    }
}

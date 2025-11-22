using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;

namespace WarfarinManager.Data.Seeding;

/// <summary>
/// Seeding dati lookup per InteractionDrugs
/// Basato su tabella PRD - Farmaci ad ALTO/MODERATO RISCHIO
/// </summary>
public static class InteractionDrugSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var drugs = new List<InteractionDrug>
        {
            // ========== FARMACI AD ALTO RISCHIO (Aumentano INR) ==========
            
            new InteractionDrug
            {
                Id = 1,
                DrugName = "Cotrimoxazolo (Trimetoprim-Sulfametoxazolo)",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 2.70m,
                Mechanism = "Inibizione CYP2C9 + inibizione sintesi Vitamina K",
                FCSAManagement = "Ridurre dose warfarin 25-40% se indispensabile. Controllo INR dopo 3-5 giorni.",
                ACCPManagement = "Monitoraggio stretto INR. Considerare antibiotico alternativo.",
                RecommendedINRCheckDays = 3
            },
            
            new InteractionDrug
            {
                Id = 2,
                DrugName = "Fluconazolo",
                Category = "Antifungal",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 4.57m,
                Mechanism = "Inibizione CYP2C9",
                FCSAManagement = "Ridurre dose warfarin 25-40%. Controllo INR ravvicinato ogni 3-5 giorni.",
                ACCPManagement = "Riduzione empirica dose. Monitoraggio stretto.",
                RecommendedINRCheckDays = 3
            },
            
            new InteractionDrug
            {
                Id = 3,
                DrugName = "Voriconazolo",
                Category = "Antifungal",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 4.57m,
                Mechanism = "Inibizione CYP2C9",
                FCSAManagement = "Ridurre dose warfarin 25-40%. Monitoraggio giornaliero INR inizialmente.",
                ACCPManagement = "Riduzione significativa dose. Controlli frequenti.",
                RecommendedINRCheckDays = 3
            },
            
            new InteractionDrug
            {
                Id = 4,
                DrugName = "Metronidazolo",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Inibizione CYP2C9",
                FCSAManagement = "Ridurre dose warfarin 1/3-1/2 se necessario. Controllo INR dopo 3 giorni.",
                ACCPManagement = "Riduzione dose empirica 30-50%. Monitoraggio stretto.",
                RecommendedINRCheckDays = 3
            },
            
            new InteractionDrug
            {
                Id = 5,
                DrugName = "Eritromicina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 1.86m,
                Mechanism = "Inibizione CYP3A4",
                FCSAManagement = "Monitoraggio INR. Ridurre dose 10-25% se necessario.",
                ACCPManagement = "Controllo INR entro 3-5 giorni dall'inizio.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 6,
                DrugName = "Claritromicina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                OddsRatio = 1.86m,
                Mechanism = "Inibizione CYP3A4",
                FCSAManagement = "Monitoraggio INR. Ridurre dose 10-25% se necessario.",
                ACCPManagement = "Controllo INR entro 3-5 giorni.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 7,
                DrugName = "Ciprofloxacina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Moderate,
                OddsRatio = 1.69m,
                Mechanism = "Meccanismo variabile",
                FCSAManagement = "Monitoraggio INR. Ridurre dose 10-15% se necessario.",
                ACCPManagement = "Controllo INR dopo 5-7 giorni.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 8,
                DrugName = "Levofloxacina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Moderate,
                OddsRatio = 1.69m,
                Mechanism = "Meccanismo variabile",
                FCSAManagement = "Monitoraggio INR. Ridurre dose 10-15% se necessario.",
                ACCPManagement = "Controllo INR dopo 5-7 giorni.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 9,
                DrugName = "Amiodarone",
                Category = "Antiarrhythmic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Inibizione CYP2C9 + lunga emivita (40-60 giorni)",
                FCSAManagement = "Ridurre dose warfarin 20-30% all'inizio. Aumentare 60% alla sospensione. Effetto persiste 4-8 settimane dopo stop.",
                ACCPManagement = "Riduzione empirica 25-30%. Controlli settimanali per 6-8 settimane.",
                RecommendedINRCheckDays = 7
            },
            
            new InteractionDrug
            {
                Id = 10,
                DrugName = "Azitromicina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Low,
                Mechanism = "Interazione minore",
                FCSAManagement = "Monitoraggio routine. Generalmente non richiede aggiustamento.",
                ACCPManagement = "Controllo INR dopo 7 giorni se terapia prolungata.",
                RecommendedINRCheckDays = 7
            },
            
            // ========== FARMACI CHE RIDUCONO INR ==========
            
            new InteractionDrug
            {
                Id = 11,
                DrugName = "Rifampicina",
                Category = "Antibiotic",
                InteractionEffect = InteractionEffect.Decreases,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Induzione CYP2C9",
                FCSAManagement = "Aumentare dose warfarin fino a 100%. Riduzione rapida in 5-8 giorni dopo stop per depositi tissutali.",
                ACCPManagement = "Aumentare dose 50-100%. Controllo INR ogni 3-5 giorni. Monitoraggio stretto alla sospensione.",
                RecommendedINRCheckDays = 3
            },
            
            new InteractionDrug
            {
                Id = 12,
                DrugName = "Carbamazepina",
                Category = "Anticonvulsant",
                InteractionEffect = InteractionEffect.Decreases,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Induzione CYP2C9",
                FCSAManagement = "Aumentare dose warfarin 50-100%. Monitoraggio stretto.",
                ACCPManagement = "Aumento significativo dose. Controlli frequenti.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 13,
                DrugName = "Fenobarbital",
                Category = "Anticonvulsant",
                InteractionEffect = InteractionEffect.Decreases,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Induzione CYP2C9",
                FCSAManagement = "Aumentare dose warfarin 50-100%. Monitoraggio stretto.",
                ACCPManagement = "Aumento significativo dose. Controlli frequenti.",
                RecommendedINRCheckDays = 5
            },
            
            new InteractionDrug
            {
                Id = 14,
                DrugName = "Fenitoina",
                Category = "Anticonvulsant",
                InteractionEffect = InteractionEffect.Variable,
                InteractionLevel = InteractionLevel.High,
                Mechanism = "Induzione CYP2C9 + competizione legame proteine plasmatiche",
                FCSAManagement = "Effetto bifasico: inizialmente può aumentare INR, poi ridurlo. Monitoraggio molto stretto.",
                ACCPManagement = "Controlli INR frequenti. Effetto imprevedibile.",
                RecommendedINRCheckDays = 3
            },
            
            // ========== ALTRI FARMACI RILEVANTI ==========
            
            new InteractionDrug
            {
                Id = 15,
                DrugName = "FANS (Ibuprofene, Diclofenac, Naprossene)",
                Category = "NSAID",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Moderate,
                Mechanism = "Effetto antiaggregante + gastropatia",
                FCSAManagement = "Evitare se possibile. Se necessario: gastroprotettore + monitoraggio. Preferire paracetamolo.",
                ACCPManagement = "Sconsigliato uso cronico. Aumenta rischio emorragico indipendentemente da INR.",
                RecommendedINRCheckDays = 7
            },
            
            new InteractionDrug
            {
                Id = 16,
                DrugName = "Aspirina (>100mg/die)",
                Category = "Antiplatelet",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Moderate,
                Mechanism = "Effetto antiaggregante sinergico",
                FCSAManagement = "Valutare rischio/beneficio. Se necessario, dosi basse (75-100mg). Gastroprotettore obbligatorio.",
                ACCPManagement = "Aumenta rischio emorragico. Usare solo se strettamente indicato.",
                RecommendedINRCheckDays = 7
            },
            
            new InteractionDrug
            {
                Id = 17,
                DrugName = "Omeprazolo",
                Category = "PPI",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Low,
                Mechanism = "Inibizione minore CYP2C19",
                FCSAManagement = "Interazione clinicamente poco rilevante. Monitoraggio routine.",
                ACCPManagement = "Nessun aggiustamento routinario necessario.",
                RecommendedINRCheckDays = 14
            },
            
            new InteractionDrug
            {
                Id = 18,
                DrugName = "Simvastatina",
                Category = "Statin",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Low,
                Mechanism = "Interazione minore",
                FCSAManagement = "Monitoraggio routine. Raramente richiede aggiustamento.",
                ACCPManagement = "Interazione clinicamente non significativa.",
                RecommendedINRCheckDays = 14
            },
            
            new InteractionDrug
            {
                Id = 19,
                DrugName = "Levotiroxina",
                Category = "Thyroid",
                InteractionEffect = InteractionEffect.Increases,
                InteractionLevel = InteractionLevel.Low,
                Mechanism = "Aumento catabolismo fattori coagulazione",
                FCSAManagement = "Monitoraggio INR se cambio dosaggio tiroxina.",
                ACCPManagement = "Controllo INR dopo 2-4 settimane da modifiche dosaggio.",
                RecommendedINRCheckDays = 14
            },
            
            new InteractionDrug
            {
                Id = 20,
                DrugName = "Alcool (uso cronico/abuso)",
                Category = "Other",
                InteractionEffect = InteractionEffect.Variable,
                InteractionLevel = InteractionLevel.Moderate,
                Mechanism = "Uso acuto aumenta INR, uso cronico può indurre enzimi",
                FCSAManagement = "Educare paziente: max 1-2 unità/die. Monitoraggio se consumo variabile.",
                ACCPManagement = "Limitare consumo. Effetto imprevedibile.",
                RecommendedINRCheckDays = 7
            }
        };
        
        modelBuilder.Entity<InteractionDrug>().HasData(drugs);
    }
}

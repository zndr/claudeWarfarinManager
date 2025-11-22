using Microsoft.EntityFrameworkCore;
using WarfarinManager.Data.Entities;

namespace WarfarinManager.Data.Seeding;

/// <summary>
/// Seeding dati lookup per IndicationTypes
/// </summary>
public static class IndicationTypeSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var indications = new List<IndicationType>
        {
            new IndicationType { Id = 1, Code = "TVP_TREATMENT", Category = "Tromboembolismo Venoso", Description = "TVP - Trombosi Venosa Profonda (trattamento)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi (primo episodio), indefinito se ricorrente" },
            new IndicationType { Id = 2, Code = "TVP_PROPHYLAXIS", Category = "Tromboembolismo Venoso", Description = "TVP - Profilassi tromboembolica", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Secondo fattori di rischio" },
            new IndicationType { Id = 3, Code = "PE_TREATMENT", Category = "Tromboembolismo Venoso", Description = "Embolia Polmonare (trattamento)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi (primo episodio), indefinito se ricorrente" },
            new IndicationType { Id = 4, Code = "TVP_PE_RECURRENT", Category = "Tromboembolismo Venoso", Description = "TVP/EP ricorrente", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 5, Code = "FA_STROKE_PREVENTION", Category = "Fibrillazione Atriale", Description = "Fibrillazione Atriale - Prevenzione stroke (CHA₂DS₂-VASc ≥2)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito" },
            new IndicationType { Id = 6, Code = "FA_MITRAL_STENOSIS", Category = "Fibrillazione Atriale", Description = "FA con stenosi mitralica", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito" },
            new IndicationType { Id = 7, Code = "FA_VALVE_PROSTHESIS", Category = "Fibrillazione Atriale", Description = "FA con protesi valvolare", TargetINRMin = 2.5m, TargetINRMax = 3.5m, TypicalDuration = "Indefinito" },
            new IndicationType { Id = 8, Code = "MECHANICAL_MITRAL", Category = "Protesi Valvolari", Description = "Protesi meccanica mitralica", TargetINRMin = 2.5m, TargetINRMax = 3.5m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 9, Code = "MECHANICAL_AORTIC", Category = "Protesi Valvolari", Description = "Protesi meccanica aortica", TargetINRMin = 2.5m, TargetINRMax = 3.5m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 10, Code = "MECHANICAL_TRICUSPID", Category = "Protesi Valvolari", Description = "Protesi meccanica tricuspidale", TargetINRMin = 2.5m, TargetINRMax = 3.5m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 11, Code = "BIOPROSTHESIS_EARLY", Category = "Protesi Valvolari", Description = "Bioprotesi valvolare (primi 3-6 mesi)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi post-impianto" },
            new IndicationType { Id = 12, Code = "MI_LV_DYSFUNCTION", Category = "Infarto Miocardico", Description = "Post-IM con disfunzione ventricolo sinistro (FE <35%)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi, poi rivalutare" },
            new IndicationType { Id = 13, Code = "MI_ANTERIOR_EXTENSIVE", Category = "Infarto Miocardico", Description = "Infarto miocardico anteriore esteso", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi" },
            new IndicationType { Id = 14, Code = "LV_ANEURYSM", Category = "Infarto Miocardico", Description = "Aneurisma ventricolare sinistro", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito" },
            new IndicationType { Id = 15, Code = "LV_THROMBUS", Category = "Infarto Miocardico", Description = "Trombo intraventricolare", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "3-6 mesi (fino a risoluzione trombo)" },
            new IndicationType { Id = 16, Code = "DILATED_CARDIOMYOPATHY", Category = "Cardiomiopatie", Description = "Cardiomiopatia dilatativa con FE ridotta (<35%)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito se alto rischio TE" },
            new IndicationType { Id = 17, Code = "APS_ARTERIAL", Category = "Sindrome Anticorpi Antifosfolipidi", Description = "Sindrome anticorpi antifosfolipidi (eventi arteriosi)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 18, Code = "APS_VENOUS", Category = "Sindrome Anticorpi Antifosfolipidi", Description = "Sindrome anticorpi antifosfolipidi (eventi venosi)", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito (lifelong)" },
            new IndicationType { Id = 19, Code = "APS_RECURRENT", Category = "Sindrome Anticorpi Antifosfolipidi", Description = "APS con eventi ricorrenti nonostante INR 2-3", TargetINRMin = 2.5m, TargetINRMax = 3.5m, TypicalDuration = "Indefinito (target più alto)" },
            new IndicationType { Id = 20, Code = "ISCHEMIC_STROKE", Category = "Stroke/TIA", Description = "Ictus ischemico cardioembolico", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito se FA o altra cardiopatia emboligena" },
            new IndicationType { Id = 21, Code = "TIA_CARDIOEMBOLIC", Category = "Stroke/TIA", Description = "TIA cardioembolico", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito se FA o altra cardiopatia emboligena" },
            new IndicationType { Id = 22, Code = "PULMONARY_HYPERTENSION", Category = "Altre Indicazioni", Description = "Ipertensione polmonare primaria", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Indefinito" },
            new IndicationType { Id = 23, Code = "PERIPHERAL_ARTERIAL_DISEASE", Category = "Altre Indicazioni", Description = "Arteriopatia periferica severa", TargetINRMin = 2.0m, TargetINRMax = 3.0m, TypicalDuration = "Secondo valutazione vascolare" }
        };
        
        modelBuilder.Entity<IndicationType>().HasData(indications);
    }
}

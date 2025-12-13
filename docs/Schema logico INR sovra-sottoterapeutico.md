## Schema Logico per la Gestione della Terapia con Warfarin

### SEZIONE 1: INPUT RICHIESTI

**1.1 Dati del paziente (per calcolo automatico rischio)**

| Campo | Tipo | Note |
|-------|------|------|
| Indicazione TAO | Enum | FA, TEV, Protesi meccanica mitrale, Protesi meccanica aortica, Valvulopatia, Altro |
| Data ultimo evento tromboembolico | Date | Nullable - per calcolo TEV recente (<3 mesi) |
| CHA₂DS₂-VASc score | Int (0-9) | Calcolabile o inserito manualmente |
| Protesi valvolare meccanica | Boolean | |
| Posizione protesi | Enum | Mitrale, Aortica, Entrambe (se protesi = true) |

**1.2 Dati terapia corrente**

| Campo | Tipo | Note |
|-------|------|------|
| Dose giornaliera attuale (mg) | Decimal | Oppure dose settimanale totale |
| Dose settimanale totale (mg) | Decimal | Calcolata automaticamente se non inserita |
| Range terapeutico target | Enum | 2.0-3.0 oppure 2.5-3.5 |

**1.3 Dati controllo attuale**

| Campo | Tipo | Note |
|-------|------|------|
| INR attuale | Decimal | Valore misurato |
| Data misurazione | Date | |
| Presenza sanguinamento | Boolean | |
| Tipo sanguinamento | Enum | Nessuno, Minore, Maggiore, Rischio vitale |
| Sede sanguinamento | Enum | Se presente: Cutaneo, GI, Urinario, Nasale, Intracranico, Retroperitoneale, Altro |

---

### SEZIONE 2: CLASSIFICAZIONE AUTOMATICA DEL RISCHIO

**2.1 Algoritmo calcolo rischio trombotico elevato**

```
FUNZIONE CalcolaRischioTromboticoElevato(paziente) → Boolean

SE paziente.ProtesiValvolareMeccanica = TRUE ALLORA
    RITORNA TRUE
    
SE paziente.DataUltimoTEV != NULL E 
   (DataOggi - paziente.DataUltimoTEV) < 90 giorni ALLORA
    RITORNA TRUE
    
SE paziente.Indicazione = "FA" E paziente.CHA2DS2VASc >= 4 ALLORA
    RITORNA TRUE
    
SE paziente.Indicazione = "TEV" E 
   (DataOggi - paziente.DataUltimoTEV) < 30 giorni ALLORA
    RITORNA TRUE

RITORNA FALSE
```

**2.2 Algoritmo classificazione emorragia**

```
FUNZIONE ClassificaEmorragia(sede, parametriClinici) → TipoEmorragia

SE sede IN ["Intracranico", "Retroperitoneale"] ALLORA
    RITORNA "RischioVitale"
    
SE parametriClinici.ShockEmorragico = TRUE ALLORA
    RITORNA "RischioVitale"
    
SE parametriClinici.NecessitaTrasfusione = TRUE O
   parametriClinici.CaloHb >= 2 g/dL O
   sede = "GI" CON sanguinamento attivo ALLORA
    RITORNA "Maggiore"
    
SE sede IN ["Cutaneo", "Nasale", "Urinario"] E 
   parametriClinici.Controllabile = TRUE ALLORA
    RITORNA "Minore"
    
RITORNA "Minore" // Default con conferma utente
```

---

### SEZIONE 3: LOGICA DECISIONALE INR SOTTOTERAPEUTICO

**3.1 Determinazione fascia INR sottoterapeutico**

Per **Target 2.0-3.0**:
| Fascia | Range INR | Codice |
|--------|-----------|--------|
| Molto basso | < 1.5 | SUB_CRITICO |
| Moderatamente basso | 1.5 - 1.79 | SUB_MODERATO |
| Lievemente basso | 1.8 - 1.99 | SUB_LIEVE |

Per **Target 2.5-3.5**:
| Fascia | Range INR | Codice |
|--------|-----------|--------|
| Molto basso | < 2.0 | SUB_CRITICO |
| Moderatamente basso | 2.0 - 2.29 | SUB_MODERATO |
| Lievemente basso | 2.3 - 2.49 | SUB_LIEVE |

**3.2 Algoritmo gestione INR sottoterapeutico**

```
FUNZIONE GestisciINRBasso(inr, target, doseSettimanale, rischioAlto) → Raccomandazione

fascia = DeterminaFasciaINRBasso(inr, target)

SWITCH fascia:

    CASO SUB_CRITICO:
        incrementoPercentuale = 15-20%  // FCSA: 10-20%
        doseSupplementare = doseSettimanale * 0.075  // 5-10% primo giorno
        nuovaDoseSettimanale = doseSettimanale * 1.175
        giorniControllo = 5-7
        
        SE rischioAlto ALLORA
            aggiungiEBPM = TRUE
            doseEBPM = "Enoxaparina 70 UI/kg x 2/die"
            durataEBPM = "Fino a INR ≥2.0 per 24 ore"
        FINE SE
        
    CASO SUB_MODERATO:
        incrementoPercentuale = 7.5-15%  // FCSA: 5-15%
        doseSupplementare = doseSettimanale * 0.05  // opzionale
        nuovaDoseSettimanale = doseSettimanale * 1.10
        giorniControllo = 7-10
        
    CASO SUB_LIEVE:
        // Valutazione clinica: possibile non modificare se ultimi 2 INR in range
        incrementoPercentuale = 5-10%  // se necessario
        doseSupplementare = 0
        nuovaDoseSettimanale = doseSettimanale * 1.05
        giorniControllo = 10-14
        
RITORNA Raccomandazione
```

---

### SEZIONE 4: LOGICA DECISIONALE INR SOVRATERAPEUTICO

**4.1 Determinazione fascia INR sovraterapeutico**

Per **Target 2.0-3.0**:
| Fascia | Range INR | Codice | Emorragia |
|--------|-----------|--------|-----------|
| Lievemente alto | 3.1 - 3.4 | SOVRA_LIEVE | No |
| Moderatamente alto | 3.5 - 3.9 | SOVRA_MODERATO | No |
| Alto | 4.0 - 4.9 | SOVRA_ALTO | No |
| Molto alto | 5.0 - 5.9 | SOVRA_MOLTO_ALTO | No |
| Critico | 6.0 - 7.9 | SOVRA_CRITICO | No |
| Estremo | ≥ 8.0 | SOVRA_ESTREMO | No |

Per **Target 2.5-3.5**:
| Fascia | Range INR | Codice | Emorragia |
|--------|-----------|--------|-----------|
| Lievemente alto | 3.6 - 3.9 | SOVRA_LIEVE | No |
| Moderatamente alto | 4.0 - 4.4 | SOVRA_MODERATO | No |
| Alto | 4.5 - 5.4 | SOVRA_ALTO | No |
| Molto alto | 5.5 - 6.4 | SOVRA_MOLTO_ALTO | No |
| Critico | 6.5 - 8.4 | SOVRA_CRITICO | No |
| Estremo | ≥ 8.5 | SOVRA_ESTREMO | No |

**4.2 Algoritmo gestione INR sovraterapeutico SENZA emorragia**

```
FUNZIONE GestisciINRAltoSenzaEmorragia(inr, target, doseSettimanale) → Raccomandazione

fascia = DeterminaFasciaINRAlto(inr, target)

SWITCH fascia:

    CASO SOVRA_LIEVE:
        // INR 3.1-3.4 (target 2-3) o 3.6-3.9 (target 2.5-3.5)
        sospensioneDosi = 0
        riduzionePercentuale = 5-10%
        nuovaDoseSettimanale = doseSettimanale * 0.925
        vitaminaK = FALSE
        giorniControllo = 7-14
        nota = "Se ultimi 2 INR in range e causa transitoria, considerare non modificare"
        
    CASO SOVRA_MODERATO:
        // INR 3.5-3.9 (target 2-3) o 4.0-4.4 (target 2.5-3.5)
        sospensioneDosi = 1  // considerare
        riduzionePercentuale = 10-15%
        nuovaDoseSettimanale = doseSettimanale * 0.875
        vitaminaK = FALSE
        giorniControllo = 5-8
        
    CASO SOVRA_ALTO:
        // INR 4.0-4.9 (target 2-3) o 4.5-5.4 (target 2.5-3.5)
        sospensioneDosi = 1
        riduzionePercentuale = 10-15%
        nuovaDoseSettimanale = doseSettimanale * 0.875
        vitaminaK = FALSE  // FCSA: no vitamina K sotto INR 5
        giorniControllo = 4-8
        controlloINR24h = FALSE
        
    CASO SOVRA_MOLTO_ALTO:
        // INR 5.0-5.9 (target 2-3) o 5.5-6.4 (target 2.5-3.5)
        sospensioneDosi = 1-2
        riduzionePercentuale = 15-20%
        nuovaDoseSettimanale = doseSettimanale * 0.80
        vitaminaK = OPZIONALE  // 2 mg PO se fattori rischio emorragico
        doseVitK = 2  // mg per os
        giorniControllo = 4-7
        controlloINR24h = TRUE
        
    CASO SOVRA_CRITICO:
        // INR 6.0-7.9 (target 2-3) o 6.5-8.4 (target 2.5-3.5)
        sospensioneDosi = 2-3
        riduzionePercentuale = 20%
        nuovaDoseSettimanale = doseSettimanale * 0.80
        vitaminaK = TRUE  // FCSA raccomanda
        doseVitK = 2-3  // mg per os
        controlloINR24h = TRUE
        
    CASO SOVRA_ESTREMO:
        // INR ≥ 8.0 (target 2-3) o ≥ 8.5 (target 2.5-3.5)
        sospensioneDosi = "Fino a INR < limite superiore range"
        riduzionePercentuale = 20-50%
        nuovaDoseSettimanale = doseSettimanale * 0.65  // media
        vitaminaK = TRUE  // OBBLIGATORIA
        doseVitK = 3-5  // mg per os (FCSA) / 2.5-5 mg (ACCP)
        controlloINR24h = TRUE
        controlloINR48h = TRUE
        nota = "Possibile seconda dose vitamina K se INR ancora elevato"
        
RITORNA Raccomandazione
```

**4.3 Algoritmo gestione INR sovraterapeutico CON emorragia**

```
FUNZIONE GestisciINRAltoConEmorragia(inr, tipoEmorragia, doseSettimanale) → Raccomandazione

SWITCH tipoEmorragia:

    CASO "Minore":
        sospendereWarfarin = TRUE
        vitaminaK = TRUE
        SE inr >= 5 E inr < 8 ALLORA
            doseVitK = 2  // mg per os
        ALTRIMENTI SE inr >= 8 ALLORA
            doseVitK = 3-5  // mg per os
        FINE SE
        viaVitK = "PO"
        controlloINR24h = TRUE
        ospedalizzazione = "VALUTARE"
        ricercaCausaLocale = TRUE
        riduzioneRipresa = 20%
        
    CASO "Maggiore":
        sospendereWarfarin = TRUE
        vitaminaK = TRUE
        doseVitK = 10  // mg EV
        viaVitK = "EV lenta (10-20 min)"
        ripetibileVitK = "ogni 12 ore"
        
        // Fattori procoagulanti
        PCC = TRUE  // Concentrato complesso protrombinico - PREFERIBILE
        dosePCC = "20-50 UI/kg"
        OPPURE
        plasma = TRUE
        dosePlasma = "15 mL/kg"
        
        ospedalizzazione = "OBBLIGATORIA"
        controlloINRPostPCC = TRUE
        controlloINRSeriato = "24-48h"
        
    CASO "RischioVitale":
        sospendereWarfarin = TRUE
        vitaminaK = TRUE
        doseVitK = 10  // mg EV
        viaVitK = "EV lenta"
        
        PCC = TRUE  // PRIMA SCELTA
        dosePCC = "20-50 UI/kg"
        
        ricoveroTerapiaIntensiva = TRUE
        SE sedeEmorragia = "Intracranico" ALLORA
            imagingCerebrale = "URGENTE"
        FINE SE
        valutazioneChirurgica = "IMMEDIATA se indicata"
        
        // Nota post-emergenza
        notaRipresa = "Se necessario proseguire anticoagulazione: eparina per 7-10 giorni 
                       fino a scomparsa effetto vitamina K"

RITORNA Raccomandazione
```

---

### SEZIONE 5: CALCOLI NUMERICI

**5.1 Funzione calcolo nuova dose settimanale**

```
FUNZIONE CalcolaNuovaDoseSettimanale(
    doseAttualeSettimanale: Decimal,
    percentualeVariazione: Decimal,  // positiva = aumento, negativa = riduzione
    doseSupplementarePrimoGiorno: Decimal = 0
) → DoseOutput

nuovaDoseSettimanale = doseAttualeSettimanale * (1 + percentualeVariazione/100)
nuovaDoseGiornalieraMedia = nuovaDoseSettimanale / 7

// Arrotondamento a quarti di compressa (1.25 mg per warfarin 5mg)
nuovaDoseGiornalieraArrotondata = Arrotonda(nuovaDoseGiornalieraMedia, 1.25)

// Distribuzione settimanale
doseGiorno1 = nuovaDoseGiornalieraArrotondata + doseSupplementarePrimoGiorno
doseGiorni2_7 = nuovaDoseGiornalieraArrotondata

// Verifica totale settimanale
totaleEffettivo = doseGiorno1 + (doseGiorni2_7 * 6)

RITORNA DoseOutput {
    DoseSettimanaleCalcolata = nuovaDoseSettimanale,
    DoseSettimanaleEffettiva = totaleEffettivo,
    DoseGiorno1 = doseGiorno1,
    DoseGiorni2_7 = doseGiorni2_7,
    SchemaSettimanale = GeneraSchemaSettimanale(totaleEffettivo)
}
```

**5.2 Funzione generazione schema settimanale**

```
FUNZIONE GeneraSchemaSettimanale(doseSettimanale: Decimal) → Array[7]

doseBase = Floor(doseSettimanale / 7, 0.5)  // arrotonda a 0.5 mg
resto = doseSettimanale - (doseBase * 7)

schema = Array[7] riempito con doseBase

// Distribuisci il resto uniformemente
giorniExtra = Ceiling(resto / 0.5)
passo = 7 / giorniExtra

PER i = 0 FINO A giorniExtra - 1:
    indice = Floor(i * passo)
    schema[indice] += 0.5
FINE PER

RITORNA schema
```

**5.3 Funzione calcolo data prossimo controllo**

```
FUNZIONE CalcolaDataProssimoControllo(
    dataOggi: Date,
    fasciaINR: Enum,
    inRangeConsecutivi: Int,
    aggiustamentoEffettuato: Boolean
) → DateRange

SE aggiustamentoEffettuato ALLORA
    // Dopo aggiustamento, controllo ravvicinato
    SWITCH fasciaINR:
        CASO SUB_CRITICO, SOVRA_ESTREMO, SOVRA_CRITICO:
            RITORNA (dataOggi + 4 giorni, dataOggi + 7 giorni)
        CASO SUB_MODERATO, SOVRA_MOLTO_ALTO, SOVRA_ALTO:
            RITORNA (dataOggi + 5 giorni, dataOggi + 8 giorni)
        CASO SUB_LIEVE, SOVRA_MODERATO, SOVRA_LIEVE:
            RITORNA (dataOggi + 7 giorni, dataOggi + 14 giorni)
        DEFAULT:
            RITORNA (dataOggi + 7 giorni, dataOggi + 14 giorni)
    FINE SWITCH
    
ALTRIMENTI SE fasciaINR = IN_RANGE ALLORA
    // Algoritmo progressivo FCSA
    SWITCH inRangeConsecutivi:
        CASO 1:
            RITORNA (dataOggi + 5 giorni, dataOggi + 10 giorni)
        CASO 2:
            RITORNA (dataOggi + 14 giorni, dataOggi + 14 giorni)
        CASO 3:
            RITORNA (dataOggi + 21 giorni, dataOggi + 21 giorni)
        CASO >= 4:
            RITORNA (dataOggi + 28 giorni, dataOggi + 28 giorni)  // Max 4 settimane
    FINE SWITCH
FINE SE
```

---

### SEZIONE 6: ALGORITMO DI FOLLOW-UP

**6.1 Diagramma di flusso principale**

```
PROCEDURA AlgoritmoGestioneTAO(paziente, controlloAttuale)

1. VALIDA INPUT
   - Verifica completezza dati
   - Calcola dose settimanale se non presente
   
2. CLASSIFICA INR
   target = paziente.RangeTerapeutico
   limiteInferiore = SE target = "2.0-3.0" ALLORA 2.0 ALTRIMENTI 2.5
   limiteSuperiore = SE target = "2.0-3.0" ALLORA 3.0 ALTRIMENTI 3.5
   
   SE controlloAttuale.INR >= limiteInferiore E 
      controlloAttuale.INR <= limiteSuperiore ALLORA
       statoINR = "IN_RANGE"
   ALTRIMENTI SE controlloAttuale.INR < limiteInferiore ALLORA
       statoINR = "SOTTOTERAPEUTICO"
   ALTRIMENTI
       statoINR = "SOVRATERAPEUTICO"
   FINE SE

3. VERIFICA EMORRAGIA
   SE controlloAttuale.PresenzaSanguinamento = TRUE ALLORA
       tipoEmorragia = ClassificaEmorragia(controlloAttuale)
       RICHIEDI conferma utente su classificazione
   FINE SE

4. GENERA RACCOMANDAZIONE
   SWITCH statoINR:
       CASO "IN_RANGE":
           raccomandazione = GestisciINRInRange(paziente, controlloAttuale)
       CASO "SOTTOTERAPEUTICO":
           rischioAlto = CalcolaRischioTromboticoElevato(paziente)
           raccomandazione = GestisciINRBasso(controlloAttuale.INR, target, 
                                              paziente.DoseSettimanale, rischioAlto)
       CASO "SOVRATERAPEUTICO":
           SE controlloAttuale.PresenzaSanguinamento ALLORA
               raccomandazione = GestisciINRAltoConEmorragia(controlloAttuale.INR, 
                                                              tipoEmorragia,
                                                              paziente.DoseSettimanale)
           ALTRIMENTI
               raccomandazione = GestisciINRAltoSenzaEmorragia(controlloAttuale.INR, 
                                                                target,
                                                                paziente.DoseSettimanale)
           FINE SE
   FINE SWITCH

5. CALCOLA OUTPUT NUMERICI
   nuovaDose = CalcolaNuovaDoseSettimanale(...)
   dataControllo = CalcolaDataProssimoControllo(...)

6. GENERA REPORT
   RITORNA RaccomandazioneCompleta {
       TestoRaccomandazione,
       NuovaDoseSettimanale,
       SchemaDosiGiornaliere,
       DoseSupplementarePrimoGiorno,
       VitaminaK_Necessaria,
       DoseVitaminaK,
       ViaVitaminaK,
       EBPM_Necessaria,
       DoseEBPM,
       DataProssimoControllo,
       Urgenza,
       NoteAggiuntive,
       FonteRaccomandazione  // "FCSA" o "ACCP/sintesi"
   }

FINE PROCEDURA
```

---

### SEZIONE 7: TABELLA RIEPILOGATIVA DECISIONALE

**7.1 INR Sottoterapeutico (Target 2.0-3.0)**

| INR | Variazione dose | Dose boost D1 | EBPM se alto rischio | Controllo |
|-----|-----------------|---------------|---------------------|-----------|
| <1.5 | +15-20% | +7.5-10% sett. | Sì (70 UI/kg x2) | 5-7 gg |
| 1.5-1.79 | +7.5-15% | +5% sett. (opz) | Valutare | 7-10 gg |
| 1.8-1.99 | +5-10% (se necessario) | No | No | 10-14 gg |

**7.2 INR Sovraterapeutico senza emorragia (Target 2.0-3.0)**

| INR | Sospensione | Variazione dose | Vitamina K | Controllo |
|-----|-------------|-----------------|------------|-----------|
| 3.1-3.4 | No | -5-10% | No | 7-14 gg |
| 3.5-3.9 | 1 dose (opz) | -10-15% | No | 5-8 gg |
| 4.0-4.9 | 1 dose | -10-15% | No | 4-8 gg |
| 5.0-5.9 | 1-2 dosi | -15-20% | 2 mg PO (opz) | 24h poi 4-7 gg |
| 6.0-7.9 | 2-3 dosi | -20% | 2-3 mg PO | 24-48h |
| ≥8.0 | Fino a INR OK | -20-50% | 3-5 mg PO | 24-48h |

**7.3 INR Sovraterapeutico con emorragia**

| Tipo emorragia | Vitamina K | Via | Fattori procoagulanti | Setting |
|----------------|------------|-----|----------------------|---------|
| Minore | 2-5 mg | PO | No | Ambulatorio/Valutare |
| Maggiore | 10 mg | EV lenta | PCC 20-50 UI/kg (o plasma 15 mL/kg) | Ricovero |
| Rischio vitale | 10 mg | EV lenta | PCC 20-50 UI/kg (preferito) | Terapia intensiva |

---

### SEZIONE 8: STRUTTURE DATI SUGGERITE (C#)

```csharp
public enum RangeTerapeutico
{
    Standard_2_3,      // INR 2.0-3.0
    Intensivo_2_5_3_5  // INR 2.5-3.5
}

public enum FasciaINR
{
    InRange,
    SubCritico,
    SubModerato,
    SubLieve,
    SovraLieve,
    SovraModerato,
    SovraAlto,
    SovraMoltoAlto,
    SovraCritico,
    SovraEstremo
}

public enum TipoEmorragia
{
    Nessuna,
    Minore,
    Maggiore,
    RischioVitale
}

public enum SedeEmorragia
{
    Nessuna,
    Cutanea,
    Nasale,
    Gengivale,
    Gastrointestinale,
    Urinaria,
    Intracranica,
    Retroperitoneale,
    Altra
}

public class RaccomandazioneTAO
{
    public string TestoRaccomandazione { get; set; }
    public decimal NuovaDoseSettimanale { get; set; }
    public decimal[] SchemaGiornaliero { get; set; } // 7 elementi
    public decimal? DoseSupplementarePrimoGiorno { get; set; }
    public bool VitaminaKNecessaria { get; set; }
    public decimal? DoseVitaminaK_mg { get; set; }
    public string ViaVitaminaK { get; set; } // "PO" o "EV"
    public bool EBPMNecessaria { get; set; }
    public string DoseEBPM { get; set; }
    public DateTime DataControlloMin { get; set; }
    public DateTime DataControlloMax { get; set; }
    public string Urgenza { get; set; } // "Routine", "Urgente", "Emergenza"
    public List<string> NoteAggiuntive { get; set; }
    public string FonteRaccomandazione { get; set; } // "FCSA", "ACCP", "Sintesi"
}
```

---

Questo schema dovrebbe fornirti una base solida per l'implementazione. Vuoi che proceda con la creazione di un documento formale in formato DOCX o PDF, oppure preferisci che sviluppi direttamente il codice C# delle classi e degli algoritmi principali?
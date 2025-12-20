# Griglia di Valutazione Pre-TAO

## Panoramica

La griglia di valutazione pre-TAO è uno strumento clinico completo per la stratificazione del rischio trombotico ed emorragico prima di iniziare la terapia anticoagulante orale (TAO) con warfarin.

## Componenti Principali

### 1. Score CHA₂DS₂-VASc (Rischio Trombotico)

**Range**: 0-9 punti

**Componenti**:
- **C** - Scompenso cardiaco congestizio / disfunzione VS: 1 punto
- **H** - Ipertensione arteriosa: 1 punto
- **A₂** - Età ≥75 anni: **2 punti**
- **D** - Diabete mellito: 1 punto
- **S₂** - Stroke/TIA/TE pregresso: **2 punti**
- **V** - Malattia vascolare (IMA, PAD, placca aortica): 1 punto
- **A** - Età 65-74 anni: 1 punto
- **Sc** - Sesso femminile: 1 punto

**Interpretazione**:
- **0 punti**: Rischio basso
- **1 punto**: Rischio basso-moderato
- **≥2 punti**: Rischio alto (indicazione alla TAO)

### 2. Score HAS-BLED (Rischio Emorragico)

**Range**: 0-9 punti

**Componenti**:
- **H** - Hypertension: PAS >160 mmHg: 1 punto
- **A** - Abnormal renal function: dialisi, trapianto, Cr >2.26 mg/dL: 1 punto
- **A** - Abnormal liver function: cirrosi, bilirubina >2x, AST/ALT/ALP >3x: 1 punto
- **S** - Stroke: storia di stroke: 1 punto
- **B** - Bleeding: storia di sanguinamento maggiore o predisposizione: 1 punto
- **L** - Labile INR: TTR <60% (se già in TAO): 1 punto
- **E** - Elderly: età >65 anni: 1 punto
- **D** - Drugs: farmaci concomitanti (antipiastrinici, FANS): 1 punto
- **D** - Drugs/Alcohol: abuso di alcol ≥8 drink/settimana: 1 punto

**Interpretazione**:
- **0 punti**: Rischio emorragico basso
- **1-2 punti**: Rischio emorragico moderato
- **≥3 punti**: Rischio emorragico alto (cautela, ma non esclude TAO)

### 3. Controindicazioni Assolute

**IMPORTANTE**: La presenza di QUALSIASI di questi criteri ESCLUDE la terapia anticoagulante.

1. **Emorragia maggiore in atto o recente** (<3 mesi)
2. **Gravidanza in corso**
3. **Discrasia ematica severa**
   - Grave trombocitopenia (<50.000/μL)
   - Coagulopatia congenita o acquisita
4. **Recente intervento neurochirurgico o oculare** (<1 mese)
5. **Emorragia intracranica recente** o malformazione vascolare cerebrale
6. **Ulcera peptica attiva** o varici esofagee a rischio
7. **Endocardite batterica acuta**
8. **Ipertensione severa non controllata** (PAS >200 mmHg, PAD >120 mmHg)
9. **Allergia documentata al warfarin**
10. **Mancanza di compliance/supervisione**

### 4. Controindicazioni Relative

**NOTA**: Richiedono valutazione attenta del rapporto rischio/beneficio.

1. **Recente emorragia gastrointestinale** (3-6 mesi)
2. **Storia di sanguinamenti maggiori** (>6 mesi fa)
3. **Insufficienza renale moderata** (GFR 30-60 mL/min)
4. **Insufficienza epatica moderata** (Child-Pugh B)
5. **Piastrinopenia moderata** (50.000-100.000/μL)
6. **Cadute frequenti** / rischio traumatico elevato
7. **Demenza o deficit cognitivo**
8. **Recente chirurgia maggiore** (1-3 mesi)
9. **Lesioni organiche a rischio** (aneurismi, neoplasie)
10. **Pericardite acuta**

### 5. Fattori Favorenti Eventi Avversi

**NOTA**: Non controindicano la TAO, ma aumentano il rischio di eventi avversi e richiedono monitoraggio intensivo.

1. **Politerapia** (>5 farmaci concomitanti)
2. **Isolamento sociale** / difficoltà di accesso al follow-up
3. **Interazioni farmacologiche note**
4. **Dieta irregolare o ricca di vitamina K**
5. **IMC estremo** (<18 o >35 kg/m²)
6. **Anemia cronica** (Hb <10 g/dL)
7. **Patologie oncologiche attive**
8. **Procedura invasiva programmata** (entro 3 mesi)
9. **Varianti genetiche note** (CYP2C9, VKORC1)

## Valutazione Globale

Il sistema fornisce una valutazione automatica basata sui punteggi:

### CONTROINDICATO
- Presenza di controindicazioni assolute

### ATTENZIONE
- HAS-BLED ≥ CHA₂DS₂-VASc E HAS-BLED ≥3
- Presenza di ≥3 controindicazioni relative

### INDICATO
- CHA₂DS₂-VASc ≥2 E HAS-BLED ≤2
- Chiaro beneficio dalla TAO

### DA VALUTARE
- CHA₂DS₂-VASc = 1: considerare TAO vs altri anticoagulanti
- CHA₂DS₂-VASc = 0: rischio tromboembolico basso, TAO non indicata
- Altre combinazioni: valutazione rischio/beneficio individualizzata

## Integrazione nell'App

### Posizionamento

La griglia di valutazione pre-TAO è integrata nella sezione **Anagrafica** del paziente, accessibile durante:
- Creazione nuovo paziente
- Modifica dati paziente esistente
- Prima prescrizione TAO

### Flusso di Lavoro

1. **Compilazione anagrafica**: dati personali del paziente
2. **Valutazione pre-TAO**: compilazione griglia rischi
3. **Revisione scores**: verifica automatica CHA₂DS₂-VASc e HAS-BLED
4. **Decisione clinica**: approvazione o controindicazione
5. **Salvataggio**: registrazione valutazione nel database

### Funzionalità

#### Auto-popolamento
- Età e sesso dal paziente
- Dati comorbidità già inseriti (scompenso, ipertensione, diabete, malattia vascolare)

#### Calcolo Real-time
- Score CHA₂DS₂-VASc aggiornato live
- Score HAS-BLED aggiornato live
- Valutazione globale dinamica

#### Storico
- Tutte le valutazioni vengono salvate con timestamp
- Possibilità di visualizzare valutazioni precedenti
- Tracciabilità delle decisioni cliniche

#### Note e Raccomandazioni
- Campo note cliniche libero
- Campo raccomandazioni
- Identificazione medico valutatore
- Flag approvazione

## Riferimenti Clinici

### CHA₂DS₂-VASc
- European Heart Rhythm Association (EHRA) 2020
- ESC Guidelines for Atrial Fibrillation 2020
- Lip GY, et al. Thromb Haemost 2010

### HAS-BLED
- Pisters R, et al. Chest 2010
- ESC Guidelines for Atrial Fibrillation 2020
- Non controindicazione assoluta alla TAO, ma strumento per identificare pazienti a rischio

### Linee Guida Italiane
- AIFA - Nota 97 (anticoagulanti orali)
- FCSA (Federazione Centri Sorveglianza Anticoagulati)
- Linee guida regionali per la gestione TAO

## Considerazioni Implementative

### Database
- Entità `PreTaoAssessment` con relazione 1:N con `Patient`
- Tutti i campi booleani per checkbox
- Campi calcolati per scores (non salvati su DB)
- Timestamp per tracciabilità

### UI/UX
- Layout a 3 colonne per panoramica scores
- Sezioni colorate per evidenziare criticità
- Controindicazioni assolute in rosso (evidenza massima)
- Controindicazioni relative in arancione
- Aggiornamento real-time dei punteggi
- Interfaccia intuitiva con checkbox descrittivi

### Validazione
- Nessuna validazione obbligatoria (tutti i campi opzionali)
- Alert se controindicazioni assolute presenti
- Warning se HAS-BLED elevato
- Suggerimento se TAO non indicata (score basso)

## Utilizzo Clinico

### Prima Valutazione
1. Compilare tutti i campi rilevanti
2. Verificare scores automatici
3. Escludere controindicazioni assolute
4. Valutare controindicazioni relative
5. Considerare fattori favorenti eventi avversi
6. Documentare decisione e raccomandazioni

### Rivalutazione Periodica
- Annuale per pazienti stabili
- Dopo eventi avversi
- In caso di cambio terapia concomitante
- Prima di procedure invasive

### Casi Borderline
Per pazienti con:
- CHA₂DS₂-VASc = 1
- HAS-BLED elevato ma CHA₂DS₂-VASc ancora più alto
- Multiple controindicazioni relative

Considerare:
- Discussione collegiale
- Coinvolgimento paziente nella decisione
- Valutazione alternative (NAO, chiusura LAA)
- Documentazione dettagliata del ragionamento

## Limitazioni

- Gli score sono strumenti di supporto, non sostituiscono il giudizio clinico
- Validati principalmente per fibrillazione atriale
- CHA₂DS₂-VASc può sottostimare rischio in alcune patologie valvolari
- HAS-BLED non è una controindicazione assoluta alla TAO
- Necessario considerare il contesto clinico completo

## Aggiornamenti Futuri

### Pianificati
- [ ] Export PDF della valutazione
- [ ] Grafico storico delle valutazioni
- [ ] Confronto pre/post intervento
- [ ] Integrazione con calcolo dose iniziale
- [ ] Alert automatici per rivalutazione periodica

### In Valutazione
- [ ] Score aggiuntivi (ATRIA, ORBIT)
- [ ] Integrazione con linee guida NAO
- [ ] Calcolatore rischio emorragico specifico
- [ ] Decision support system con IA

---

**Versione**: 1.0
**Data**: 2025-01-27
**Autore**: Sistema TaoGEST
**Revisione**: Da programmare annualmente

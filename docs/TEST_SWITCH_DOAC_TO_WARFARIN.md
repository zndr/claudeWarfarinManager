# 🧪 PIANO DI TEST - SWITCH DOAC → WARFARIN

## Obiettivo
Verificare la correttezza dei protocolli di switch da DOAC a Warfarin per tutti i 4 farmaci, con particolare attenzione a:
- Calcolo corretto della timeline
- Gestione funzione renale
- Bridging EBPM vs Overlap
- Controindicazioni e warnings
- Salvataggio database

---

## TEST SUITE 1: DABIGATRAN → WARFARIN

### Test 1.1: Paziente con ClCr ≥50 (Overlap 3 giorni)

**Input:**
```
Direzione: DOAC → Warfarin
DOAC: Dabigatran
Warfarin: Warfarin (Coumadin)

Paziente:
- Età: 68 anni
- Peso: 75 kg
- Sesso: M
- ClCr: 65 mL/min
```

**Output Atteso:**
```
✅ Dosaggio DOAC: "150 mg BID (due volte al giorno)"
✅ Rationale: "Dose standard"

Timeline:
📅 Giorno 0: Iniziare Warfarin mantenendo Dabigatran
   - Assumere Warfarin (dose iniziale 5 mg/die) E continuare Dabigatran 150mg BID

📅 Giorno 1: Continuare Warfarin + Dabigatran
   - Assumere entrambi. Controllare INR prima della dose di Dabigatran

📅 Giorno 2: Continuare Warfarin + Dabigatran
   - Assumere entrambi. Controllare INR prima della dose di Dabigatran

📅 Giorno 3: Sospendere Dabigatran
   - Non assumere più Dabigatran. Continuare solo Warfarin. Controllare INR

📅 Giorno 4: Monitoraggio INR
   - Controllo INR quotidiano fino a stabilizzazione nel range 2-3

Note Cliniche:
📌 OVERLAP GRADUATO: Il Dabigatran viene continuato per 3 giorni (ClCr 65.0 mL/min ≥50)
📌 L'INR va misurato PRIMA della dose di Dabigatran (a valle) per evitare interferenze
📌 Questo metodo evita periodi di scopertura anticoagulante
```

### Test 1.2: Paziente con ClCr 30-50 (Overlap 2 giorni)

**Input:**
```
Età: 78 anni
Peso: 62 kg
Sesso: F
Creatinina sierica: 1.8 mg/dL
→ ClCr calcolata: ~35 mL/min
```

**Output Atteso:**
```
✅ Dosaggio: "110 mg BID" (ridotto per ClCr 30-50)
✅ Timeline: Overlap 2 giorni
📅 Giorno 2: Sospendere Dabigatran
```

### Test 1.3: Paziente con ClCr 15-30 (Overlap 1 giorno)

**Input:**
```
Età: 85 anni
Peso: 55 kg
Sesso: F
ClCr manuale: 25 mL/min
```

**Output Atteso:**
```
✅ Dosaggio: "110 mg BID" (ridotto per ClCr 30-50, età ≥80)
⚠️ WARNING: ClCr 25.0 mL/min. Funzione renale severamente ridotta
✅ Timeline: Overlap 1 giorno
📅 Giorno 1: Sospendere Dabigatran
```

### Test 1.4: Controindicazione - ClCr <30

**Input:**
```
ClCr: 28 mL/min
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE: "ClCr 28.0 mL/min. Dabigatran controindicato se ClCr <30 mL/min"
❌ Switch NON RACCOMANDATO
```

---

## TEST SUITE 2: EDOXABAN → WARFARIN

### Test 2.1: Paziente con dose standard 60mg

**Input:**
```
Direzione: DOAC → Warfarin
DOAC: Edoxaban
Warfarin: Warfarin

Paziente:
- Età: 70 anni
- Peso: 80 kg
- Sesso: M
- ClCr: 70 mL/min
```

**Output Atteso:**
```
✅ Dosaggio originale: "60 mg una volta al giorno"
✅ Dose ridotta per switch: "30 mg"

Timeline:
📅 Giorno 0: Ridurre Edoxaban a metà dose e iniziare Warfarin
   - Ridurre Edoxaban da 60 mg a 30 mg
   - Iniziare Warfarin (dose 5 mg/die)

📅 Giorno 1-3: Continuare Edoxaban 30mg + Warfarin
   - Controllare INR prima della dose di Edoxaban

📅 Giorno 4: Controllo INR decisionale
   - Se INR ≥2.0: sospendere Edoxaban
   - Se INR <2.0: continuare overlap un altro giorno

Note Cliniche:
📌 METODO A METÀ DOSE: Edoxaban ridotto a metà dose durante introduzione Warfarin
📌 Schema validato nello studio ENGAGE AF-TIMI 48
```

### Test 2.2: Paziente già in dose ridotta 30mg

**Input:**
```
Peso: 58 kg (≤60)
ClCr: 45 mL/min
```

**Output Atteso:**
```
✅ Dosaggio originale: "30 mg" (ridotto per peso ≤60 kg, ClCr 30-50)
✅ Dose ridotta per switch: "15 mg"

📅 Giorno 0: Ridurre Edoxaban da 30 mg a 15 mg + Warfarin
```

### Test 2.3: Controindicazione - ClCr >95 in FA

**Input:**
```
ClCr: 105 mL/min
```

**Output Atteso:**
```
⚠️ NON RACCOMANDATO: ClCr 105.0 mL/min >95.
   Edoxaban ha efficacia ridotta in FA con ClCr molto elevata
```

---

## TEST SUITE 3: APIXABAN → WARFARIN

### Test 3.1: Paziente dose standard 5mg BID

**Input:**
```
Direzione: DOAC → Warfarin
DOAC: Apixaban
Warfarin: Acenocumarolo (Sintrom)

Paziente:
- Età: 65 anni
- Peso: 85 kg
- Sesso: M
- ClCr: 80 mL/min
- Creatinina: 1.0 mg/dL
```

**Output Atteso:**
```
✅ Dosaggio: "5 mg BID" (dose standard)

Timeline:
📅 Giorno 0: Sospendere Apixaban e iniziare EBPM + Warfarin
   - Non assumere più Apixaban
   - Iniziare Eparina a Basso Peso Molecolare (es. Enoxaparina 1 mg/kg ogni 12h)
   - Iniziare Acenocumarolo (dose iniziale 5 mg/die o 2-3 mg in anziani)

📅 Giorno 1-5: Continuare EBPM + Warfarin
   - Mantenere sovrapposizione EBPM + Warfarin
   - Controllo INR quotidiano

📅 Giorno 6: Valutazione INR per sospensione EBPM
   - Se INR ≥2.0 per due controlli consecutivi: sospendere EBPM
   - Se INR <2.0: continuare EBPM + Warfarin

📅 Giorno 7: Monitoraggio INR
   - Controllo INR ogni 2-3 giorni fino a stabilizzazione 2-3

Note Cliniche:
📌 BRIDGING CON EPARINA: Apixaban ha emivita breve (~12 ore)
📌 EBPM garantisce anticoagulazione continua durante i 5-7 giorni necessari
📌 Sospendere EBPM solo quando INR stabilmente ≥2.0 per almeno 24-48h
```

### Test 3.2: Paziente dose ridotta 2.5mg BID (criteri ABC)

**Input:**
```
Età: 82 anni (≥80) ✓
Peso: 58 kg (≤60) ✓
Creatinina: 1.6 mg/dL (≥1.5) ✓
→ 3/3 criteri ABC soddisfatti
```

**Output Atteso:**
```
✅ Dosaggio: "2.5 mg BID"
✅ Rationale: "Dose ridotta per criteri ABC: età ≥80 anni, peso ≤60 kg, creatinina ≥1.5 mg/dL"

⚠️ WARNING: Età 82 anni. Paziente molto anziano
⚠️ WARNING: Peso 58.0 kg <50 kg [se <50]
```

### Test 3.3: Paziente con 2/3 criteri ABC

**Input:**
```
Età: 81 anni (≥80) ✓
Peso: 65 kg (>60) ✗
Creatinina: 1.7 mg/dL (≥1.5) ✓
→ 2/3 criteri ABC soddisfatti
```

**Output Atteso:**
```
✅ Dosaggio: "2.5 mg BID"
✅ Rationale: "Dose ridotta per criteri ABC: età ≥80 anni, creatinina ≥1.5 mg/dL"
```

### Test 3.4: Paziente con 1/3 criteri ABC (NO riduzione)

**Input:**
```
Età: 79 anni (<80) ✗
Peso: 70 kg (>60) ✗
Creatinina: 1.8 mg/dL (≥1.5) ✓
→ 1/3 criteri: dose standard
```

**Output Atteso:**
```
✅ Dosaggio: "5 mg BID"
✅ Rationale: "Dose standard"
```

---

## TEST SUITE 4: RIVAROXABAN → WARFARIN

### Test 4.1: Paziente dose standard 20mg

**Input:**
```
Direzione: DOAC → Warfarin
DOAC: Rivaroxaban
Warfarin: Warfarin

Paziente:
- Età: 72 anni
- Peso: 78 kg
- Sesso: M
- ClCr: 75 mL/min
```

**Output Atteso:**
```
✅ Dosaggio: "20 mg una volta al giorno (con il pasto)"
✅ Rationale: "Dose standard"

ℹ️ IMPORTANTE: Rivaroxaban 15-20 mg deve essere assunto CON IL CIBO

Timeline:
📅 Giorno 0: Sospendere Rivaroxaban e iniziare EBPM + Warfarin
   [Stesso protocollo di Apixaban]

Note Cliniche:
📌 BRIDGING CON EPARINA: Rivaroxaban ha emivita ~5-13 ore
📌 L'EBPM garantisce anticoagulazione continua
ℹ️ Alternativa (basso rischio): In pazienti selezionati a basso rischio trombotico,
   è teoricamente possibile sovrapporre Rivaroxaban + Warfarin senza EBPM,
   ma l'approccio con bridging è più sicuro
```

### Test 4.2: Paziente dose ridotta 15mg (ClCr <50)

**Input:**
```
Età: 75 anni
Peso: 68 kg
Sesso: F
ClCr: 42 mL/min
```

**Output Atteso:**
```
✅ Dosaggio: "15 mg una volta al giorno (con il pasto)"
✅ Rationale: "Dose ridotta per ClCr 42.0 mL/min (15-49 mL/min)"

⚠️ WARNING: ClCr 42.0 mL/min. Insufficienza renale moderata
   Monitorare funzione renale ogni 6 mesi
```

### Test 4.3: Controindicazione - ClCr <15

**Input:**
```
ClCr: 12 mL/min
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: ClCr 12.0 mL/min.
   Tutti i DOAC sono controindicati se ClCr <15 mL/min
```

---

## TEST SUITE 5: CONTROINDICAZIONI COMUNI

### Test 5.1: Valvole meccaniche

**Input:**
```
DOAC: Apixaban
Controindicazioni:
☑ Valvole meccaniche
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: Presenza di valvole meccaniche.
   I DOAC sono controindicati.
❌ SWITCH NON RACCOMANDATO
```

### Test 5.2: Stenosi mitralica

**Input:**
```
DOAC: Rivaroxaban
Controindicazioni:
☑ Stenosi mitralica moderata/severa
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: Stenosi mitralica moderata/severa.
   I DOAC sono controindicati.
```

### Test 5.3: Gravidanza

**Input:**
```
DOAC: Edoxaban
Controindicazioni:
☑ Gravidanza o allattamento
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: Gravidanza o allattamento.
   I DOAC sono controindicati.
```

### Test 5.4: Sindrome antifosfolipidi

**Input:**
```
DOAC: Dabigatran
Controindicazioni:
☑ Sindrome da antifosfolipidi
```

**Output Atteso:**
```
⚠️ CONTROINDICAZIONE RELATIVA: Sindrome da antifosfolipidi.
   I DOAC hanno mostrato risultati inferiori a Warfarin in APS ad alto rischio
```

---

## TEST SUITE 6: WARNINGS

### Test 6.1: Peso elevato >120kg

**Input:**
```
Peso: 135 kg
```

**Output Atteso:**
```
⚠️ ATTENZIONE: Peso 135.0 kg >120 kg.
   Evidenza limitata per DOAC in pazienti con peso molto elevato.
   Considerare monitoraggio più stretto.
```

### Test 6.2: Peso basso <50kg

**Input:**
```
Peso: 47 kg
```

**Output Atteso:**
```
⚠️ ATTENZIONE: Peso 47.0 kg <50 kg.
   Rischio di concentrazioni plasmatiche più elevate.
   Considerare dosaggio ridotto e monitoraggio.
```

### Test 6.3: Età molto avanzata ≥85

**Input:**
```
Età: 88 anni
```

**Output Atteso:**
```
⚠️ ATTENZIONE: Età 88 anni.
   Paziente molto anziano: valutare rischio emorragico,
   aderenza terapeutica e rischio cadute.
```

---

## TEST SUITE 7: CALCOLO CLEARANCE CREATININA

### Test 7.1: Calcolo Cockcroft-Gault - Uomo

**Input:**
```
Età: 70 anni
Peso: 80 kg
Sesso: M
Creatinina sierica: 1.2 mg/dL
```

**Formula Cockcroft-Gault:**
```
ClCr = [(140 - età) × peso × (0.85 se F)] / (72 × creatinina)
ClCr = [(140 - 70) × 80 × 1.0] / (72 × 1.2)
ClCr = [70 × 80] / 86.4
ClCr = 5600 / 86.4
ClCr = 64.8 mL/min
```

**Output Atteso:**
```
✅ ClCr Calcolata: 64.8 mL/min
```

### Test 7.2: Calcolo Cockcroft-Gault - Donna

**Input:**
```
Età: 75 anni
Peso: 65 kg
Sesso: F
Creatinina sierica: 1.5 mg/dL
```

**Formula:**
```
ClCr = [(140 - 75) × 65 × 0.85] / (72 × 1.5)
ClCr = [65 × 65 × 0.85] / 108
ClCr = 3591.25 / 108
ClCr = 33.3 mL/min
```

**Output Atteso:**
```
✅ ClCr Calcolata: 33.3 mL/min
⚠️ WARNING: ClCr 33.3 mL/min. Insufficienza renale moderata
```

---

## TEST SUITE 8: SALVATAGGIO DATABASE

### Test 8.1: Verifica salvataggio completo

**Dopo aver generato e salvato un protocollo:**

**Verificare nel database `TherapySwitches`:**
```sql
SELECT
    Id,
    PatientId,
    SwitchDate,
    Direction,
    DoacType,
    WarfarinType,
    CreatinineClearance,
    RecommendedDosage,
    FirstFollowUpDate,
    FollowUpCompleted,
    SwitchCompleted
FROM TherapySwitches
ORDER BY SwitchDate DESC
LIMIT 1;
```

**Output Atteso:**
```
✅ Record salvato con:
   - Direction: "DoacToWarfarin"
   - DoacType: "Dabigatran" (o altro)
   - WarfarinType: "Warfarin"
   - CreatinineClearance: valore corretto
   - RecommendedDosage: dosaggio calcolato
   - FirstFollowUpDate: +30 giorni da oggi
   - FollowUpCompleted: false
   - SwitchCompleted: false
```

### Test 8.2: Verifica JSON serializzati

**Verificare campi JSON:**
```sql
SELECT
    ProtocolTimeline,
    Contraindications,
    Warnings,
    ClinicalNotes
FROM TherapySwitches
WHERE Id = [ultimo_id];
```

**Output Atteso:**
```
✅ ProtocolTimeline: JSON array con tutti gli step
✅ Contraindications: JSON array (o NULL se nessuna)
✅ Warnings: JSON array (o NULL se nessuno)
✅ ClinicalNotes: testo con note separate da \n
```

---

## TEST SUITE 9: PIANO DI MONITORAGGIO

### Test 9.1: Verifica piano DOAC → Warfarin

**Output Atteso:**
```
📋 PIANO DI MONITORAGGIO POST-SWITCH:
• Controllo INR frequente nei primi 7-14 giorni (ogni 2-3 giorni) fino a stabilizzazione
• Target INR: 2.0-3.0 per la maggior parte delle indicazioni
• Dopo stabilizzazione: controllo INR ogni 2-4 settimane
• Calcolare Time in Therapeutic Range (TTR) dopo 3 mesi
• Controllo emocromo e funzione renale/epatica secondo necessità clinica
• Educare il paziente su interazioni farmacologiche e alimentari del Warfarin
• Follow-up clinico a 1 mese per verificare stabilità INR e assenza di eventi avversi
```

---

## CHECKLIST GENERALE DI TEST

### ✅ Funzionalità UI

- [ ] Form si carica correttamente
- [ ] Selezione direzione switch (radio button funzionano)
- [ ] Dropdown DOAC popolato correttamente
- [ ] Selezione Warfarin/Acenocumarolo funziona
- [ ] Input parametri paziente validati
- [ ] Toggle ClCr manuale/calcolato funziona
- [ ] Bottone "Calcola ClCr" calcola correttamente
- [ ] Checkbox controindicazioni funzionano
- [ ] Bottone "Genera Protocollo" attivo quando form valido
- [ ] Risultati visualizzati correttamente
- [ ] Bottone "Salva" funziona
- [ ] Bottone "Nuovo Calcolo" ricarica la pagina

### ✅ Business Logic

- [ ] Calcolo dosaggio Dabigatran corretto (età, ClCr)
- [ ] Calcolo dosaggio Edoxaban corretto (peso, ClCr)
- [ ] Calcolo dosaggio Apixaban corretto (criteri ABC)
- [ ] Calcolo dosaggio Rivaroxaban corretto (ClCr)
- [ ] Timeline Dabigatran con overlap variabile per ClCr
- [ ] Timeline Edoxaban con metà dose
- [ ] Timeline Apixaban/Rivaroxaban con EBPM
- [ ] Controindicazioni assolute bloccano lo switch
- [ ] Warnings visualizzati correttamente
- [ ] Note cliniche appropriate per ogni DOAC

### ✅ Database

- [ ] Migration applicata correttamente
- [ ] Tabella TherapySwitches creata
- [ ] Indici creati correttamente
- [ ] Salvataggio dati funziona
- [ ] Foreign key con Patients funziona
- [ ] JSON serializzato correttamente
- [ ] FirstFollowUpDate impostata a +30 giorni

### ✅ Integrazione

- [ ] Menu "Switch Terapia" visibile
- [ ] Click su menu apre finestra
- [ ] WebView2 carica HTML correttamente
- [ ] Bridge JavaScript ↔ C# funziona
- [ ] Servizio ISwitchCalculatorService iniettato
- [ ] ViewModel riceve parametri correttamente
- [ ] Errori gestiti gracefully

---

## 🎯 RISULTATI ATTESI GLOBALI

Tutti i test devono passare con:
- ✅ **0 errori** di calcolo
- ✅ **100% accuratezza** nelle timeline
- ✅ **Tutte le controindicazioni** rilevate
- ✅ **Tutti i warnings** visualizzati
- ✅ **Salvataggio database** funzionante
- ✅ **UI responsive** e funzionale

---

## 📊 REPORT DI TEST

```
╔══════════════════════════════════════════════════╗
║         TEST SWITCH DOAC → WARFARIN              ║
╠══════════════════════════════════════════════════╣
║ Test Suite 1 (Dabigatran):        [ ] PASSED    ║
║ Test Suite 2 (Edoxaban):          [ ] PASSED    ║
║ Test Suite 3 (Apixaban):          [ ] PASSED    ║
║ Test Suite 4 (Rivaroxaban):       [ ] PASSED    ║
║ Test Suite 5 (Controindicazioni): [ ] PASSED    ║
║ Test Suite 6 (Warnings):          [ ] PASSED    ║
║ Test Suite 7 (Calcolo ClCr):      [ ] PASSED    ║
║ Test Suite 8 (Database):          [ ] PASSED    ║
║ Test Suite 9 (Monitoraggio):      [ ] PASSED    ║
╠══════════════════════════════════════════════════╣
║ TOTALE:                           [ ] / 9        ║
╚══════════════════════════════════════════════════╝
```

**Data Test:** _________________
**Tester:** _________________
**Build Version:** _________________

---

## 🔧 DEBUG TIPS

Se un test fallisce:

1. **Aprire DevTools** (F12 nella finestra Switch)
2. **Console Tab**: vedere errori JavaScript
3. **Network Tab**: verificare caricamento risorse
4. **Logs Serilog**: controllare `%LocalAppData%\WarfarinManager\Logs\`
5. **Database**: ispezionare con SQLite Browser

---

**Buon Testing! 🚀**

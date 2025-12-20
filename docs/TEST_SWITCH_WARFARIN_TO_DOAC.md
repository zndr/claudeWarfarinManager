# 🧪 PIANO DI TEST - SWITCH WARFARIN → DOAC

## Obiettivo
Verificare la correttezza dei protocolli di switch da Warfarin/Acenocumarolo a DOAC per tutti i 4 farmaci, con particolare attenzione a:
- Soglie INR corrette per ogni DOAC
- Calcolo tempo di attesa
- Dosaggio personalizzato
- No bridging necessario
- Controindicazioni specifiche

---

## TEST SUITE 1: WARFARIN → APIXABAN

### Test 1.1: Paziente standard (dose 5mg BID)

**Input:**
```
Direzione: Warfarin → DOAC
Warfarin: Warfarin (Coumadin)
DOAC: Apixaban

Paziente:
- Età: 65 anni
- Peso: 75 kg
- Sesso: M
- ClCr: 70 mL/min
- Creatinina: 1.1 mg/dL
- INR attuale: 2.8
```

**Output Atteso:**
```
✅ Dosaggio: "5 mg BID (due volte al giorno)"
✅ Rationale: "Dose standard"
✅ Soglia INR: 2.0

Timeline:
📅 Giorno 0: Sospendere Warfarin (Coumadin)
   - Ultima dose di Warfarin oggi. Non assumere più il farmaco

📅 Giorno 2: Controllare INR
   - Eseguire prelievo per INR. Obiettivo: INR ≤2.0

📅 Giorno 3: Iniziare Apixaban se INR ≤2.0
   - Dosaggio: 5 mg BID
   - Assumere solo se INR è sceso a ≤2.0
   - Se INR ancora elevato, ripetere controllo dopo 24h

Note Cliniche:
📌 PRINCIPIO 'Stop and Wait': Warfarin ha emivita 36-42 ore
📌 L'attesa media è di 3-4 giorni
📌 Non è necessario bridging con eparina
📌 INR attuale del paziente: 2.8
```

### Test 1.2: Paziente con criteri ABC (dose ridotta 2.5mg)

**Input:**
```
Età: 82 anni (≥80) ✓
Peso: 58 kg (≤60) ✓
Creatinina: 1.6 mg/dL (≥1.5) ✓
ClCr: 35 mL/min
INR attuale: 2.5
```

**Output Atteso:**
```
✅ Dosaggio: "2.5 mg BID (due volte al giorno)"
✅ Rationale: "Dose ridotta per criteri ABC: età ≥80 anni, peso ≤60 kg, creatinina ≥1.5 mg/dL"

⚠️ WARNING: Età 82 anni. Paziente molto anziano
⚠️ WARNING: Peso 58.0 kg <50 kg
⚠️ WARNING: ClCr 35.0 mL/min. Insufficienza renale moderata

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (obiettivo ≤2.0)
📅 Giorno 3: Iniziare Apixaban 2.5 mg BID se INR ≤2.0
```

### Test 1.3: INR già basso (switch immediato possibile)

**Input:**
```
Età: 70 anni
Peso: 80 kg
ClCr: 65 mL/min
Creatinina: 1.0 mg/dL
INR attuale: 1.8
```

**Output Atteso:**
```
✅ INR attuale (1.8) già sotto soglia (2.0)
✅ Può iniziare Apixaban immediatamente dopo sospensione Warfarin

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 1-2: Iniziare Apixaban 5 mg BID
   (può iniziare quando INR ≤2.0, già soddisfatto)
```

---

## TEST SUITE 2: WARFARIN → RIVAROXABAN

### Test 2.1: Paziente standard (dose 20mg)

**Input:**
```
Direzione: Warfarin → DOAC
Warfarin: Warfarin
DOAC: Rivaroxaban

Paziente:
- Età: 68 anni
- Peso: 78 kg
- Sesso: M
- ClCr: 75 mL/min
- INR attuale: 2.6
```

**Output Atteso:**
```
✅ Dosaggio: "20 mg una volta al giorno (con il pasto)"
✅ Rationale: "Dose standard"
✅ Soglia INR: 3.0 (più permissiva per Rivaroxaban)

ℹ️ IMPORTANTE: Rivaroxaban 15-20 mg deve essere assunto CON IL CIBO

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR
   - Obiettivo: INR ≤3.0

📅 Giorno 3: Iniziare Rivaroxaban se INR ≤3.0
   - Dosaggio: 20 mg die CON IL CIBO
   - Se INR ancora >3.0, attendere e ricontrollare dopo 24h

Note Cliniche:
📌 La soglia più permissiva (≤3.0) utile per switch rapidi
📌 L'assunzione col cibo è cruciale per l'assorbimento
```

### Test 2.2: Paziente con ClCr <50 (dose ridotta 15mg)

**Input:**
```
Età: 76 anni
Peso: 65 kg
Sesso: F
ClCr: 42 mL/min
INR attuale: 3.1
```

**Output Atteso:**
```
✅ Dosaggio: "15 mg una volta al giorno (con il pasto)"
✅ Rationale: "Dose ridotta per ClCr 42.0 mL/min (15-49 mL/min)"

⚠️ WARNING: ClCr 42.0 mL/min. Insufficienza renale moderata

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (obiettivo ≤2.5)
📅 Giorno 3-4: Iniziare Rivaroxaban 15 mg die quando INR ≤2.5
```

---

## TEST SUITE 3: WARFARIN → DABIGATRAN

### Test 3.1: Paziente giovane standard (dose 150mg BID)

**Input:**
```
Direzione: Warfarin → DOAC
Warfarin: Warfarin
DOAC: Dabigatran

Paziente:
- Età: 62 anni
- Peso: 82 kg
- Sesso: M
- ClCr: 85 mL/min
- INR attuale: 2.4
```

**Output Atteso:**
```
✅ Dosaggio: "150 mg BID (due volte al giorno)"
✅ Rationale: "Dose standard"
✅ Soglia INR: 2.0

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (obiettivo ≤2.0)
📅 Giorno 3: Iniziare Dabigatran 150 mg BID se INR ≤2.0

Note Cliniche:
📌 PRINCIPIO 'Stop and Wait': Warfarin emivita 36-42 ore
📌 L'attesa media è di 3-4 giorni
📌 Non è necessario bridging con eparina
```

### Test 3.2: Paziente ≥80 anni (dose ridotta 110mg)

**Input:**
```
Età: 83 anni (≥80)
Peso: 72 kg
Sesso: M
ClCr: 55 mL/min
INR attuale: 2.7
```

**Output Atteso:**
```
✅ Dosaggio: "110 mg BID (due volte al giorno)"
✅ Rationale: "Dose ridotta per età ≥80 anni"

⚠️ WARNING: Età 83 anni. Paziente molto anziano

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (obiettivo ≤2.0)
📅 Giorno 3: Iniziare Dabigatran 110 mg BID se INR ≤2.0
```

### Test 3.3: Controindicazione - ClCr <30

**Input:**
```
Età: 78 anni
Peso: 60 kg
ClCr: 28 mL/min
INR attuale: 2.5
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: ClCr 28.0 mL/min.
   Dabigatran controindicato se ClCr <30 mL/min.

❌ SWITCH NON RACCOMANDATO
```

### Test 3.4: ClCr 30-50 (dose ridotta 110mg)

**Input:**
```
Età: 72 anni
Peso: 68 kg
ClCr: 45 mL/min
```

**Output Atteso:**
```
✅ Dosaggio: "110 mg BID (due volte al giorno)"
✅ Rationale: "Dose ridotta per ClCr 45.0 mL/min (30-50 mL/min)"

⚠️ WARNING: ClCr 45.0 mL/min. Insufficienza renale moderata
```

---

## TEST SUITE 4: WARFARIN → EDOXABAN

### Test 4.1: Paziente standard (dose 60mg)

**Input:**
```
Direzione: Warfarin → DOAC
Warfarin: Warfarin
DOAC: Edoxaban

Paziente:
- Età: 70 anni
- Peso: 75 kg
- Sesso: M
- ClCr: 70 mL/min
- INR attuale: 2.9
```

**Output Atteso:**
```
✅ Dosaggio: "60 mg una volta al giorno"
✅ Rationale: "Dose standard"
✅ Soglia INR: 2.5

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR
   - Obiettivo: INR ≤2.5

📅 Giorno 3: Iniziare Edoxaban se INR ≤2.5
   - Dosaggio: 60 mg die
   - Se INR ancora >2.5, attendere 24h e ricontrollare

Note Cliniche:
📌 Soglia intermedia (2.5)
📌 Attenzione alla sovrastima dell'INR nei giorni successivi (interferenza analitica)
```

### Test 4.2: Peso ≤60kg (dose ridotta 30mg)

**Input:**
```
Età: 68 anni
Peso: 58 kg (≤60)
Sesso: F
ClCr: 75 mL/min
INR attuale: 2.3
```

**Output Atteso:**
```
✅ Dosaggio: "30 mg una volta al giorno"
✅ Rationale: "Dose ridotta per: peso ≤60 kg"

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (≤2.5)
📅 Giorno 3: Iniziare Edoxaban 30 mg die
```

### Test 4.3: ClCr 30-50 (dose ridotta 30mg)

**Input:**
```
Età: 74 anni
Peso: 72 kg
ClCr: 38 mL/min
INR attuale: 2.6
```

**Output Atteso:**
```
✅ Dosaggio: "30 mg una volta al giorno"
✅ Rationale: "Dose ridotta per: ClCr 38.0 mL/min (30-50 mL/min)"

⚠️ WARNING: ClCr 38.0 mL/min. Insufficienza renale moderata
```

### Test 4.4: Controindicazione - ClCr >95 in FA

**Input:**
```
Età: 55 anni
Peso: 85 kg
ClCr: 105 mL/min
INR attuale: 2.4
```

**Output Atteso:**
```
⚠️ NON RACCOMANDATO: ClCr 105.0 mL/min >95.
   Edoxaban ha efficacia ridotta in FA con ClCr molto elevata.
   Scegliere altro DOAC.

❌ Considerare Apixaban o Rivaroxaban invece
```

---

## TEST SUITE 5: ACENOCUMAROLO → DOAC

### Test 5.1: Acenocumarolo vs Warfarin (tempo di attesa ridotto)

**Input A (Warfarin):**
```
Warfarin: Warfarin (emivita 36-42h)
DOAC: Apixaban
INR attuale: 2.5
```

**Output Atteso A:**
```
Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR
📅 Giorno 3: Iniziare Apixaban se INR ≤2.0

Note: L'attesa media è di 3-4 giorni
```

**Input B (Acenocumarolo):**
```
Warfarin: Acenocumarolo (emivita 8-11h)
DOAC: Apixaban
INR attuale: 2.5
```

**Output Atteso B:**
```
Timeline:
📅 Giorno 0: Sospendere Acenocumarolo (Sintrom)
📅 Giorno 1: Controllare INR
📅 Giorno 2: Iniziare Apixaban se INR ≤2.0

Note: L'attesa media è di 2-3 giorni (più breve per Acenocumarolo)
```

---

## TEST SUITE 6: CONTROINDICAZIONI SPECIFICHE

### Test 6.1: Valvole meccaniche (tutti i DOAC)

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

❌ SWITCH NON RACCOMANDATO - Mantenere Warfarin
```

### Test 6.2: Stenosi mitralica

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

❌ SWITCH NON RACCOMANDATO - Mantenere Warfarin
```

### Test 6.3: Gravidanza/Allattamento

**Input:**
```
DOAC: Dabigatran
Controindicazioni:
☑ Gravidanza o allattamento
```

**Output Atteso:**
```
❌ CONTROINDICAZIONE ASSOLUTA: Gravidanza o allattamento.
   I DOAC sono controindicati.

❌ Nessun anticoagulante orale in gravidanza
   Switchare a EBPM (Eparina a basso peso molecolare)
```

### Test 6.4: Sindrome antifosfolipidi tripla positività

**Input:**
```
DOAC: Edoxaban
Controindicazioni:
☑ Sindrome da antifosfolipidi
```

**Output Atteso:**
```
⚠️ CONTROINDICAZIONE RELATIVA: Sindrome da antifosfolipidi.
   I DOAC hanno mostrato risultati inferiori a Warfarin in APS ad alto rischio
   (tripla positività).

⚠️ Valutare attentamente - preferibile mantenere Warfarin
```

---

## TEST SUITE 7: COMBINAZIONI COMPLESSE

### Test 7.1: Paziente con multipli fattori di riduzione dose

**Input (Apixaban):**
```
Età: 85 anni (≥80) ✓
Peso: 55 kg (≤60) ✓
Creatinina: 1.8 mg/dL (≥1.5) ✓
ClCr: 28 mL/min
INR attuale: 2.1
```

**Output Atteso:**
```
✅ Dosaggio: "2.5 mg BID"
✅ Rationale: "Dose ridotta per criteri ABC: età ≥80 anni, peso ≤60 kg, creatinina ≥1.5 mg/dL"

⚠️ WARNING: Età 85 anni. Paziente molto anziano
⚠️ WARNING: Peso 55.0 kg <50 kg
⚠️ WARNING: ClCr 28.0 mL/min. Funzione renale severamente ridotta

Note: Tutti e 3 i criteri ABC soddisfatti - massima cautela
```

### Test 7.2: Paziente obeso con funzione renale normale

**Input (Rivaroxaban):**
```
Età: 58 anni
Peso: 142 kg (>120)
Sesso: M
ClCr: 95 mL/min
INR attuale: 2.3
```

**Output Atteso:**
```
✅ Dosaggio: "20 mg una volta al giorno (con il pasto)"
✅ Rationale: "Dose standard"

⚠️ ATTENZIONE: Peso 142.0 kg >120 kg.
   Evidenza limitata per DOAC in pazienti con peso molto elevato.
   Considerare monitoraggio più stretto.

ℹ️ Possibile considerare Warfarin come alternativa più monitorabile
```

---

## TEST SUITE 8: SCENARI REALI COMPLETI

### Scenario 1: Paziente con scarso controllo INR (TTR <70%)

**Background:**
```
Paziente maschio, 68 anni
TTR ultimi 6 mesi: 52% (scarso controllo)
Ragione switch: migliorare stabilità anticoagulazione
INR oscillante tra 1.7 e 3.8
Ultima settimana: INR 2.2, 2.9, 3.2, 2.4
INR oggi: 2.6
```

**Input:**
```
DOAC scelto: Apixaban (più stabile, BID)
Età: 68 anni
Peso: 78 kg
ClCr: 68 mL/min
Creatinina: 1.15 mg/dL
INR attuale: 2.6
```

**Output Atteso:**
```
✅ Dosaggio: "5 mg BID"
✅ Switch raccomandato per migliorare stabilità

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (obiettivo ≤2.0)
📅 Giorno 3-4: Iniziare Apixaban quando INR ≤2.0

Note:
📌 Vantaggio: nessun monitoraggio INR necessario dopo switch
📌 Educare su importanza aderenza (dose fissa BID)
📌 Follow-up a 1 mese per verifica tollerabilità
```

### Scenario 2: Paziente anziano fragile

**Background:**
```
Donna 87 anni, vive sola
Difficoltà accesso centro prelievi (abita in area rurale)
INR instabile per scarsa compliance alimentare
Rischio cadute presente
```

**Input:**
```
DOAC scelto: Edoxaban (monosomministrazione, dose fissa)
Età: 87 anni
Peso: 57 kg
Sesso: F
Creatinina: 1.4 mg/dL
ClCr calcolata: 32 mL/min
INR attuale: 2.8
```

**Output Atteso:**
```
✅ Dosaggio: "30 mg una volta al giorno"
✅ Rationale: "Dose ridotta per: peso ≤60 kg, ClCr 32.0 mL/min (30-50 mL/min)"

⚠️ WARNING: Età 87 anni. Paziente molto anziano
⚠️ WARNING: ClCr 32.0 mL/min. Insufficienza renale moderata

Timeline:
📅 Giorno 0: Sospendere Warfarin
📅 Giorno 2: Controllare INR (≤2.5)
📅 Giorno 3-4: Iniziare Edoxaban 30mg die quando INR ≤2.5

Raccomandazioni aggiuntive:
📌 Monosomministrazione facilita compliance
📌 Nessun vincolo alimentare (vs Warfarin)
📌 Valutare rischio cadute vs beneficio
📌 Controllo funzione renale ogni 3 mesi (ClCr borderline)
📌 Supporto familiare/badante per aderenza
```

---

## CHECKLIST TEST WARFARIN → DOAC

### ✅ Soglie INR

- [ ] Apixaban: INR ≤ 2.0
- [ ] Rivaroxaban: INR ≤ 3.0
- [ ] Dabigatran: INR ≤ 2.0
- [ ] Edoxaban: INR ≤ 2.5

### ✅ Calcolo Dosaggi

- [ ] Apixaban criteri ABC (2 su 3)
- [ ] Rivaroxaban ridotto se ClCr <50
- [ ] Dabigatran ridotto se età ≥80 o ClCr 30-50
- [ ] Edoxaban ridotto se peso ≤60 o ClCr 30-50

### ✅ Timeline

- [ ] Warfarin: attesa 3-4 giorni (controllo giorno 2)
- [ ] Acenocumarolo: attesa 2-3 giorni (controllo giorno 1)
- [ ] No bridging necessario
- [ ] Ripetere INR se ancora sopra soglia

### ✅ Controindicazioni

- [ ] Valvole meccaniche → blocca switch
- [ ] Stenosi mitralica → blocca switch
- [ ] Gravidanza → blocca switch
- [ ] ClCr <15 (tutti DOAC) → blocca switch
- [ ] ClCr <30 (Dabigatran) → blocca switch
- [ ] ClCr >95 (Edoxaban in FA) → sconsigliato

### ✅ Warnings

- [ ] Peso >120 kg
- [ ] Peso <50 kg
- [ ] Età ≥85 anni
- [ ] ClCr 15-30 (con DOAC appropriato)
- [ ] ClCr 30-50 (monitoraggio renale)

### ✅ Note Cliniche

- [ ] Principio "Stop and Wait"
- [ ] Emivita Warfarin vs Acenocumarolo
- [ ] No monitoraggio INR post-switch
- [ ] Importanza aderenza
- [ ] Follow-up 1 mese

---

## 📊 REPORT DI TEST

```
╔══════════════════════════════════════════════════╗
║       TEST SWITCH WARFARIN → DOAC                ║
╠══════════════════════════════════════════════════╣
║ Test Suite 1 (Apixaban):          [ ] PASSED    ║
║ Test Suite 2 (Rivaroxaban):       [ ] PASSED    ║
║ Test Suite 3 (Dabigatran):        [ ] PASSED    ║
║ Test Suite 4 (Edoxaban):          [ ] PASSED    ║
║ Test Suite 5 (Acenocumarolo):     [ ] PASSED    ║
║ Test Suite 6 (Controindicazioni): [ ] PASSED    ║
║ Test Suite 7 (Combinazioni):      [ ] PASSED    ║
║ Test Suite 8 (Scenari Reali):     [ ] PASSED    ║
╠══════════════════════════════════════════════════╣
║ TOTALE:                           [ ] / 8        ║
╚══════════════════════════════════════════════════╝
```

**Data Test:** _________________
**Tester:** _________________
**Build Version:** _________________

---

**Buon Testing! 🚀**

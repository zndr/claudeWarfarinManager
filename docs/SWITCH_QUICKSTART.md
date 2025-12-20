# 🚀 QUICKSTART GUIDE - TAB SWITCH TERAPIA

**TaoGest - Switch Warfarin ↔ DOAC**

---

## ⚡ INIZIO RAPIDO (5 MINUTI)

### **1. Aprire TaoGest**
```
Lanciare: WarfarinManager.UI.exe
```

### **2. Accedere al Tab Switch**
```
Menu → Strumenti → 🔄 Switch Terapia (Warfarin ↔ DOAC)
```

### **3. Esempio Pratico - Warfarin → Apixaban**

**Scenario:** Paziente uomo 70 anni, in TAO da 2 anni, TTR scarso (<65%)

**Compilare Form:**
```
✅ Direzione: Warfarin → DOAC (radio button)
✅ DOAC: Apixaban
✅ Warfarin: Warfarin (Coumadin)

Parametri Paziente:
✅ Età: 70
✅ Peso: 75
✅ Sesso: M
✅ INR attuale: 2.6

Clearance Creatinina:
✅ Modalità: "Calcola (Cockcroft-Gault)"
✅ Creatinina sierica: 1.2 mg/dL
✅ Click su "Calcola ClCr"
   → Risultato: 64.8 mL/min
```

**Click "Genera Protocollo"**

**Risultato:**
```
💊 DOSAGGIO: 5 mg BID (dose standard)

📅 TIMELINE:
Giorno 0: Sospendere Warfarin
Giorno 2: Controllare INR (obiettivo ≤2.0)
Giorno 3: Iniziare Apixaban 5mg BID se INR ≤2.0

🔬 MONITORAGGIO:
• Controllo emocromo e funzione renale a 1 mese
• NON necessario monitoraggio INR dopo switch
```

**Click "💾 Salva"** → Salvato nel database!

---

## 📖 SCENARI COMUNI

### **Scenario 1: Paziente Anziano → DOAC**

**Caso:** Donna 83 anni, peso 58kg, difficoltà accesso centro prelievi

```
Input:
• Direzione: Warfarin → DOAC
• DOAC: Edoxaban (monosomministrazione)
• Età: 83, Peso: 58, Sesso: F
• Creatinina: 1.4 mg/dL → ClCr: 32 mL/min
• INR: 2.8

Output:
• Dosaggio: 30 mg una volta al giorno
  (ridotto per peso ≤60kg E ClCr 30-50)
• ⚠️ WARNING: Età 83 anni, monitoraggio stretto
• Timeline: Giorno 3-4 iniziare quando INR ≤2.5
```

### **Scenario 2: DOAC → Warfarin per Chirurgia**

**Caso:** Paziente in Dabigatran deve passare a Warfarin per intervento programmato

```
Input:
• Direzione: DOAC → Warfarin
• DOAC: Dabigatran
• Età: 68, Peso: 75, Sesso: M
• ClCr: 65 mL/min

Output:
• Metodo: OVERLAP GRADUATO (3 giorni)
• Giorno 0: Iniziare Warfarin + mantenere Dabigatran
• Giorno 1-2: Continuare entrambi
• Giorno 3: Sospendere Dabigatran
• Monitorare INR quotidianamente
```

### **Scenario 3: Controindicazione Rilevata**

**Caso:** Paziente con valvole meccaniche chiede switch a DOAC

```
Input:
• DOAC: Rivaroxaban
• ☑ Valvole meccaniche (flaggato)

Output:
• ❌ CONTROINDICAZIONE ASSOLUTA
• "Presenza di valvole meccaniche. I DOAC sono controindicati."
• SWITCH NON RACCOMANDATO
```

---

## 🎯 TIPS & TRICKS

### **💡 Tip 1: Calcolo ClCr Automatico**
- ✅ **Usa sempre il calcolatore** Cockcroft-Gault integrato
- Richiede: età, peso, sesso, creatinina sierica
- Formula validata: `[(140 - età) × peso × (0.85 se F)] / (72 × Cr)`

### **💡 Tip 2: Criteri ABC per Apixaban**
Dose ridotta 2.5mg se **≥2 criteri**:
- Età ≥80 anni
- Peso ≤60 kg
- Creatinina ≥1.5 mg/dL

```
Esempio:
• Età: 82 ✓
• Peso: 59 ✓
• Cr: 1.3 ✗
→ 2/3 criteri → DOSE RIDOTTA 2.5mg BID
```

### **💡 Tip 3: Soglie INR per DOAC**
Memorizza le soglie:
```
Rivaroxaban: ≤3.0 (più permissiva)
Edoxaban:    ≤2.5
Apixaban:    ≤2.0 (più conservativa)
Dabigatran:  ≤2.0
```

### **💡 Tip 4: Rivaroxaban CON CIBO**
⚠️ **IMPORTANTE:** Rivaroxaban 15-20mg **deve essere assunto CON IL CIBO** per assorbimento ottimale!

### **💡 Tip 5: Follow-up Automatico**
- Sistema imposta automaticamente follow-up a **+30 giorni**
- Controllare emocromo e funzione renale
- Se ClCr <50: controllo renale ogni **6 mesi**

---

## ⚠️ ERRORI COMUNI DA EVITARE

### **❌ Errore 1: Switch con INR ancora alto**
```
SBAGLIATO:
• INR 3.2 → Inizio Apixaban subito
  (Soglia Apixaban è ≤2.0!)

CORRETTO:
• INR 3.2 → Attendere 2-3 giorni
• Ricontrollare INR
• Iniziare solo quando ≤2.0
```

### **❌ Errore 2: Dimenticare controindicazioni**
```
SBAGLIATO:
• Paziente con protesi meccanica → Switch a DOAC

CORRETTO:
• Flaggare "Valvole meccaniche"
• Sistema blocca switch
• Mantenere Warfarin
```

### **❌ Errore 3: ClCr non aggiornata**
```
SBAGLIATO:
• Usare ClCr vecchia di 1 anno

CORRETTO:
• Calcolare ClCr recente
• Ricalcolare se cambio peso/creatinina
• Aggiornare ogni 6 mesi se ClCr borderline
```

### **❌ Errore 4: Dabigatran con ClCr <30**
```
SBAGLIATO:
• ClCr 28 mL/min → Dabigatran 110mg

CORRETTO:
• Sistema rileva controindicazione
• Dabigatran VIETATO se ClCr <30
• Scegliere altro DOAC o mantenere Warfarin
```

### **❌ Errore 5: Bridging non necessario Warfarin→DOAC**
```
SBAGLIATO:
• Warfarin → DOAC con EBPM

CORRETTO:
• Warfarin → DOAC: NO bridging
• "Stop and Wait" principe
• EBPM solo per DOAC → Warfarin
```

---

## 🔍 TROUBLESHOOTING

### **Problema: Pagina HTML non si carica**
```
Soluzione:
1. Verificare WebView2 Runtime installato
2. Check file: Resources\Guides\switch-therapy.html
3. Provare F5 (refresh) nella finestra
4. Controllare logs in %LocalAppData%\WarfarinManager\Logs\
```

### **Problema: Calcolo ClCr non funziona**
```
Soluzione:
1. Verificare tutti i campi compilati:
   - Età, Peso, Sesso, Creatinina
2. Usare punto decimale (non virgola): 1.2 ✓ non 1,2 ✗
3. In alternativa: input manuale ClCr
```

### **Problema: Salvataggio non funziona**
```
Soluzione:
1. Verificare database writable
2. Check permessi directory %LocalAppData%\WarfarinManager\
3. Controllare logs per errori specifici
4. Migration applicata? (auto al primo avvio)
```

### **Problema: DevTools (F12) non si apre**
```
Info:
F12 apre Developer Tools per debug JavaScript
Se non funziona: non critico, solo per sviluppatori
```

---

## 📚 RISORSE RAPIDE

### **Documentazione Completa:**
```
D:\Claude\winTaoGest\docs\
├── SWITCH_IMPLEMENTATION_SUMMARY.md     - Overview completo
├── TEST_SWITCH_DOAC_TO_WARFARIN.md      - Test DOAC→Warfarin
├── TEST_SWITCH_WARFARIN_TO_DOAC.md      - Test Warfarin→DOAC
├── BUILD_AND_TEST_RESULTS.md            - Build report
└── SWITCH_QUICKSTART.md                 - Questa guida
```

### **Linee Guida Scientifiche:**
- ESC/EHRA 2021
- Nota AIFA 97
- SmPC farmaci (Apixaban, Rivaroxaban, Dabigatran, Edoxaban)

---

## 🎓 FORMAZIONE 5-MINUTI

### **Concetti Chiave da Ricordare:**

1. **Warfarin → DOAC = "Stop and Wait"**
   - Sospendere Warfarin
   - Attendere INR sotto soglia
   - NO bridging necessario

2. **DOAC → Warfarin = Overlap/Bridging**
   - Dabigatran: overlap variabile (ClCr-dipendente)
   - Edoxaban: metà dose + overlap
   - Apixaban/Rivaroxaban: bridging EBPM

3. **Dosaggio DOAC Personalizzato**
   - Età, peso, ClCr sempre importanti
   - Criteri ABC per Apixaban
   - Riduzione dose obbligatoria se ClCr bassa

4. **Controindicazioni = STOP**
   - Valvole meccaniche
   - Stenosi mitralica
   - Gravidanza
   - ClCr troppo bassa

5. **Follow-up = Essenziale**
   - 1 mese: emocromo + funzione renale
   - ClCr <50: controlli più frequenti
   - NO INR dopo switch a DOAC

---

## ✅ CHECKLIST RAPIDA

Prima di ogni switch:
- [ ] Verificato INR recente (Warfarin→DOAC)
- [ ] Calcolato ClCr aggiornata
- [ ] Controllato controindicazioni
- [ ] Verificato interazioni farmacologiche
- [ ] Educato paziente su aderenza
- [ ] Programmato follow-up 1 mese
- [ ] Salvato protocollo nel database

---

## 🆘 HELP

**Per supporto:**
1. Consultare piano test per scenario specifico
2. Controllare logs: `%LocalAppData%\WarfarinManager\Logs\`
3. F12 nella finestra Switch per debug JavaScript
4. Rivedere linee guida ESC/EHRA 2021

---

**Buon lavoro con TaoGest Switch! 🚀**

*Guida aggiornata al 29 Novembre 2025*

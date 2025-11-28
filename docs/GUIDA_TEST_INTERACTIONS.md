# Guida Test - Interazioni Farmacologiche Warfarin

## Percorso per Accedere alla Guida

1. Avvia l'applicazione TaoGEST
2. Menu → **Aiuto**
3. **Linee guida professionali**
4. Clicca su **💊 Interazioni Farmacologiche Warfarin**

---

## Funzionalità da Testare

### 1. **Apertura e Layout**
- ✅ La finestra si apre in modalità massimizzata (1200x800)
- ✅ Header blu con titolo "Warfarin Check"
- ✅ Disclaimer medico in evidenza (sfondo rosso)
- ✅ Grafico a ciambella (Chart.js) che mostra distribuzione rischi
- ✅ Legenda con 3 livelli: Alto Rischio (rosso), Moderato (arancione), Cibo/Erbe (verde)

### 2. **Ricerca Farmaci**
Nella barra di ricerca prova:
- Digitare **"Aspirina"** → Deve mostrare solo la card Aspirina/FANS
- Digitare **"Antibiotico"** → Deve mostrare le card degli antibiotici
- Cancellare la ricerca → Ripristina tutte le card

### 3. **Filtri per Categoria**
- **Tutti**: Mostra tutte e 13 le interazioni
- **⛔ Alto Rischio**: Mostra solo 7 card rosse (Ciprofloxacina, Eritromicina, Bactrim, Aspirina, Amiodarone, Ginkgo, Fluconazolo)
- **⚠️ Moderato**: Mostra 3 card arancioni (Ibuprofene, Paracetamolo, Alcool, Carbamazepina)
- **🥗 Cibo/Erbe**: Mostra 3 card verdi (Verdure, Mirtillo, Ginkgo se classificato)

### 4. **Card Interattive**
Ogni card deve mostrare:
- Icona grande (es. 💊, 🦴, 🫀, 🥦)
- Nome del farmaco/alimento
- Categoria (es. Antibiotici, Antinfiammatori)
- Badge colorato con livello di rischio
- Breve descrizione del meccanismo

**Test hover**: Passare il mouse → La card si alza leggermente

### 5. **Modal Dettagli**
Cliccare su una qualsiasi card (es. **Ciprofloxacina**):
- ✅ Il modal si apre con animazione
- ✅ Header rosso per alto rischio (o arancione/verde per altri)
- ✅ Mostra tutte le informazioni:
  - **Meccanismo**: "Inibisce il metabolismo del Warfarin"
  - **Effetto sull'INR**: "Aumento INR 📈 (Rischio Emorragia)"
  - **Consiglio Clinico**: Dettagli completi
- ✅ Pulsante "Chiudi Scheda" funziona
- ✅ Click sulla X in alto a destra chiude il modal
- ✅ Click fuori dal modal (area scura) lo chiude

### 6. **Navigazione Browser**
Nella toolbar WPF in alto:
- **⬅ Indietro**: Torna alla pagina precedente (se hai navigato)
- **Avanti ➡**: Va alla pagina successiva (se disponibile)
- **🔄 Aggiorna**: Ricarica la pagina
- **✖ Chiudi**: Chiude la finestra

### 7. **Test Funzionamento Offline**
1. **Con internet attivo**: Apri la guida → Verifica che funzioni
2. **Disattiva WiFi/Ethernet**
3. **Chiudi e riapri la guida**
4. **Verifica**: Tutto deve continuare a funzionare perfettamente
   - Grafico Chart.js caricato
   - Stili Tailwind applicati
   - Tutte le funzionalità JS operative

---

## Elenco Completo Interazioni (13 totali)

### Alto Rischio (7):
1. **Ciprofloxacina** 💊 - Antibiotici
2. **Eritromicina/Claritromicina** 💊 - Antibiotici
3. **Bactrim (Sulfametoxazolo)** 💊 - Antibiotici
4. **Aspirina/FANS** 🦴 - Antinfiammatori
5. **Amiodarone** 🫀 - Cardiovascolari
6. **Ginkgo Biloba** 🌿 - Erbe
7. **Fluconazolo** 🍄 - Antifungini

### Moderato (4):
8. **Ibuprofene/Ketoprofene** 🦴 - Antinfiammatori
9. **Paracetamolo (Tachipirina)** 🤒 - Analgesici
10. **Alcool** 🍷 - Cibo
11. **Carbamazepina** 🧠 - Antiepilettici

### Cibo/Erbe (2):
12. **Verdure a Foglia Larga** 🥦 - Cibo
13. **Succo di Mirtillo** 🫐 - Cibo

---

## Grafico Chart.js

Il grafico a ciambella mostra:
- **Rosso**: 7 elementi Alto Rischio
- **Arancione**: 4 elementi Moderato
- **Verde**: 2 elementi Cibo/Erbe

Hover sul grafico → Mostra tooltip con numero elementi

---

## Problemi Comuni e Soluzioni

### La finestra non si apre
- Verifica che WebView2 Runtime sia installato (viene installato automaticamente con il pacchetto NuGet)
- Controlla i log dell'applicazione per errori

### Il grafico non si visualizza
- Verifica che `chart.min.js` sia presente in `Resources/Guides/lib/chartjs/`
- Controlla la console del browser (F12 dev tools non disponibile, ma errori visibili nel log)

### Gli stili non si applicano
- Verifica che `tailwind-custom.css` sia presente in `Resources/Guides/css/`
- Controlla il percorso relativo nel file HTML

### La ricerca non funziona
- Verifica che `interactions.js` sia caricato correttamente
- Controlla la console JavaScript per errori

---

## Note per il Futuro

Questo file HTML servirà da **template** per le altre guide:
- Guida TAO completa
- Guida controllo INR
- Guida Bridge Therapy
- Guida eventi avversi

**Struttura da replicare**:
1. Header con logo e titolo
2. Disclaimer medico (se necessario)
3. Introduzione contestuale
4. Grafico interattivo (Chart.js)
5. Barra ricerca + filtri
6. Grid di card informative
7. Modal dettagli con informazioni complete

---

## Feedback e Miglioramenti

Dopo il test, valutare:
- [ ] Colori e contrasti adeguati?
- [ ] Font leggibili a diverse risoluzioni?
- [ ] Animazioni fluide?
- [ ] Informazioni complete e chiare?
- [ ] Navigazione intuitiva?
- [ ] Performance soddisfacenti?

---

**Buon Test! 🎯**

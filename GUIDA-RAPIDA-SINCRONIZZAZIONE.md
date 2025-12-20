# 🔄 Guida Rapida: Come Aggiornare le Guide in TaoGEST

## ✅ Metodo Più Semplice (Raccomandato)

### Opzione A: Sincronizzazione Automatica (Durante il Build)

1. **Modifica i file** nella cartella `docs`:
   ```
   D:\Claude\winTaoGest\docs\interactions.html
   D:\Claude\winTaoGest\docs\LineeGuida.html
   D:\Claude\winTaoGest\docs\LineeGuida.pdf
   ...ecc
   ```

2. **Salva le modifiche** (Ctrl+S)

3. **Esegui Build in Visual Studio**:
   - Premi `F6` (Build)
   - Oppure: **Build → Build Solution**
   - Oppure: Premi `F5` (Run)

4. **Fatto!** ✨
   - Le guide vengono sincronizzate automaticamente
   - L'applicazione viene compilata con le nuove modifiche
   - Puoi subito testare le modifiche nell'app

### Opzione B: Sincronizzazione Manuale (Più Veloce per Test)

Se vuoi vedere le modifiche **immediatamente** senza ricompilare tutto:

1. **Modifica i file** in `docs` e salva

2. **Doppio click** su:
   ```
   D:\Claude\winTaoGest\scripts\sync-guides.bat
   ```

3. **Premi F5** in Visual Studio (Run)
   - Non serve fare Rebuild
   - L'app si riavvia con le modifiche

## 📁 Dove Sono i File?

### File che DEVI modificare (la tua area di lavoro):
```
D:\Claude\winTaoGest\docs\
├── interactions.html              ← Modifica qui
├── LineeGuida.html                ← Modifica qui
├── LineeGuida.pdf                 ← Modifica qui
├── Guida Warfarin per pazienti.pdf ← Modifica qui
├── Algoritmo Gestione INR.html    ← Modifica qui
└── infografica-tao.html           ← Modifica qui
```

### File che vengono AGGIORNATI automaticamente (NON modificare):
```
D:\Claude\winTaoGest\src\WarfarinManager.UI\Resources\Guides\
├── interactions.html              ← Copia automatica
├── linee-guida-tao.html          ← Copia automatica
├── LineeGuida.pdf                 ← Copia automatica
├── Guida Warfarin per pazienti.pdf ← Copia automatica
├── algoritmo-gestione-inr.html   ← Copia automatica
└── infografica-tao.html           ← Copia automatica
```

## 🎯 Workflow Consigliato

### Durante lo Sviluppo Attivo (Modifiche Frequenti):

1. Apri il file HTML in `docs` con il tuo editor preferito
2. Fai le modifiche
3. Salva (Ctrl+S)
4. Esegui `sync-guides.bat` (doppio click)
5. Premi F5 in Visual Studio
6. Testa le modifiche
7. Ripeti 2-6 fino a quando sei soddisfatto

### Per Modifiche Occasionali:

1. Modifica i file in `docs`
2. Salva
3. Build in Visual Studio (F6 o F5)
4. Testa

## ⚡ Script Disponibili

### `sync-guides.bat` (Più Semplice)
- Doppio click per eseguire
- Non richiede configurazione
- Copia tutti i file

### `sync-guides.ps1` (Più Dettagliato)
- Output colorato
- Mostra quali file sono stati modificati
- Copia solo i file necessari
- Esecuzione:
  ```powershell
  cd D:\Claude\winTaoGest\scripts
  powershell -ExecutionPolicy Bypass -File sync-guides.ps1
  ```

## ❓ FAQ

### Q: Ho modificato un file ma non vedo le modifiche nell'app
**A:** Prova questi passaggi:
1. Chiudi l'applicazione
2. Build → Clean Solution
3. Esegui `sync-guides.bat`
4. Build → Rebuild Solution
5. Premi F5

### Q: Lo script dice "File già aggiornato" ma ho fatto modifiche
**A:** Probabilmente hai modificato il file nella cartella `Resources\Guides` invece che in `docs`. Modifica sempre i file in `docs`!

### Q: Posso disabilitare la sincronizzazione automatica?
**A:** Sì, vedi il file `scripts\README.md` per le istruzioni.

### Q: Posso aggiungere nuovi file alle guide?
**A:** Sì! Aggiungi il nome del file all'array `$filesToSync` nello script `sync-guides.ps1` alla riga 25.

## 🎨 Pro Tips

1. **Usa un editor HTML con live preview** (es. VS Code con Live Server) per vedere le modifiche HTML in tempo reale

2. **Tieni aperto lo script batch** in una finestra separata per esecuzioni rapide

3. **Crea una scorciatoia** di `sync-guides.bat` sul desktop per accesso ancora più veloce

4. **Usa Git** per tenere traccia delle modifiche alle guide (i file in `docs` sono già tracciati)

## 📝 Checklist Prima di un Commit

Prima di fare commit delle modifiche:

- [ ] Ho modificato i file in `docs` (non in `Resources\Guides`)
- [ ] Ho eseguito la sincronizzazione (manuale o automatica)
- [ ] Ho testato le modifiche nell'applicazione
- [ ] L'app compila senza errori
- [ ] Le guide si aprono correttamente

## 🆘 In Caso di Problemi

1. Verifica che i file esistano in `D:\Claude\winTaoGest\docs`
2. Controlla i permessi dei file (lettura/scrittura)
3. Assicurati che Visual Studio non abbia i file aperti/bloccati
4. Chiudi l'applicazione prima di fare il rebuild
5. Consulta `scripts\README.md` per troubleshooting dettagliato

---

**Ricorda:** Modifica sempre i file in `docs`, mai in `Resources\Guides`! 🎯

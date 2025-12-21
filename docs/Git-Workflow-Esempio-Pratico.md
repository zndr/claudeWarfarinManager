# 📝 Git Workflow - Esempio Pratico Completo

> **Documento creato il:** 21 Dicembre 2024
> **Scopo:** Esempio passo-passo di un commit completo dalla A alla Z
> **Caso d'uso:** Aggiunta della guida Git/GitHub al repository

---

## 🎯 **OBIETTIVO DELL'OPERAZIONE**

Aggiungere la guida Git/GitHub al repository e salvarla con un commit

---

## 📚 Il Workflow in 6 Passi

```
1. git status          → Vedere cosa c'è
2. git add file.md     → Preparare il file
3. git status          → Verificare staging
4. git commit -m "..." → Salvare il commit
5. git push origin     → Inviare a GitHub
6. git status          → Verifica finale
```

---

## 🔧 **PASSO 1: Verificare lo stato**

### Comando:
```bash
git status
```

### 📖 SPIEGAZIONE:

**Cosa fa:**
- Vediamo dove siamo e cosa c'è da committare
- Verifichiamo che la guida sia tra i file "Untracked"

**Perché è importante:**
- ✅ Capire su quale branch siamo
- ✅ Vedere quali file sono stati modificati
- ✅ Identificare file nuovi non ancora tracciati

---

### Output ottenuto:

```
On branch master
Your branch is up to date with 'origin/master'.

Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
	modified:   .claude/settings.local.json

Untracked files:
  (use "git add <file>..." to include in what will be committed)
	ApplyMigration.ps1
	CheckMigrations.ps1
	add_wizard_column.sql
	docs/Git-GitHub-Guida.md          ← QUESTO CI INTERESSA!
	docs/nomogramma di pengo.csv
	docs/nomogramma di pengo.htm
	docs/nomogramma di pengo.mht
	docs/nomogramma di pengo.pdf
	docs/nomogramma di pengo_file/
	installer/TaoGEST-Setup.~is

no changes added to commit (use "git add" and/or "git commit -a")
```

---

### ✅ RISULTATO PASSO 1:

- ✅ Siamo su branch `master`
- ✅ La guida `docs/Git-GitHub-Guida.md` è tra i file "Untracked"
- ✅ Pronta per essere aggiunta!

---

## 🔧 **PASSO 2: Aggiungere la guida alla Staging Area**

### Comando:
```bash
git add docs/Git-GitHub-Guida.md
```

### 📖 SPIEGAZIONE:

**Sintassi del comando:**
- `git add` = prepara il file per il commit
- `docs/Git-GitHub-Guida.md` = percorso specifico del file da aggiungere

**Perché usare il percorso specifico e non `git add .`:**
- ✅ Controllo preciso su cosa committare
- ✅ Evito di aggiungere file temporanei per errore
- ✅ Commit focalizzato su una singola cosa

---

### 🧠 CONCETTO: STAGING AREA

La Staging Area (o Index) è un'area intermedia tra le tue modifiche e il repository.

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│ Working         │       │ Staging Area    │       │ Repository      │
│ Directory       │       │ (Index)         │       │ (Commits)       │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ File modificati │ add → │ File pronti per │ commit│ Snapshot        │
│ o nuovi         │       │ il commit       │   →   │ permanenti      │
└─────────────────┘       └─────────────────┘       └─────────────────┘
```

**Flusso nel nostro caso:**
```
Working Directory          Staging Area             Repository
     ↓                          ↓                        ↓
Git-GitHub-Guida.md  →→→  Git-GitHub-Guida.md  →→→  (dopo commit)
  (file creato)           (ADESSO QUI!)           (sarà qui dopo)
```

---

### ⚠️ ATTENZIONE:

**Cosa NON abbiamo fatto:**
- ❌ `git add .` (avrebbe aggiunto TUTTO, anche file temporanei)
- ❌ `git add docs/` (avrebbe aggiunto TUTTA la cartella docs)

**Cosa abbiamo fatto:**
- ✅ Aggiunto SOLO il file che ci interessa
- ✅ File temporanei rimangono "Untracked" (corretto!)

---

### ✅ RISULTATO PASSO 2:

- ✅ File aggiunto alla staging area
- ✅ Pronto per essere committato
- ✅ Altri file rimasti fuori (come desiderato)

---

## 🔧 **PASSO 3: Verificare che sia stato aggiunto correttamente**

### Comando:
```bash
git status
```

### 📖 SPIEGAZIONE:

**Perché rifare `git status`:**
- ✅ Controllo di sicurezza
- ✅ Vedere che il file sia in "Changes to be committed"
- ✅ Verificare che non abbiamo aggiunto altro per errore

**Best Practice:**
- 🎯 Sempre verificare PRIMA di committare
- 🎯 `git status` è il comando più importante (usalo SPESSO!)

---

### Output ottenuto:

```
On branch master
Your branch is up to date with 'origin/master'.

Changes to be committed:
  (use "git restore --staged <file>..." to unstage)
	new file:   docs/Git-GitHub-Guida.md    ← PERFETTO! In staging!

Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
	modified:   .claude/settings.local.json

Untracked files:
  (use "git add <file>..." to include in what will be committed)
	ApplyMigration.ps1
	CheckMigrations.ps1
	add_wizard_column.sql
	docs/nomogramma di pengo.csv
	docs/nomogramma di pengo.htm
	docs/nomogramma di pengo.mht
	docs/nomogramma di pengo.pdf
	docs/nomogramma di pengo_file/
	installer/TaoGEST-Setup.~is
```

---

### ✅ RISULTATO PASSO 3:

```
Changes to be committed:
	new file:   docs/Git-GitHub-Guida.md  ← ✅ PERFETTO!
```

**Analisi output:**
- ✅ Il file è in "Changes to be committed" (staging area)
- ✅ Marcato come "new file" (Git sa che è nuovo)
- ✅ Altri file ancora "Untracked" (corretto)
- ✅ Tutto pronto per il commit!

**Se volessi rimuoverlo dalla staging:**
```bash
git restore --staged docs/Git-GitHub-Guida.md
```
(Ma NON lo facciamo, vogliamo committare!)

---

## 🔧 **PASSO 4: Creare il commit**

### Comando:
```bash
git commit -m "$(cat <<'EOF'
Docs: Aggiunta guida completa Git e GitHub

Creato documento di riferimento per comprendere Git e GitHub
con spiegazioni dettagliate, esempi pratici e troubleshooting.

Contenuto:
- Concetti fondamentali (repository locale/remoto/web)
- Comandi principali con spiegazioni passo-passo
- Workflow completo per Pull Request
- Sezione speciale su "Your branch is up to date" (ingannevole)
- Strategie di merge (merge commit, squash, rebase)
- Schema riassuntivo comandi quick reference
- Troubleshooting problemi comuni
- Best practice e consigli

Il documento include diagrammi ASCII, tabelle formattate e
esempi concreti basati su sessioni di lavoro reali.

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
EOF
)"
```

---

### 📖 SPIEGAZIONE:

**Sintassi del comando:**
- `git commit` = crea uno snapshot permanente
- `-m "..."` = messaggio del commit (message)
- `"$(cat <<'EOF' ... EOF)"` = heredoc per messaggio multi-riga

**Perché usare heredoc:**
- ✅ Messaggi multi-riga formattati bene
- ✅ Evita problemi con apici/virgolette
- ✅ Più leggibile

**Alternativa semplice (messaggio breve):**
```bash
git commit -m "Docs: Aggiunta guida Git e GitHub"
```

---

### 🧠 CONCETTO: COMMIT

**Cos'è un commit:**

Un commit è uno **"snapshot" permanente** del progetto in un momento specifico.

**Cosa contiene:**
```
Commit 674b041
├─ File modificati (nel nostro caso: Git-GitHub-Guida.md)
├─ Contenuto completo dei file
├─ Messaggio descrittivo
├─ Data e ora (es: 2024-12-21 14:30:00)
├─ Autore (nome ed email)
├─ Hash univoco (674b041...)
└─ Puntatore al commit precedente (parent)
```

**Visualizzazione:**
```
A (init) ← B (feature) ← C (fix) ← D (docs) ← HEAD (tu sei qui)
                                    ↑
                               674b041
                          "Aggiunta guida"
```

---

### 🎯 BEST PRACTICE per messaggi commit:

**Formato consigliato:**
```
Tipo: Breve descrizione (max 50 caratteri)

Corpo del messaggio più dettagliato che spiega:
- Perché questa modifica è necessaria
- Cosa risolve o aggiunge
- Eventuali considerazioni tecniche

Footer opzionale (issue, co-autori, ecc.)
```

**Tipi comuni:**
- `Feat:` - Nuova funzionalità
- `Fix:` - Correzione bug
- `Docs:` - Documentazione
- `Refactor:` - Refactoring codice
- `Test:` - Aggiunta/modifica test
- `Chore:` - Manutenzione, build, ecc.

**Esempi:**
```
✅ GOOD: "Fix: Corretto calcolo dosaggio in fase induzione"
✅ GOOD: "Feat: Implementato wizard configurazione paziente"
✅ GOOD: "Docs: Aggiunta guida completa Git e GitHub"

❌ BAD:  "fix"
❌ BAD:  "aggiornamenti"
❌ BAD:  "modifiche varie"
```

---

### Output ottenuto:

```
[master 674b041] Docs: Aggiunta guida completa Git e GitHub
 1 file changed, 799 insertions(+)
 create mode 100644 docs/Git-GitHub-Guida.md
```

---

### ✅ RISULTATO PASSO 4:

**Analisi output:**

```
[master 674b041] ← Branch e hash del commit
 1 file changed,  ← Numero di file modificati
 799 insertions(+) ← Righe aggiunte (nessuna rimossa)
 create mode 100644 ← Nuovo file creato con permessi standard
```

**Cosa è successo:**
- ✅ Commit creato con hash `674b041`
- ✅ Salvato sul branch `master`
- ✅ 1 file aggiunto al repository
- ✅ 799 righe di documentazione salvate
- ✅ Commit salvato LOCALMENTE (non ancora su GitHub)

**Stato attuale:**
```
Repository Locale:   A---B---C---D (nuovo commit 674b041)
Repository Remoto:   A---B---C     (ancora vecchio)
                                   ↑
                          Serve git push per sincronizzare!
```

---

## 🔧 **PASSO 5: Inviare il commit a GitHub**

### Comando:
```bash
git push origin master
```

### 📖 SPIEGAZIONE:

**Sintassi del comando:**
- `git push` = invia commit locali al repository remoto
- `origin` = nome del repository remoto (GitHub)
- `master` = nome del branch da pushare

**Cosa significa "origin":**
- Nome convenzionale per il repository remoto principale
- Configurato automaticamente quando fai `git clone`
- Puoi vederlo con: `git remote -v`

**Perché specificare "master":**
- ✅ Esplicito e chiaro
- ✅ Evita push di branch sbagliati
- ✅ Best practice per sicurezza

---

### 🧠 CONCETTO: PUSH

**Cosa fa `git push`:**

Sincronizza i commit dal repository **locale** al repository **remoto** (GitHub).

```
┌────────────────────────────────────────────────┐
│ PRIMA del push:                                │
│                                                │
│ Locale:   A---B---C---D (nuovo commit guida)  │
│ Remoto:   A---B---C     (vecchio)             │
│                                                │
│ Situazione: NON sincronizzati ❌              │
└────────────────────────────────────────────────┘

                    ↓ git push ↓

┌────────────────────────────────────────────────┐
│ DOPO il push:                                  │
│                                                │
│ Locale:   A---B---C---D                       │
│ Remoto:   A---B---C---D                       │
│                                                │
│ Situazione: Sincronizzati! ✅                 │
└────────────────────────────────────────────────┘
```

---

### ⚠️ ATTENZIONE:

**Cosa succede durante il push:**

1. Git contatta GitHub
2. Confronta i commit locali con quelli remoti
3. Carica SOLO i commit nuovi (D nel nostro caso)
4. Aggiorna il puntatore `master` su GitHub
5. Conferma che tutto è andato bene

**Situazioni possibili:**

**✅ Push riuscito (normale):**
```
To https://github.com/user/repo.git
   1c123b6..674b041  master -> master
```

**❌ Push rifiutato (remoto più avanti):**
```
! [rejected]        master -> master (fetch first)
error: failed to push some refs
```
**Soluzione:** Fare `git pull` prima di `git push`

**⚠️ Push con conflitti:**
```
CONFLICT (content): Merge conflict in file.cs
```
**Soluzione:** Risolvere conflitti manualmente, poi riprovare

---

### Output ottenuto:

```
To https://github.com/zndr/claudeWarfarinManager.git
   1c123b6..674b041  master -> master
```

---

### ✅ RISULTATO PASSO 5:

**Analisi output:**

```
To https://github.com/zndr/claudeWarfarinManager.git
   ↑
   Repository remoto di destinazione

   1c123b6..674b041  master -> master
   ↑        ↑        ↑         ↑
   vecchio  nuovo    locale    remoto
```

**Traduzione:**
- Il branch `master` su GitHub è stato aggiornato
- Da commit `1c123b6` (vecchio) a `674b041` (nuovo con la guida)
- Push completato con successo! ✅

**Cosa è successo:**
- ✅ Il commit è ora visibile su GitHub
- ✅ Altri collaboratori possono vederlo
- ✅ Repository locale e remoto sincronizzati
- ✅ La guida è accessibile via web

**Dove vederla su GitHub:**
```
🌐 https://github.com/zndr/claudeWarfarinManager/blob/master/docs/Git-GitHub-Guida.md
```

---

## 🔧 **PASSO 6: Verifica finale**

### Comando:
```bash
git status
```

### 📖 SPIEGAZIONE:

**Perché fare una verifica finale:**
- ✅ Confermare che tutto sia sincronizzato
- ✅ Vedere se ci sono altri file da committare
- ✅ Assicurarsi che il workspace sia pulito

**Cosa ci aspettiamo di vedere:**
- ✅ "Your branch is up to date with 'origin/master'"
- ✅ Nessun file in "Changes to be committed"
- ✅ Working directory pulito (tranne file intenzionalmente non tracciati)

---

### Output ottenuto:

```
On branch master
Your branch is up to date with 'origin/master'.

Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
	modified:   .claude/settings.local.json

Untracked files:
  (use "git add <file>..." to include in what will be committed)
	ApplyMigration.ps1
	CheckMigrations.ps1
	add_wizard_column.sql
	docs/nomogramma di pengo.csv
	docs/nomogramma di pengo.htm
	docs/nomogramma di pengo.mht
	docs/nomogramma di pengo.pdf
	docs/nomogramma di pengo_file/
	installer/TaoGEST-Setup.~is

no changes added to commit (use "git add" and/or "git commit -a")
```

---

### ✅ RISULTATO PASSO 6:

**Analisi output:**

```
✅ "Your branch is up to date with 'origin/master'"
   ↑ Ora è VERO perché abbiamo appena fatto push!

✅ "docs/Git-GitHub-Guida.md" NON appare più
   ↑ È stato committato e pushato!

✅ File temporanei ancora "Untracked"
   ↑ Corretto, li abbiamo volutamente esclusi
```

**Stato finale:**
- ✅ La guida è stata committata
- ✅ Il commit è stato pushato su GitHub
- ✅ Master locale e remoto sono sincronizzati
- ✅ File temporanei rimasti fuori (come desiderato)
- ✅ Workspace pronto per nuovo lavoro

---

## 🎉 **OPERAZIONE COMPLETATA CON SUCCESSO!**

### 📋 Riepilogo completo dell'operazione:

| Passo | Comando | Scopo | Risultato |
|-------|---------|-------|-----------|
| 1 | `git status` | Vedere situazione iniziale | File identificato ✅ |
| 2 | `git add docs/...` | Preparare file per commit | File in staging ✅ |
| 3 | `git status` | Verificare staging | Staging confermato ✅ |
| 4 | `git commit -m "..."` | Creare snapshot permanente | Commit creato ✅ |
| 5 | `git push origin master` | Inviare a GitHub | Push riuscito ✅ |
| 6 | `git status` | Verifica finale | Sincronizzato ✅ |

---

### 🌐 Dove trovare la guida:

**📄 Locale (sul tuo computer):**
```
d:\Claude\TaoGest\docs\Git-GitHub-Guida.md
```

**☁️ Su GitHub (web):**
```
https://github.com/zndr/claudeWarfarinManager/blob/master/docs/Git-GitHub-Guida.md
```

**Benefici:**
- ✅ Backup sicuro su cloud
- ✅ Visibile a tutti i collaboratori
- ✅ Renderizzata con formattazione bella
- ✅ Storico delle modifiche tracciato

---

## 🎓 LEZIONI APPRESE

### 1️⃣ Il workflow base di Git:

```
Status → Add → Status → Commit → Push → Status
  ↓      ↓      ↓        ↓        ↓       ↓
Vedere Preparare Verificare Salvare Inviare Confermare
```

**Questo è il workflow che userai ogni giorno!**

---

### 2️⃣ Importanza di `git status`:

Abbiamo usato `git status` **3 volte** in 6 passi!

**Quando usarlo:**
- ✅ SEMPRE prima di qualsiasi operazione
- ✅ Dopo ogni modifica importante
- ✅ Quando sei confuso su cosa sta succedendo
- ✅ Prima di commit/push

**Regola d'oro:**
> "Quando sei in dubbio, fai `git status`"

---

### 3️⃣ Commit focalizzati:

**Cosa abbiamo fatto:**
- ✅ Committato SOLO il file della guida
- ✅ Lasciato fuori file temporanei
- ✅ Messaggio chiaro e descrittivo

**Perché è importante:**
- ✅ Storia più pulita e leggibile
- ✅ Facile fare revert se serve
- ✅ Review più semplice per altri
- ✅ Commit ha un "tema" chiaro

---

### 4️⃣ Sincronizzazione locale/remoto:

**Concetto chiave:**
- Repository locale e remoto sono **SEPARATI**
- Devi **sincronizzarli esplicitamente** con push/pull
- Dopo commit SEI ANCORA SOLO LOCALE
- Solo dopo push sei su GitHub

```
Commit → Solo locale 💻
Push   → Anche remoto ☁️
```

---

## 📚 Comandi usati in questo workflow

### Comandi principali:

```bash
# 1. Vedere stato
git status

# 2. Aggiungere file specifico
git add percorso/file.md

# 3. Committare con messaggio
git commit -m "Tipo: Descrizione breve"

# 4. Pushare a GitHub
git push origin master
```

---

### Varianti utili:

```bash
# Aggiungere più file
git add file1.cs file2.cs file3.cs

# Aggiungere tutti i file modificati (ATTENZIONE!)
git add .

# Commit con messaggio multi-riga (heredoc)
git commit -m "$(cat <<'EOF'
Titolo

Descrizione
dettagliata
EOF
)"

# Vedere commit recenti
git log --oneline -5

# Vedere differenze prima di commit
git diff
```

---

## 🎯 Cosa fare dopo

Ora che hai completato il tuo primo commit completo:

### ✅ Prossimi passi:

1. **Pratica questo workflow** ogni volta che modifichi il codice
2. **Studia la guida Git** che abbiamo appena committato
3. **Sperimenta** con branch e merge
4. **Usa `git status`** SPESSO (non puoi sbagliare!)

### 📖 Approfondimenti consigliati:

- [ ] Branch e merge
- [ ] Conflitti e come risolverli
- [ ] Pull Request su GitHub
- [ ] Git log e history
- [ ] .gitignore per file da escludere

---

## 💡 Suggerimenti finali

### ✅ Best Practice da seguire:

1. **Commit piccoli e frequenti** - Meglio 10 commit piccoli che 1 gigante
2. **Messaggi descrittivi** - Il "te del futuro" ti ringrazierà
3. **`git status` è tuo amico** - Usalo sempre, non ti stancherai mai
4. **Pull prima di push** - Evita conflitti fastidiosi
5. **Branch per feature** - Mantieni master pulito

### ❌ Errori da evitare:

1. ❌ `git add .` senza verificare cosa stai aggiungendo
2. ❌ Commit con messaggio "fix" o "wip" o "test"
3. ❌ Push senza aver fatto commit
4. ❌ Modificare file senza fare pull prima
5. ❌ Lavorare direttamente su master (usa branch!)

---

## 🔗 Collegamenti

### File correlati:

- 📄 [Guida Git e GitHub completa](Git-GitHub-Guida.md)
- 📄 Questo documento: `Git-Workflow-Esempio-Pratico.md`

### Repository:

- 🌐 [Repository su GitHub](https://github.com/zndr/claudeWarfarinManager)
- 📊 [Pull Request #4](https://github.com/zndr/claudeWarfarinManager/pull/4)

---

## 📝 Note Personali

Usa questo spazio per annotare i tuoi appunti mentre studi questo workflow:

```
Cosa ho imparato:




Dubbi da chiarire:




Comandi da memorizzare:




```

---

**Fine del workflow esempio** ✅

> Studia questo esempio con calma, passo dopo passo.
> Ogni comando ha uno scopo preciso!

---

**Creato con ❤️ da Claude Code**
**Data:** 21 Dicembre 2024
**Versione:** 1.0

# 📚 Guida Git e GitHub - Concetti Fondamentali

> **Documento creato il:** 21 Dicembre 2024
> **Autore:** Claude Code
> **Scopo:** Guida pratica per comprendere Git e GitHub passo dopo passo

---

## 📋 Indice

1. [Concetti Base](#concetti-base)
2. [Le Tre Copie del Codice](#le-tre-copie-del-codice)
3. [Comandi Fondamentali](#comandi-fondamentali)
4. [Workflow Completo](#workflow-completo)
5. [Il Mistero di "up to date"](#il-mistero-di-up-to-date)
6. [Strategie di Merge](#strategie-di-merge)
7. [Schema Riassuntivo Comandi](#schema-riassuntivo-comandi)
8. [Troubleshooting](#troubleshooting)

---

## 🧠 Concetti Base

### Cos'è Git?

**Git** è un sistema di controllo versione **distribuito** che tiene traccia delle modifiche ai file nel tempo.

**Caratteristiche principali:**
- ✅ Ogni sviluppatore ha una **copia completa** del repository
- ✅ Lavoro **offline** possibile (commit locali)
- ✅ **Branch** per sviluppo parallelo
- ✅ **Merge** per unire modifiche

### Cos'è GitHub?

**GitHub** è una piattaforma **cloud** per ospitare repository Git.

**Funzioni principali:**
- ☁️ **Backup** remoto del codice
- 👥 **Collaborazione** tra sviluppatori
- 🔄 **Sincronizzazione** tra computer diversi
- 📊 **Pull Request** per review del codice

---

## 🗂️ Le Tre Copie del Codice

Quando lavori con Git e GitHub, hai **3 versioni** del tuo codice:

```
┌─────────────────────────────────────────────────────────┐
│ 1. 💻 REPOSITORY LOCALE (il tuo computer)              │
│    Percorso: d:\Claude\TaoGest                          │
│    ├─ Working Directory (dove modifichi i file)        │
│    ├─ Staging Area (file pronti per commit)            │
│    └─ Local Repository (commit salvati localmente)     │
│                                                          │
│    Branch:                                              │
│    ├─ master (branch principale locale)                │
│    └─ new-logic (branch di sviluppo)                   │
└─────────────────────────────────────────────────────────┘
                         ▲ ▼
                    git push / pull
                         ▲ ▼
┌─────────────────────────────────────────────────────────┐
│ 2. ☁️ REPOSITORY REMOTO (GitHub)                        │
│    URL: github.com/zndr/claudeWarfarinManager           │
│    Nome in Git: "origin"                                │
│                                                          │
│    Branch:                                              │
│    ├─ master (branch principale remoto)                │
│    └─ new-logic (branch di sviluppo remoto)            │
└─────────────────────────────────────────────────────────┘
                         ▲ ▼
                    Interfaccia Web
                         ▲ ▼
┌─────────────────────────────────────────────────────────┐
│ 3. 🌐 GITHUB WEB INTERFACE                             │
│    Qui fai:                                             │
│    ├─ Pull Request                                      │
│    ├─ Code Review                                       │
│    ├─ Merge (unione branch)                            │
│    └─ Gestione Issues                                  │
└─────────────────────────────────────────────────────────┘
```

### ⚠️ IMPORTANTE: Sincronizzazione NON Automatica!

Le 3 copie **NON si sincronizzano automaticamente**!

**Esempio:**
- Se fai un merge su GitHub (web interface)
- Il repository **remoto** viene aggiornato ✅
- Il repository **locale** rimane vecchio ❌
- Devi fare **`git pull`** per sincronizzare!

---

## 🔧 Comandi Fondamentali

### 1. `git status` - Vedere lo stato

**Obiettivo:** Capire cosa sta succedendo nel repository

```bash
git status
```

**Cosa mostra:**
- ✅ Su quale branch sei
- ✅ File modificati ma non staged
- ✅ File staged pronti per commit
- ✅ File non tracciati (nuovi)
- ✅ Quanto sei avanti/indietro rispetto al remote

**Esempio output:**
```
On branch master
Your branch is behind 'origin/master' by 2 commits.

Changes not staged for commit:
  modified:   file1.cs

Untracked files:
  newfile.cs
```

---

### 2. `git add` - Preparare file per commit

**Obiettivo:** Spostare file dalla Working Directory alla Staging Area

```bash
# Aggiungere un singolo file
git add src/file.cs

# Aggiungere più file
git add file1.cs file2.cs file3.cs

# Aggiungere tutti i file modificati (usare con cautela!)
git add .
```

**Concetto - Le 3 Zone di Git:**

```
Working Directory          Staging Area           Repository
(modifiche non salvate)    (pronti per commit)    (commit salvati)
        ↓                        ↓                      ↓
   [file1.cs]              [file1.cs]              Commit A
   [file2.cs]              [file3.cs]              Commit B
   [file3.cs]                                      Commit C

   git add file1.cs  →→→→→  [sposta qui]
                            git commit  →→→→→→→→  [salva qui]
```

---

### 3. `git commit` - Salvare modifiche

**Obiettivo:** Creare uno snapshot permanente delle modifiche

```bash
# Commit con messaggio
git commit -m "Messaggio descrittivo"

# Commit con messaggio multi-riga (heredoc)
git commit -m "$(cat <<'EOF'
Titolo del commit

Descrizione dettagliata:
- Modifica 1
- Modifica 2

🤖 Generated with Claude Code
EOF
)"
```

**Best Practice per messaggi:**
- ✅ Prima riga: titolo conciso (max 50 caratteri)
- ✅ Riga vuota
- ✅ Corpo: spiegazione dettagliata del "perché"
- ✅ Usare verbi all'imperativo: "Fix", "Add", "Update"

**Esempi:**
```
❌ BAD:  "fix"
❌ BAD:  "modifiche varie"
✅ GOOD: "Fix: Corretto comportamento nomogramma Pengo"
✅ GOOD: "Feat: Implementato wizard configurazione paziente"
```

---

### 4. `git push` - Inviare commit a GitHub

**Obiettivo:** Sincronizzare repository locale → remoto

```bash
# Push del branch corrente
git push origin nome-branch

# Esempi
git push origin master
git push origin new-logic
```

**Cosa succede:**
```
PRIMA del push:

Locale:    A---B---C---D (nuovo)
Remoto:    A---B---C

DOPO il push:

Locale:    A---B---C---D
Remoto:    A---B---C---D  ✅ sincronizzati!
```

---

### 5. `git fetch` - Chiedere novità a GitHub

**Obiettivo:** Scaricare informazioni sugli aggiornamenti (SENZA modificare file)

```bash
git fetch origin
```

**Cosa fa:**
- ✅ Contatta GitHub
- ✅ Scarica informazioni su nuovi commit
- ✅ Aggiorna i riferimenti a `origin/master`, `origin/new-logic`, ecc.
- ❌ NON modifica i tuoi file locali
- ❌ NON modifica il tuo branch corrente

**Output esempio:**
```
From https://github.com/user/repo
   effdb4b..1c123b6  master -> origin/master
```

**Traduzione:** "origin/master è passato da effdb4b a 1c123b6"

---

### 6. `git pull` - Scaricare e integrare modifiche

**Obiettivo:** Sincronizzare repository remoto → locale

```bash
git pull origin master
```

**Cosa fa:**
```
git pull = git fetch + git merge

1. FETCH: Scarica info da GitHub
2. MERGE: Integra le modifiche nel tuo branch
```

**Quando usarlo:**
- ✅ Dopo un merge fatto su GitHub
- ✅ Prima di iniziare a lavorare (per avere l'ultima versione)
- ✅ Quando `git status` dice "Your branch is behind"

---

### 7. `git checkout` - Cambiare branch

**Obiettivo:** Spostarsi tra branch diversi

```bash
# Passare a un branch esistente
git checkout master
git checkout new-logic

# Creare E passare a un nuovo branch
git checkout -b feature-nuova
```

**Cosa succede:**
- ✅ I file nella cartella cambiano per riflettere il branch
- ✅ Il prompt mostra il branch corrente
- ⚠️ Le modifiche non salvate potrebbero andare perse!

**Esempio:**
```
$ git checkout master
Switched to branch 'master'
Your branch is up to date with 'origin/master'.

$ git checkout new-logic
Switched to branch 'new-logic'
```

---

### 8. `git branch` - Gestire branch

**Obiettivo:** Vedere e gestire i branch

```bash
# Vedere tutti i branch locali
git branch

# Vedere branch locali E remoti
git branch -a

# Vedere il branch corrente
git branch --show-current

# Creare un nuovo branch (senza spostarsi)
git branch nome-nuovo-branch

# Eliminare un branch locale
git branch -d nome-branch
```

**Output esempio:**
```
$ git branch
  master
* new-logic    ← asterisco indica branch corrente
  feature-test
```

---

## 🔄 Workflow Completo

### Scenario Tipico: Sviluppare una nuova funzionalità

```bash
# 1. Partire da master aggiornato
git checkout master
git pull origin master

# 2. Creare branch per la feature
git checkout -b feature-nuova

# 3. Fare modifiche ai file
# ... modifica file1.cs, file2.cs ...

# 4. Vedere cosa è cambiato
git status

# 5. Aggiungere file modificati
git add file1.cs file2.cs

# 6. Committare
git commit -m "Feat: Implementata nuova funzionalità"

# 7. Inviare a GitHub
git push origin feature-nuova

# 8. Creare Pull Request su GitHub (web)
# ... vai su github.com e clicca "Create Pull Request" ...

# 9. Dopo approvazione, fare merge su GitHub

# 10. Tornare a master e aggiornare
git checkout master
git pull origin master

# 11. (Opzionale) Eliminare branch locale
git branch -d feature-nuova
```

---

## 🔍 Il Mistero di "up to date"

### ⚠️ Il Messaggio Ingannevole

Quando fai `git checkout master`, potresti vedere:

```
Your branch is up to date with 'origin/master'
```

### ❌ Cosa NON significa:

- ❌ "Sei sincronizzato con GitHub ADESSO"
- ❌ "Non ci sono modifiche su GitHub"
- ❌ "Il tuo master è aggiornato"

### ✅ Cosa SIGNIFICA veramente:

**"Il tuo master locale è sincronizzato con l'ULTIMA VERSIONE CHE IL TUO COMPUTER CONOSCE di origin/master"**

### 🧠 Il Problema:

Git sul tuo computer ha una **"memoria"** di com'era `origin/master` l'ultima volta che hai fatto `fetch`/`pull`/`push`.

Ma **NON SA** se nel frattempo qualcuno (o tu su GitHub) ha fatto modifiche!

### ✅ La Soluzione: Chiedere a GitHub!

```bash
# STEP 1: Chiedi "ci sono novità?"
git fetch origin

# STEP 2: Verifica lo stato REALE
git status
```

**Esempio concreto:**

```bash
# PRIMA del fetch
$ git checkout master
Switched to branch 'master'
Your branch is up to date with 'origin/master'.  ← POTREBBE ESSERE FALSO!

# Chiediamo a GitHub
$ git fetch origin
From https://github.com/user/repo
   effdb4b..1c123b6  master -> origin/master  ← NOVITÀ TROVATE!

# ORA vediamo la verità
$ git status
On branch master
Your branch is behind 'origin/master' by 2 commits.  ← ECCO LA VERITÀ!
  (use "git pull" to update your local branch)
```

### 📊 Visualizzazione:

```
SITUAZIONE REALE (su GitHub):
origin/master:  A---B---C---D---E  (2 nuovi commit)

CONOSCENZA del tuo computer PRIMA del fetch:
origin/master:  A---B---C  (vecchia info)
master locale:  A---B---C
Status: "up to date" ← SBAGLIATO!

CONOSCENZA del tuo computer DOPO il fetch:
origin/master:  A---B---C---D---E  (info aggiornata!)
master locale:  A---B---C
Status: "behind by 2 commits" ← CORRETTO!
```

### 🎯 Regola d'Oro:

**Prima di fidarti di "up to date", fai sempre:**

```bash
git fetch origin
git status
```

---

## 🔀 Strategie di Merge

Quando hai una Pull Request su GitHub, hai **3 opzioni** di merge:

### 1️⃣ Create a Merge Commit (CONSIGLIATA)

**Cosa fa:**
- Crea un commit speciale di "merge"
- **Mantiene tutti i commit separati**
- Conserva la storia completa

**Come appare:**
```
master:  A---B---C-----------M (merge commit)
                  \         /
feature:           D---E---F
```

**✅ Vantaggi:**
- Storia completa e tracciabile
- Ogni feature ha i suoi commit dettagliati
- Facile fare revert di singole feature
- Standard professionale

**❌ Svantaggi:**
- History più "affollata"

**Quando usarla:**
- ✅ Hai commit significativi e ben descritti
- ✅ Vuoi mantenere la storia dello sviluppo
- ✅ Progetti con team multipli
- ✅ Quando stai imparando Git (per capire meglio)

---

### 2️⃣ Squash and Merge

**Cosa fa:**
- **Schiaccia** tutti i commit in UNO solo
- Perde i dettagli dei singoli commit

**Come appare:**
```
master:  A---B---C---S (un solo commit "squashed")
```

**✅ Vantaggi:**
- History lineare e pulita
- Un commit per feature

**❌ Svantaggi:**
- Perdi i messaggi dettagliati originali
- Non vedi le modifiche incrementali
- Difficile fare revert parziale

**Quando usarla:**
- Hai fatto tanti "WIP" o "fix typo"
- Vuoi nascondere commit intermedi
- La feature è semplice

---

### 3️⃣ Rebase and Merge

**Cosa fa:**
- "Riscrive la storia"
- Sposta i commit come se fossero fatti direttamente su master

**Come appare:**
```
master:  A---B---C---D---E (lineare)
```

**✅ Vantaggi:**
- History completamente lineare
- Mantiene commit separati

**❌ Svantaggi:**
- Cambia gli hash dei commit
- Più complesso
- Può creare problemi se altri hanno il tuo branch

**Quando usarla:**
- Lavori da solo
- Vuoi history super pulita
- Sei esperto di Git

---

## 📝 Schema Riassuntivo Comandi

### 🎯 Comandi di Base (Uso Quotidiano)

| Comando | Scopo | Quando Usarlo |
|---------|-------|---------------|
| `git status` | Vedere stato repository | **SEMPRE** prima di ogni operazione |
| `git add file.cs` | Preparare file per commit | Dopo aver modificato file |
| `git commit -m "msg"` | Salvare modifiche | Quando una funzionalità è completa |
| `git push origin branch` | Inviare a GitHub | Dopo commit, per backup/condivisione |
| `git pull origin branch` | Scaricare da GitHub | Prima di iniziare a lavorare |
| `git fetch origin` | Chiedere novità | Per verificare aggiornamenti |

---

### 🌿 Comandi Branch

| Comando | Scopo | Esempio |
|---------|-------|---------|
| `git branch` | Vedere branch | `git branch` |
| `git branch nome` | Creare branch | `git branch feature-x` |
| `git checkout nome` | Cambiare branch | `git checkout master` |
| `git checkout -b nome` | Crea + cambia | `git checkout -b feature-x` |
| `git branch -d nome` | Eliminare branch | `git branch -d feature-x` |

---

### 📊 Comandi Informativi

| Comando | Scopo | Output |
|---------|-------|--------|
| `git log` | Storia commit | Lista commit con hash, autore, data |
| `git log --oneline` | Storia concisa | Una riga per commit |
| `git log --graph` | Storia grafica | Visualizza branch e merge |
| `git diff` | Differenze non staged | Modifiche non ancora in staging |
| `git diff --staged` | Differenze staged | Modifiche pronte per commit |

---

### 🔄 Workflow Pull Request Completo

```bash
# === FASE 1: PREPARAZIONE ===
git checkout master              # Vai su master
git pull origin master           # Aggiorna master

# === FASE 2: SVILUPPO ===
git checkout -b feature-nome     # Crea branch feature
# ... fai modifiche ai file ...
git status                       # Verifica modifiche
git add file1.cs file2.cs       # Prepara file
git commit -m "Feat: ..."       # Commit locale

# === FASE 3: PUSH ===
git push origin feature-nome     # Invia a GitHub

# === FASE 4: PULL REQUEST (su GitHub Web) ===
# Vai su github.com
# Clicca "Compare & pull request"
# Compila descrizione
# Clicca "Create pull request"

# === FASE 5: MERGE (su GitHub Web) ===
# Dopo review/approvazione
# Clicca "Merge pull request"
# Scegli strategia (Create merge commit)
# Conferma merge

# === FASE 6: PULIZIA LOCALE ===
git checkout master              # Torna a master
git fetch origin                 # Chiedi novità
git pull origin master           # Scarica merge
git branch -d feature-nome       # Elimina branch locale
```

---

### ⚠️ Come Risolvere "Your branch is up to date" (Sospetto)

```bash
# PROBLEMA: Messaggio "up to date" ma non sei sicuro

# SOLUZIONE PASSO-PASSO:

# 1. Chiedi a GitHub se ci sono novità
git fetch origin

# 2. Verifica lo stato REALE
git status

# 3a. Se dice "behind by X commits":
git pull origin master           # Scarica aggiornamenti

# 3b. Se dice "up to date":
# ✅ Sei veramente aggiornato!
```

---

### 🆘 Comandi di Emergenza

| Situazione | Comando | Spiegazione |
|------------|---------|-------------|
| Ho fatto modifiche sbagliate | `git restore file.cs` | Annulla modifiche non staged |
| Ho fatto `add` per errore | `git restore --staged file.cs` | Rimuovi da staging |
| Voglio annullare ultimo commit | `git reset --soft HEAD~1` | Commit → staging |
| Voglio eliminare tutto | `git reset --hard HEAD` | ⚠️ PERICOLO: perdi tutto! |
| Branch sbagliato | `git stash` poi `git checkout` | Salva temporaneamente |

---

## 🐛 Troubleshooting

### Problema 1: "Your branch is behind"

```bash
# Messaggio:
Your branch is behind 'origin/master' by 2 commits

# Soluzione:
git pull origin master
```

---

### Problema 2: "Your branch is ahead"

```bash
# Messaggio:
Your branch is ahead of 'origin/master' by 1 commit

# Significa: Hai commit locali non ancora su GitHub

# Soluzione:
git push origin master
```

---

### Problema 3: "Your branch has diverged"

```bash
# Messaggio:
Your branch and 'origin/master' have diverged

# Significa: Hai commit locali E GitHub ha commit diversi

# Soluzione (semplice):
git pull origin master          # Scarica e fa merge automatico

# Soluzione (avanzata):
git fetch origin
git rebase origin/master        # Riscrive storia locale
```

---

### Problema 4: Merge conflict

```bash
# Messaggio durante pull/merge:
CONFLICT (content): Merge conflict in file.cs

# Soluzione:
# 1. Apri file.cs
# 2. Cerca i marker:
#    <<<<<<< HEAD
#    tuo codice
#    =======
#    codice da GitHub
#    >>>>>>> origin/master
# 3. Risolvi manualmente
# 4. Rimuovi i marker
# 5. Salva il file
# 6. git add file.cs
# 7. git commit -m "Risolto conflitto"
```

---

### Problema 5: "Permission denied" durante push

```bash
# Verifica autenticazione GitHub
git config --global user.name "Tuo Nome"
git config --global user.email "tua@email.com"

# Potrebbe servire configurare token di accesso su GitHub
```

---

## 📚 Risorse Utili

### Link Importanti

- 📖 [Git Documentation Ufficiale](https://git-scm.com/doc)
- 📖 [GitHub Guides](https://guides.github.com/)
- 📖 [Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)

### Visualizzatori Git

- 🎨 [GitKraken](https://www.gitkraken.com/) - Client grafico
- 🎨 [SourceTree](https://www.sourcetreeapp.com/) - Client grafico
- 🎨 [Git Graph VSCode](https://marketplace.visualstudio.com/items?itemName=mhutchie.git-graph) - Estensione VSCode

---

## 🎯 Consigli Finali

### Best Practice

1. **Commit frequenti e piccoli** - Meglio 10 commit piccoli che 1 gigante
2. **Messaggi descrittivi** - Il "te del futuro" ti ringrazierà
3. **Pull prima di push** - Evita conflitti
4. **Branch per feature** - Non lavorare direttamente su master
5. **Fetch spesso** - Controlla aggiornamenti regolarmente

### Da Evitare

- ❌ `git add .` senza verificare cosa stai aggiungendo
- ❌ Commit con messaggio "fix" o "wip"
- ❌ Push --force su branch condivisi
- ❌ Modificare commit già pushati (rebase pubblici)
- ❌ Committare file temporanei, password, chiavi

---

## 📝 Note Personali

Usa questo spazio per aggiungere i tuoi appunti, comandi personalizzati, o situazioni specifiche del tuo progetto.

```bash
# Esempi dei tuoi comandi frequenti:




```

---

**Fine della guida** 🎉

> Questa guida è un documento vivo - aggiornala man mano che impari nuovi concetti!

---

**Generato con ❤️ da Claude Code**
**Versione:** 1.0 - Dicembre 2024

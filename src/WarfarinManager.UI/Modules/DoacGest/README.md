# DoacGest React Module

Questa cartella deve contenere i file compilati del progetto React DoacGest.

## Come installare il modulo

### Step 1: Compila il progetto React DoacGest

Vai alla cartella del progetto React DoacGest e compila:

```bash
cd /path/to/doacgest-react-project
npm install
npm run build
```

### Step 2: Copia i file compilati

Copia **TUTTO il contenuto** della cartella `dist/` generata dal build qui:

```bash
# Windows (PowerShell)
xcopy /E /I dist "D:\Claude\TaoGest\src\WarfarinManager.UI\Modules\DoacGest"

# Windows (CMD)
xcopy /E /I dist "D:\Claude\TaoGest\src\WarfarinManager.UI\Modules\DoacGest"

# Linux/Mac
cp -R dist/* /path/to/TaoGest/src/WarfarinManager.UI/Modules/DoacGest/
```

### Step 3: Verifica struttura

Dopo la copia, la cartella dovrebbe contenere:

```
Modules/DoacGest/
├── index.html              ← File principale
├── assets/
│   ├── index-[hash].js     ← JavaScript compilato
│   ├── index-[hash].css    ← CSS compilato (opzionale)
│   └── ...                 ← Altri asset
└── README.md              ← Questo file
```

## Compilazione di TaoGEST

Per includere automaticamente questa cartella nel build di TaoGEST, aggiungi nel file `.csproj`:

```xml
<ItemGroup>
  <None Update="Modules\DoacGest\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Troubleshooting

### Errore: "Modulo DoacGest non trovato"

- Verifica che la cartella `Modules/DoacGest` esista nella directory di output
- Controlla che `index.html` sia presente

### Errore: "File index.html non trovato"

- Assicurati di aver copiato **tutti** i file dalla cartella `dist/`
- Verifica che il build React sia completato con successo

### WebView2 non carica l'applicazione

- Verifica che WebView2 Runtime sia installato
- Apri DevTools (F12 in Debug mode) per vedere eventuali errori JavaScript
- Controlla i log dell'applicazione in `%LocalAppData%\WarfarinManager\Logs\`

## Per maggiori informazioni

Consulta il file `INTEGRATION_TAOGEST.md` nella root del progetto.

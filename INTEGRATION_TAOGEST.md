# 🔌 Guida Integrazione DoacGest in TaoGEST

## ✅ Prerequisiti
- [x] Build completato (`npm run build`)
- [x] Cartella `dist/` generata con successo
- [x] WebView2 Runtime installato
- [x] NuGet package `Microsoft.Web.WebView2` in TaoGEST

---

## 📁 Step 1: Copiare i file nel progetto TaoGEST

```bash
# Dalla cartella doac, copia l'intera cartella dist in TaoGEST
xcopy /E /I dist "C:\Path\To\TaoGest\Modules\DoacGest"
```

Struttura finale in TaoGEST:
```
TaoGest/
├── TaoGest.sln
├── TaoGest/
│   ├── Modules/
│   │   └── DoacGest/
│   │       ├── index.html
│   │       └── assets/
│   │           └── index-[hash].js
│   └── ...
```

---

## 💻 Step 2: Codice C# - Aggiungere Window XAML

### File: `DoacGestWindow.xaml`

```xml
<Window x:Class="TaoGest.Modules.DoacGestWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:wv2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        Title="DoacGest Expert - Gestione Anticoagulanti"
        Height="900" Width="1400"
        WindowStartupLocation="CenterScreen"
        Background="#F8FAFC">
    <Grid>
        <!-- WebView2 Container -->
        <wv2:WebView2 x:Name="DoacGestWebView" />

        <!-- Loading Overlay (opzionale) -->
        <Border x:Name="LoadingOverlay"
                Background="#F8FAFC"
                Visibility="Visible">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                <TextBlock Text="⏳ Caricamento DoacGest Expert..."
                           FontSize="18" FontWeight="Bold"
                           Foreground="#4F46E5"
                           Margin="0,0,0,10"/>
                <ProgressBar IsIndeterminate="True" Width="200" Height="4"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

---

### File: `DoacGestWindow.xaml.cs`

```csharp
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace TaoGest.Modules
{
    public partial class DoacGestWindow : Window
    {
        private string _modulePath;
        private object _currentPatient; // Riferimento al paziente corrente in TaoGest

        public DoacGestWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        // Costruttore con paziente
        public DoacGestWindow(object patient) : this()
        {
            _currentPatient = patient;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Path al modulo DoacGest
                _modulePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Modules",
                    "DoacGest"
                );

                if (!Directory.Exists(_modulePath))
                {
                    MessageBox.Show(
                        $"Modulo DoacGest non trovato in:\n{_modulePath}\n\nVerifica l'installazione.",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    Close();
                    return;
                }

                // Inizializza WebView2
                await DoacGestWebView.EnsureCoreWebView2Async(null);

                // Configura host virtuale
                DoacGestWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "doacgest.local",
                    _modulePath,
                    CoreWebView2HostResourceAccessKind.Allow
                );

                // Listener messaggi da React
                DoacGestWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // Listener navigazione completata
                DoacGestWebView.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                };

                // Naviga alla app
                DoacGestWebView.CoreWebView2.Navigate("https://doacgest.local/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore inizializzazione DoacGest:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Close();
            }
        }

        // ========== COMUNICAZIONE DA REACT ==========
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                var message = JsonSerializer.Deserialize<WebMessage>(json);

                switch (message?.Type)
                {
                    case "MODULE_READY":
                        OnModuleReady();
                        break;

                    case "SAVE_PATIENT_DATA":
                        SavePatientData(message.Payload);
                        break;

                    case "REQUEST_PATIENT_DATA":
                        SendPatientDataToModule();
                        break;

                    case "EXPORT_REPORT":
                        ExportReport(message.Payload);
                        break;

                    case "PRINT_REQUEST":
                        PrintReport(message.Payload);
                        break;

                    case "DOSAGE_CHANGE":
                        NotifyDosageChange(message.Payload);
                        break;

                    case "SHOW_NOTIFICATION":
                        ShowNotification(message.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore comunicazione WebView:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ========== HANDLERS ==========

        private void OnModuleReady()
        {
            // Modulo React pronto - invia dati paziente se disponibili
            if (_currentPatient != null)
            {
                SendPatientDataToModule();
            }
        }

        private void SavePatientData(object payload)
        {
            try
            {
                // TODO: Implementa salvataggio in database TaoGest
                var jsonPayload = JsonSerializer.Serialize(payload);
                File.WriteAllText(
                    Path.Combine(_modulePath, "patient_data.json"),
                    jsonPayload
                );

                SendNotification("Dati salvati con successo!", "success");
            }
            catch (Exception ex)
            {
                SendNotification($"Errore salvataggio: {ex.Message}", "error");
            }
        }

        private void ExportReport(object payload)
        {
            try
            {
                // TODO: Implementa export in formato TaoGest
                var element = ((JsonElement)payload);
                var content = element.GetProperty("content").GetString();
                var filename = element.GetProperty("filename").GetString();

                var savePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TaoGest",
                    "Reports",
                    filename
                );

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                File.WriteAllText(savePath, content);

                MessageBox.Show(
                    $"Report esportato in:\n{savePath}",
                    "Export Completato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore export: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport(object payload)
        {
            try
            {
                // TODO: Implementa stampa tramite sistema di stampa TaoGest
                DoacGestWebView.CoreWebView2.ExecuteScriptAsync("window.print();");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore stampa: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NotifyDosageChange(object payload)
        {
            // TODO: Notifica a TaoGest di un cambio dosaggio
            // Puoi triggerare eventi, salvare in DB, ecc.
            SendNotification("Dosaggio aggiornato", "info");
        }

        private void ShowNotification(object payload)
        {
            var element = ((JsonElement)payload);
            var message = element.GetProperty("message").GetString();
            var type = element.GetProperty("type").GetString();

            var icon = type switch
            {
                "success" => MessageBoxImage.Information,
                "error" => MessageBoxImage.Error,
                "warning" => MessageBoxImage.Warning,
                _ => MessageBoxImage.None
            };

            MessageBox.Show(message, "DoacGest", MessageBoxButton.OK, icon);
        }

        // ========== COMUNICAZIONE A REACT ==========

        private void SendToReact(string type, object payload)
        {
            var message = new
            {
                type,
                payload,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var json = JsonSerializer.Serialize(message);
            DoacGestWebView.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void SendNotification(string message, string type)
        {
            SendToReact("SHOW_NOTIFICATION", new { message, type });
        }

        private void SendPatientDataToModule()
        {
            if (_currentPatient == null) return;

            // TODO: Mappa l'oggetto paziente TaoGest al formato DoacGest
            var patientData = new
            {
                name = GetPatientProperty("Name"),
                age = GetPatientProperty("Age"),
                weight = GetPatientProperty("Weight"),
                sex = GetPatientProperty("Sex"),
                creatinine = GetPatientProperty("Creatinine"),
                // ... altri campi
            };

            SendToReact("PATIENT_DATA", patientData);
        }

        private object GetPatientProperty(string propertyName)
        {
            // TODO: Implementa reflection o accesso diretto alle proprietà paziente
            return null;
        }

        // ========== MODELLO MESSAGGI ==========
        private class WebMessage
        {
            public string Type { get; set; }
            public object Payload { get; set; }
            public long Timestamp { get; set; }
        }
    }
}
```

---

## 🎯 Step 3: Aprire DoacGest da TaoGEST

### Esempio: Da Menu TaoGEST

```csharp
// In MainWindow.xaml.cs o dove gestisci il menu

private void MenuItem_OpenDoacGest_Click(object sender, RoutedEventArgs e)
{
    // Opzione 1: Senza paziente specifico
    var doacWindow = new Modules.DoacGestWindow();
    doacWindow.Show();

    // Opzione 2: Con paziente corrente
    // var doacWindow = new Modules.DoacGestWindow(CurrentPatient);
    // doacWindow.ShowDialog(); // Modale
}
```

### Esempio: Da Context Menu Paziente

```csharp
private void ContextMenu_AnalyzeDOAC_Click(object sender, RoutedEventArgs e)
{
    var selectedPatient = PatientListView.SelectedItem;

    if (selectedPatient == null)
    {
        MessageBox.Show("Seleziona un paziente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    var doacWindow = new Modules.DoacGestWindow(selectedPatient);
    doacWindow.ShowDialog();
}
```

---

## 📊 Mapping Dati Paziente

### Da TaoGest a DoacGest

Adatta questo mapping al tuo modello paziente:

```csharp
private object ConvertToPatientData(TaoGestPatient patient)
{
    return new
    {
        name = patient.FullName,
        age = CalculateAge(patient.DateOfBirth),
        weight = patient.Weight,
        sex = patient.Gender == "M" ? "M" : "F",
        creatinine = patient.LabResults?.Creatinine ?? 1.0,
        doac = patient.CurrentMedication?.Contains("Apixaban") == true ? "Apixaban" : "Rivaroxaban",
        indication = "Fibrillazione Atriale Non Valvolare",
        liverStatus = new
        {
            cirrhosis = patient.Diagnoses?.Contains("Cirrosi") ?? false,
            portalHypertension = false,
            abnormalFunction = patient.LabResults?.ALT > 40 || patient.LabResults?.AST > 40
        },
        otherRisks = new
        {
            hypertension = patient.Diagnoses?.Contains("Ipertensione") ?? false,
            bleedingHistory = patient.History?.BleedingEvents > 0,
            strokeHistory = patient.History?.StrokeEvents > 0,
            alcoholAbuse = false,
            antiplatelets = patient.CurrentMedication?.Contains("Aspirina") ?? false,
            nsaids = false
        }
    };
}

private int CalculateAge(DateTime birthDate)
{
    var today = DateTime.Today;
    var age = today.Year - birthDate.Year;
    if (birthDate.Date > today.AddYears(-age)) age--;
    return age;
}
```

---

## 🧪 Testing

### Test 1: Verifica caricamento modulo

1. Compila e avvia TaoGEST
2. Apri DoacGest
3. Verifica che l'overlay di caricamento scompaia
4. Controlla la console JavaScript (F12 in WebView2)

### Test 2: Comunicazione bidirezionale

```csharp
// Aggiungi button temporaneo per test
private void TestButton_Click(object sender, RoutedEventArgs e)
{
    var testData = new { name = "Test Patient", age = 70, weight = 75 };
    SendToReact("PATIENT_DATA", testData);
}
```

Verifica che i dati compaiano nell'UI React.

---

## 🐛 Troubleshooting

### WebView2 non carica l'app
- Verifica che `_modulePath` punti a `dist/`
- Controlla che `index.html` esista
- Apri DevTools: `DoacGestWebView.CoreWebView2.OpenDevToolsWindow();`

### Messaggi non arrivano
- Aggiungi breakpoint in `OnWebMessageReceived`
- Verifica JSON con try-catch
- Controlla console browser per errori JS

### Errori TypeScript
```bash
npm run build
```
Se fallisce, controlla errori nel terminale

---

## 🚀 Deploy

### Build Finale
```bash
npm run build
```

### Distribuire con TaoGEST
Includi la cartella `Modules/DoacGest` nel setup installer di TaoGEST.

---

## 📞 Supporto

Per problemi di integrazione, verifica:
1. WebView2 Runtime installato
2. Cartella `Modules/DoacGest` esistente
3. File `index.html` e `assets/` presenti
4. Permessi di lettura sulla cartella

---

**✅ Setup Completato!**

DoacGest Expert è ora integrato in TaoGEST e pronto per l'uso.

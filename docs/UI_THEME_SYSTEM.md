# Sistema di Temi TaoGEST

## Panoramica

TaoGEST supporta ora un sistema di temi modulare e centralizzato con supporto per modalità chiara (Light) e scura (Dark). Il sistema è stato progettato per essere facilmente estensibile e manutenibile.

## Architettura

### Struttura File

```
src/WarfarinManager.UI/
├── Resources/
│   ├── Themes/
│   │   ├── LightTheme.xaml      # Palette colori tema chiaro
│   │   └── DarkTheme.xaml       # Palette colori tema scuro
│   └── Styles/
│       ├── Icons.xaml           # Icone Segoe MDL2 Assets
│       ├── Buttons.xaml         # Stili pulsanti
│       └── Common.xaml          # Stili comuni (TextBox, DataGrid, ecc.)
├── Services/
│   └── ThemeManager.cs          # Servizio per gestione temi
└── Properties/
    ├── Settings.settings        # Configurazione app
    └── Settings.Designer.cs     # Generato automaticamente
```

### Componenti Principali

#### 1. ThemeManager

Servizio singleton che gestisce il cambio di tema runtime:

```csharp
// Cambia tema
ThemeManager.Instance.SetTheme(AppTheme.Dark);

// Toggle tra Light e Dark
ThemeManager.Instance.ToggleTheme();

// Ottieni tema corrente
var currentTheme = ThemeManager.Instance.CurrentTheme;

// Ascolta eventi di cambio tema
ThemeManager.Instance.ThemeChanged += (sender, e) => {
    // e.OldTheme, e.NewTheme
};
```

#### 2. ResourceDictionary

Tutti gli stili e i colori sono definiti in ResourceDictionary XAML:

- **LightTheme.xaml / DarkTheme.xaml**: Definiscono i colori usando `DynamicResource`
- **Icons.xaml**: Mappa le icone Segoe MDL2 Assets
- **Buttons.xaml**: Stili per pulsanti (Primary, Secondary, Success, Danger, Warning, ecc.)
- **Common.xaml**: Stili per controlli comuni (TextBox, ComboBox, DataGrid, ecc.)

#### 3. Palette Colori

Ogni tema definisce un set completo di colori semantici:

**Categorie di Colori:**
- **Primary**: Colore principale del brand
- **Semantic**: Success, Danger, Warning, Info
- **Background**: Window, Content, Card, Header, Toolbar, StatusBar
- **Border**: Border, BorderLight, BorderDark, FocusBorder
- **Text**: TextPrimary, TextSecondary, TextTertiary, TextOnPrimary, TextDisabled
- **Control**: Control background/border/hover/pressed/disabled
- **DataGrid**: Header, Row, AlternateRow, Hover, Selected

## Utilizzo negli XAML

### Riferimento ai Colori

Usa sempre `DynamicResource` per i colori per supportare il cambio tema runtime:

```xml
<!-- CORRETTO -->
<Border Background="{DynamicResource CardBackgroundBrush}"
        BorderBrush="{DynamicResource BorderBrush}">
    <TextBlock Foreground="{DynamicResource TextPrimaryBrush}"
               Text="Contenuto" />
</Border>

<!-- SCORRETTO - Non cambia con il tema -->
<Border Background="#FFFFFF" BorderBrush="#E0E0E0">
    <TextBlock Foreground="#333333" Text="Contenuto" />
</Border>
```

### Utilizzo Stili Predefiniti

```xml
<!-- Pulsanti -->
<Button Style="{StaticResource PrimaryButtonStyle}" Content="Salva" />
<Button Style="{StaticResource SecondaryButtonStyle}" Content="Annulla" />
<Button Style="{StaticResource SuccessButtonStyle}" Content="Conferma" />
<Button Style="{StaticResource DangerButtonStyle}" Content="Elimina" />
<Button Style="{StaticResource WarningButtonStyle}" Content="Attenzione" />
<Button Style="{StaticResource IconButtonStyle}">
    <TextBlock Text="{StaticResource Icon.Settings}"
               Style="{StaticResource IconStyle}" />
</Button>

<!-- Controlli Input -->
<TextBox Style="{StaticResource ModernTextBoxStyle}" />
<ComboBox Style="{StaticResource ModernComboBoxStyle}" />
<DatePicker Style="{StaticResource ModernDatePickerStyle}" />
<CheckBox Style="{StaticResource ModernCheckBoxStyle}" />

<!-- Card -->
<Border Style="{StaticResource CardStyle}">
    <StackPanel>
        <TextBlock Style="{StaticResource SectionHeaderStyle}"
                   Text="Titolo Sezione" />
        <TextBlock Style="{StaticResource InputLabelStyle}"
                   Text="Etichetta:" />
        <TextBlock Style="{StaticResource ValueTextStyle}"
                   Text="Valore" />
    </StackPanel>
</Border>

<!-- DataGrid -->
<DataGrid Style="{StaticResource ModernDataGridStyle}" />

<!-- TabControl -->
<TabControl Style="{StaticResource ModernTabControlStyle}">
    <TabItem Style="{StaticResource ModernTabItemStyle}" Header="Tab 1" />
    <TabItem Style="{StaticResource ModernTabItemStyle}" Header="Tab 2" />
</TabControl>

<!-- Alert Boxes -->
<Border Style="{StaticResource InfoBoxStyle}">
    <TextBlock Text="Info message" />
</Border>
<Border Style="{StaticResource SuccessBoxStyle}">
    <TextBlock Text="Success message" />
</Border>
<Border Style="{StaticResource WarningBoxStyle}">
    <TextBlock Text="Warning message" />
</Border>
<Border Style="{StaticResource DangerBoxStyle}">
    <TextBlock Text="Error message" />
</Border>
```

### Icone Segoe MDL2 Assets

Usa il font integrato di Windows per icone vettoriali scalabili:

```xml
<!-- Icona semplice -->
<TextBlock Text="{StaticResource Icon.Add}"
           Style="{StaticResource IconStyle}" />

<!-- Icona grande -->
<TextBlock Text="{StaticResource Icon.Calendar}"
           Style="{StaticResource LargeIconStyle}" />

<!-- Icona piccola -->
<TextBlock Text="{StaticResource Icon.Check}"
           Style="{StaticResource SmallIconStyle}" />

<!-- Icona colorata -->
<TextBlock Text="{StaticResource Icon.Heart}"
           Style="{StaticResource DangerIconStyle}" />
<TextBlock Text="{StaticResource Icon.CheckMark}"
           Style="{StaticResource SuccessIconStyle}" />

<!-- Pulsante con icona -->
<Button Style="{StaticResource PrimaryButtonStyle}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="{StaticResource Icon.Save}"
                   Style="{StaticResource IconStyle}"
                   Margin="0,0,8,0" />
        <TextBlock Text="Salva" />
    </StackPanel>
</Button>
```

### Icone Disponibili

```
Icon.Add, Icon.AddFriend, Icon.Calendar, Icon.Contact, Icon.ContactInfo
Icon.Copy, Icon.Delete, Icon.Document, Icon.Edit, Icon.Flag
Icon.Health, Icon.Heart, Icon.HeartBroken, Icon.Home, Icon.Important
Icon.Info, Icon.List, Icon.Medical, Icon.Medication, Icon.More
Icon.Page, Icon.Paste, Icon.People, Icon.Pills, Icon.Print
Icon.Refresh, Icon.Report, Icon.Save, Icon.Search, Icon.Settings
Icon.Stethoscope, Icon.Switch, Icon.Target, Icon.Warning, Icon.Chart
Icon.ChartLine, Icon.ChartBar, Icon.Back, Icon.Forward, Icon.Up
Icon.Down, Icon.Close, Icon.Check, Icon.CheckMark, Icon.Cancel
Icon.Filter, Icon.Sort, Icon.Sun, Icon.Moon, Icon.Database
Icon.Export, Icon.Import, Icon.Download, Icon.Upload, Icon.Sync
Icon.Link, Icon.Unlink, Icon.Hospital, Icon.Emergency, Icon.Clock
Icon.History, Icon.TestTube, Icon.DNA, Icon.Blood
```

## UI/UX del Toggle Tema

Il pulsante per cambiare tema si trova nella StatusBar in basso a destra dell finestra principale:

- **Icona Sole** (☀️): Visualizzata quando il tema è scuro → Clicca per passare al tema chiaro
- **Icona Luna** (🌙): Visualizzata quando il tema è chiaro → Clicca per passare al tema scuro

La preferenza dell'utente viene salvata automaticamente in `Settings` e ripristinata all'avvio.

## Estendere il Sistema

### Aggiungere un Nuovo Colore

1. Aggiungi il colore in **LightTheme.xaml**:
```xml
<SolidColorBrush x:Key="MyCustomBrush" Color="#FF5733"/>
```

2. Aggiungi la variante in **DarkTheme.xaml**:
```xml
<SolidColorBrush x:Key="MyCustomBrush" Color="#FF8C66"/>
```

3. Usa nei tuoi XAML:
```xml
<Border Background="{DynamicResource MyCustomBrush}" />
```

### Aggiungere un Nuovo Stile

1. Aggiungi lo stile in **Common.xaml** (o crea un nuovo file):
```xml
<Style x:Key="MyCustomStyle" TargetType="Button">
    <Setter Property="Background" Value="{DynamicResource PrimaryBrush}"/>
    <Setter Property="Foreground" Value="{DynamicResource TextOnPrimaryBrush}"/>
    <!-- ... altre proprietà ... -->
</Style>
```

2. Se crei un nuovo file, registralo in **App.xaml**:
```xml
<ResourceDictionary.MergedDictionaries>
    <!-- ... altri dizionari ... -->
    <ResourceDictionary Source="pack://application:,,,/Resources/Styles/MyCustomStyles.xaml"/>
</ResourceDictionary.MergedDictionaries>
```

### Aggiungere un Nuovo Tema

1. Crea `Resources/Themes/MyTheme.xaml` con la stessa struttura di LightTheme.xaml
2. Aggiungi l'enum in `ThemeManager.cs`:
```csharp
public enum AppTheme
{
    Light,
    Dark,
    MyTheme  // <-- Nuovo tema
}
```
3. Aggiorna il metodo `ApplyTheme` in `ThemeManager.cs`

## Best Practices

1. **Sempre `DynamicResource` per i colori**: Mai hardcodare colori (#RRGGBB) negli XAML
2. **Riutilizzare stili esistenti**: Prima di creare un nuovo stile, verifica se ne esiste già uno adatto
3. **Semantic naming**: Usa nomi semantici per i colori (PrimaryBrush, SuccessBrush) invece di nomi descrittivi (BlueBrush, GreenBrush)
4. **Testare entrambi i temi**: Assicurati che ogni nuovo componente sia leggibile in entrambi i temi
5. **Icone vettoriali**: Preferisci Segoe MDL2 Assets alle immagini PNG/JPG per le icone

## Migrazione Codice Esistente

Per migrare view esistenti al nuovo sistema di temi:

1. **Sostituisci colori hardcoded**:
   - `Background="#FFFFFF"` → `Background="{DynamicResource WindowBackgroundBrush}"`
   - `Foreground="#333333"` → `Foreground="{DynamicResource TextPrimaryBrush}"`

2. **Usa stili predefiniti**:
   - Rimuovi stili inline e usa gli stili dal ResourceDictionary

3. **Sostituisci emoji con icone MDL2**:
   - `Text="📋"` → `Text="{StaticResource Icon.Document}" Style="{StaticResource IconStyle}"`

## Risoluzione Problemi

### Il tema non cambia

- Verifica di usare `DynamicResource` e non `StaticResource` per i colori
- Assicurati che `ThemeManager.Instance.Initialize()` sia chiamato in `App.xaml.cs`

### Icone non visualizzate

- Verifica che il file `Icons.xaml` sia incluso nei MergedDictionaries di `App.xaml`
- Usa `StaticResource` per le icone (i glifi non cambiano tra temi)

### Stile non trovato

- Verifica che il file di stili sia referenziato in `App.xaml`
- Controlla l'ortografia della chiave dello stile

## Risorse

- [Segoe MDL2 Assets Icon List](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)
- [WPF Resource Dictionaries](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/merged-resource-dictionaries)
- [Dynamic vs Static Resources](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/staticresource-markup-extension)

using System;
using System.Windows;

namespace WarfarinManager.UI.Services;

/// <summary>
/// Manages application theme switching between Light and Dark modes
/// </summary>
public class ThemeManager
{
    private const string LightThemeUri = "pack://application:,,,/Resources/Themes/LightTheme.xaml";
    private const string DarkThemeUri = "pack://application:,,,/Resources/Themes/DarkTheme.xaml";
    private const string ThemeSettingKey = "AppTheme";

    private static ThemeManager? _instance;
    private static readonly object _lock = new();

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ThemeManager();
                }
            }
            return _instance;
        }
    }

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private AppTheme _currentTheme;

    public AppTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                var oldTheme = _currentTheme;
                _currentTheme = value;
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, value));
            }
        }
    }

    private ThemeManager()
    {
        _currentTheme = LoadThemeFromSettings();
    }

    /// <summary>
    /// Initializes the theme system and applies the saved theme
    /// </summary>
    public void Initialize()
    {
        ApplyTheme(CurrentTheme);
    }

    /// <summary>
    /// Switches to the specified theme
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        if (CurrentTheme != theme)
        {
            ApplyTheme(theme);
            SaveThemeToSettings(theme);
            CurrentTheme = theme;
        }
    }

    /// <summary>
    /// Toggles between Light and Dark themes
    /// </summary>
    public void ToggleTheme()
    {
        var newTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        SetTheme(newTheme);
    }

    private void ApplyTheme(AppTheme theme)
    {
        var themeUri = theme == AppTheme.Light ? LightThemeUri : DarkThemeUri;
        var newTheme = new ResourceDictionary { Source = new Uri(themeUri) };

        // Find and remove existing theme dictionary
        ResourceDictionary? existingTheme = null;
        foreach (var dict in Application.Current.Resources.MergedDictionaries)
        {
            if (dict.Source?.ToString().Contains("/Themes/") == true)
            {
                existingTheme = dict;
                break;
            }
        }

        if (existingTheme != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
        }

        // Add new theme at the beginning to ensure it has priority
        Application.Current.Resources.MergedDictionaries.Insert(0, newTheme);
    }

    private AppTheme LoadThemeFromSettings()
    {
        try
        {
            var savedTheme = Properties.Settings.Default.AppTheme;
            if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
            {
                return theme;
            }
        }
        catch
        {
            // If loading fails, return default
        }

        return AppTheme.Light;
    }

    private void SaveThemeToSettings(AppTheme theme)
    {
        try
        {
            Properties.Settings.Default.AppTheme = theme.ToString();
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Silently fail if saving doesn't work
        }
    }
}

/// <summary>
/// Available application themes
/// </summary>
public enum AppTheme
{
    Light,
    Dark
}

/// <summary>
/// Event args for theme changed event
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public AppTheme OldTheme { get; }
    public AppTheme NewTheme { get; }

    public ThemeChangedEventArgs(AppTheme oldTheme, AppTheme newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}

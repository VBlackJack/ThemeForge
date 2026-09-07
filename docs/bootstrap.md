# Application startup and Windows integration

English | [Français](fr/bootstrap.md)

[Introduction](integration-guide.md) | [Controls](controls.md) | [Custom themes](custom-themes.md)

## 5. Bootstrap App.xaml.cs

`AddThemeForge` registers the engine. `UseThemeForge` restores preferences or applies defaults, starts Windows following, and saves subsequent changes.

```csharp
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme;
using ThemeForge.Theme.DependencyInjection;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ServiceCollection services = new ServiceCollection();
        services.AddThemeForge(this, options =>
        {
            options.DefaultTheme = ThemeNames.Drakul;
            options.WindowsFollow = new WindowsFollowOptions
            {
                LightTheme = ThemeNames.Folio,
                DarkTheme = ThemeNames.Drakul,
            };
            options.FollowWindowsByDefault = true;
            options.ApplicationName = "MyApp";
        });
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        _services = services.BuildServiceProvider();
        _services.UseThemeForge();
        _services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
```

`ThemeForgeOptions`:

- `DefaultTheme`: used when no saved choice or active following takes precedence.
- `WindowsFollow`: the light/dark pair and `FollowAccent` (default `true`). Required for both first-run following and restoring follow mode.
- `FollowWindowsByDefault`: enables following on first run when no preference exists.
- `ApplicationName`: enables persistence. Without it, there is no automatic restore or save.
- `PreferenceStore`, `DefaultAccentTint`, `AvailableThemes`, `OnError`: advanced settings.

Registration exposes one shared singleton through `IThemeService`, `ISystemThemeFollower`, `ISystemAccentFollower`, and `IWindowsThemeFollower`. `TryAddSingleton` preserves interfaces registered by the consumer beforehand.

`UseThemeForge()` returns an `IDisposable`, is idempotent, and cleans up when the container is disposed. The main window receives its dependencies through its constructor.

## 6. Follow Windows theme and accent

`WindowsFollow` and `FollowWindowsByDefault` cover normal startup. For a manual follow toggle, inject `IWindowsThemeFollower`:

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IWindowsThemeFollower _follower;
    public ShellViewModel(IWindowsThemeFollower follower) => _follower = follower;

    [RelayCommand]
    private void EnableFollow()
        => _follower.FollowWindows(new WindowsFollowOptions
        {
            LightTheme = ThemeNames.Folio,
            DarkTheme = ThemeNames.Drakul,
        });
}
```

Following uses separate interfaces to preserve `IThemeService` compatibility.

- Manual `ApplyTheme(...)` disables light/dark following and restores user control.
- Manual `ApplyAccentTint(...)` disables accent following.
- An unavailable Windows light/dark state leaves the current theme unchanged.

Observe `ISystemThemeFollower.IsFollowingSystem` and `ISystemAccentFollower.IsFollowingSystemAccent`. Without DI, cast the `ThemeService` instance to the relevant additive interfaces.

## 7. Persist user preferences

When `ApplicationName` is set, startup restores the previous theme, accent, or follow mode, and saves changes automatically. The default store writes JSON atomically to `%AppData%/<ApplicationName>/preferences.json`.

Persistence records the user's intent: a Windows theme change while following does not turn the resolved theme into a manual choice.

Supply a custom location:

```csharp
options.PreferenceStore = new JsonThemePreferenceStore(preferencePath, onError: Log);
```

Or implement `IThemePreferenceStore.Load()` and `Save(...)` for another backend. The default `Load()` returns no preference for missing, malformed, or unknown-version files. `JsonThemePreferenceStore.FilePath` exposes the location for features such as resetting preferences.

## 8. Theme the title bar

Call the extension after `InitializeComponent()`:

```csharp
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, IThemeService themeService)
    {
        InitializeComponent();
        DataContext = viewModel;
        this.ApplyThemeForgeTitleBar(themeService);
    }
}
```

The title bar derives light/dark mode and colours from the theme and updates on `ThemeChanged`. Caption colours require Windows 11; immersive dark mode requires Windows 10 2004 or later. Older systems retain unsupported OS defaults without errors.

`TitleBarOptions` can override caption, text, and border colours. Null values derive from the current theme.

## Preference validation and intent notifications

Unknown `AccentTint` values in stored preferences fall back to the configured default.
Invalid `DefaultAccentTint` options fail registration before the service collection changes.
`ApplyAccentTint` validates and prepares its resource dictionary before changing state.

`ThemeService` implements the optional `IThemeIntentNotifier` interface. Its
`ThemeIntentChanged` event reports changes to Windows-follow intent even when the visible
theme and revision are unchanged. Bootstrap subscribes to it for auto-save. Custom theme
services can implement this capability without changing `IThemeService`.
Calling `FollowWindows` with `FollowAccent = false` disables an existing accent follower.

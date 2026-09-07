# Guide d’intégration : partie 2

[English](../bootstrap.md) | [Introduction](integration-guide.md) | [Initialisation](bootstrap.md) | [Contrôles](controls.md) | [Thèmes personnalisés](custom-themes.md)

## 5. Bootstrap App.xaml.cs : une ligne pour tout câbler

`AddThemeForge` enregistre le moteur ; `UseThemeForge` orchestre le démarrage :
il restaure le choix persisté de l'utilisateur (ou applique le défaut), arme le
suivi Windows, et sauvegarde automatiquement chaque changement.

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
            options.ApplicationName = "MonApp";
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

`ThemeForgeOptions` :

- `DefaultTheme` : le thème appliqué quand il n'y a ni choix persisté ni suivi.
- `WindowsFollow` : la paire clair/sombre (et `FollowAccent`, défaut `true`).
  Nécessaire pour le 1er run en suivi ET pour restaurer le mode suivi.
- `FollowWindowsByDefault` : au 1er run sans préférence, suit Windows si `true`.
- `ApplicationName` : active la persistance (section 7). Sans lui, ni restore ni
  auto-save.
- `PreferenceStore`, `DefaultAccentTint`, `AvailableThemes`, `OnError` :
  réglages avancés.

`AddThemeForge` enregistre `ThemeService` comme singleton partagé, exposé via
`IThemeService`, `ISystemThemeFollower`, `ISystemAccentFollower` et
`IWindowsThemeFollower`. Les quatre interfaces résolvent la même instance, sans
aucun cast. `TryAddSingleton` est utilisé : si tu enregistres toi-même l'une de
ces interfaces avant l'appel, ta version est conservée.

`UseThemeForge()` retourne un `IDisposable` ; il est idempotent (un second appel
ne ré-abonne rien) et se nettoie quand le container est disposé.

La fenêtre principale est résolue via DI puis affichée. Son constructeur reçoit
ses dépendances (section 8 pour la barre de titre).

## 6. Suivre le thème et l'accent Windows

Le suivi est armé par le bootstrap via `WindowsFollow` + `FollowWindowsByDefault`.
Tu n'as rien d'autre à écrire pour le cas standard.

Pour le piloter à la main (par exemple un toggle "Suivre Windows"), injecte
`IWindowsThemeFollower` et appelle le preset, qui arme clair/sombre ET accent en
un seul appel :

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

Les capacités de suivi vivent sur des interfaces SÉPARÉES pour ne pas modifier
`IThemeService` (gelé pour la stabilité SemVer).

Interactions à connaître :

- Un `ApplyTheme(...)` manuel désactive le suivi clair/sombre : le choix explicite
  de l'utilisateur reprend la main. C'est aussi la façon d'arrêter le suivi.
- Un `ApplyAccentTint(...)` manuel désactive le suivi d'accent.
- Si Windows ne fournit pas d'état clair/sombre, ThemeForge conserve le thème
  courant.

L'état s'observe via `IsFollowingSystem` (`ISystemThemeFollower`) et
`IsFollowingSystemAccent` (`ISystemAccentFollower`).

> Sans DI, tu peux caster ton instance `ThemeService` vers `IWindowsThemeFollower`
> ou les deux followers : ces interfaces sont additives et non-breaking.

## 7. Persister le choix de l'utilisateur

Quand `options.ApplicationName` est défini, `UseThemeForge` restaure au démarrage
le dernier choix de l'utilisateur (thème explicite, accent, ou mode suivi) et le
sauvegarde à chaque changement. Le store par défaut écrit un JSON sous
`%AppData%/<ApplicationName>/preferences.json`, en écriture atomique.

L'intention est persistée, pas le thème momentané : en mode suivi, une bascule
clair/sombre de Windows ne fige pas le thème résolu.

Pour un emplacement ou un backend custom (registre, cloud), fournis ta propre
implémentation :

```csharp
options.PreferenceStore = new JsonThemePreferenceStore(monChemin, onError: Log);
```

ou implémente `IThemePreferenceStore` (deux membres, `Load()` / `Save(...)`).
`Load()` ne lève jamais : un fichier absent, corrompu ou de version inconnue
renvoie "aucune préférence". `JsonThemePreferenceStore.FilePath` te donne le
chemin (utile pour un bouton "réinitialiser mes préférences").

## 8. Thématiser la barre de titre

Dans le code-behind de ta fenêtre, appelle l'extension après
`InitializeComponent`. La caption suit le thème courant (mode clair/sombre déduit
du fond, couleurs dérivées du thème) et se re-synchronise sur `ThemeChanged`.

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

Le rendu est best-effort selon la version de Windows : les couleurs de caption
demandent Windows 11, le mode sombre immersif Windows 10 2004+. Sur un OS plus
ancien, la barre reste au défaut OS, sans erreur. `TitleBarOptions` permet de
surcharger les couleurs (caption, texte, bordure) ; laissées nulles, elles
dérivent du thème.

## Validation des préférences et notification de l’intention

Une valeur `AccentTint` inconnue dans les préférences déclenche le repli sur la valeur
par défaut configurée. Une option `DefaultAccentTint` invalide fait échouer l’enregistrement
avant modification des services. `ApplyAccentTint` valide et prépare son dictionnaire
de ressources avant de modifier l’état actif.

`ThemeService` implémente l’interface optionnelle `IThemeIntentNotifier`. Son événement
`ThemeIntentChanged` signale les changements d’intention de suivi Windows même si le thème
visible et sa révision restent identiques. Le bootstrap s’y abonne pour la sauvegarde
automatique. Les services personnalisés peuvent l’implémenter sans modifier `IThemeService`.
Un appel à `FollowWindows` avec `FollowAccent = false` arrête un suivi d’accent déjà actif.

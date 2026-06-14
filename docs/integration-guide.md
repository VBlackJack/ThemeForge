# Guide d'intégration ThemeForge

Ce guide t'aide à brancher ThemeForge dans une application WPF tierce.

## 1. Préambule

Ce guide est écrit pour toi si :

- tu développes une app WPF desktop ;
- tu veux un moteur de théming propre sans refaire les styles à la main ;
- tu veux changer de thème au runtime, suivre Windows et mémoriser le choix ;
- tu veux utiliser les styles natifs et les composites ThemeForge.

À la fin, tu auras :

- un thème posé au démarrage, restauré depuis le choix persisté de l'utilisateur ;
- le suivi clair/sombre et accent de Windows, opt-in ;
- la barre de titre alignée sur le thème ;
- les styles WPF natifs appliqués automatiquement ;
- un `IThemeService` injectable et une bascule runtime via `ApplyTheme` ;
- les tokens ThemeForge utilisables dans tes propres styles.

Prérequis :

- .NET 10 SDK ;
- une app WPF ciblant `net10.0-windows` ;
- un `.csproj` SDK-style moderne.

ThemeForge est publié sur **nuget.org** (restore anonyme, aucune authentification)
et aussi sur GitHub Packages. L'intégration recommandée se fait par
`PackageReference` depuis nuget.org.

## 2. La voie rapide : le template

Le plus simple est de partir du template, qui génère une app déjà entièrement
câblée (suivi Windows, persistance, barre de titre), prête à lancer.

```pwsh
dotnet new install ThemeForge.Templates
dotnet new tf-wpf -n MonApp
```

Le projet généré est la référence runnable de tout ce qui suit : son `App.xaml.cs`
montre le bootstrap complet, sa `MainWindow` montre le sélecteur de thème, le
toggle "Suivre Windows" et la barre de titre thématisée.

Si tu intègres dans une app WPF existante, suis les sections ci-dessous.

## 3. Référencer ThemeForge

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="2.0.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.0.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.0.0" />
</ItemGroup>
```

Les packages se restaurent depuis nuget.org en accès anonyme, sans `nuget.config`
ni authentification.

`ThemeForge.Theme` contient le moteur :

- `IThemeService`, `ThemeService`, `ThemeNames` ;
- les suivis Windows opt-in (`IWindowsThemeFollower`, `ISystemThemeFollower`,
  `ISystemAccentFollower`) ;
- la persistance (`IThemePreferenceStore`, `JsonThemePreferenceStore`) ;
- le theming de barre de titre (`ApplyThemeForgeTitleBar`) ;
- les 16 `ResourceDictionary` de thèmes.

Le package coeur reste sans dépendance NuGet.

`ThemeForge.Controls` contient les styles WPF natifs, les composites et l'index
WPF `Themes/Generic.xaml`.

`ThemeForge.Theme.DependencyInjection` apporte le wiring `AddThemeForge` et
`UseThemeForge` (section 5), et tracte
`Microsoft.Extensions.DependencyInjection.Abstractions`.

Si ton app n'a pas déjà de container DI, ajoute aussi l'implémentation :

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
```

> Source secondaire (optionnelle). ThemeForge est aussi publié sur GitHub
> Packages (compte VBlackJack). Cette source exige un PAT GitHub avec le scope
> `read:packages`, même en lecture. nuget.org reste le chemin par défaut et ne
> demande aucune authentification.

## 4. Bootstrap App.xaml : merger les styles, jamais un thème

Dans `App.xaml`, merge UNIQUEMENT le dictionnaire de styles natifs. Le thème se
pose au runtime, possédé de bout en bout par le bootstrap (section 5).

```xml
<Application
    x:Class="MonApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

> **Avertissement.** Ne merge jamais un thème en statique dans `App.xaml`. Un
> thème statique n'est pas marqué par `ThemeService`, donc jamais retiré, et il
> reste en dernière position du merge : sur une clé dupliquée, WPF donne la
> priorité au dernier dictionnaire. Ses brushes l'emportent alors sur le thème
> appliqué au runtime, et le switch de thème change `CurrentTheme` sans aucun
> effet visible. Le thème est donc possédé exclusivement par le bootstrap.

Retire aussi `StartupUri` de `App.xaml` : la fenêtre principale est créée et
affichée par le bootstrap (section 5).

Les styles natifs sont implicites. Tu écris un `<Button>`, un `<TextBox>` ou un
`<DataGrid>` normal, et WPF applique le style ThemeForge dès que le dictionnaire
est mergé.

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

## 9. Utiliser les contrôles natifs stylés

Le point d'entrée consommateur est
`ThemeForge.Controls;component/Styles/Studio.xaml`. Les styles sont implicites :
tu n'as rien à référencer par clé.

23 contrôles WPF natifs sont stylés, dont :

- `Button`, `ToggleButton`, `RepeatButton`, `CheckBox`, `RadioButton` ;
- `TextBox`, `PasswordBox`, `ComboBox`, `ComboBoxItem` ;
- `ListBox`, `ListBoxItem`, `ListView`, `TreeView`, `TreeViewItem`, `DataGrid` ;
- `TabControl`, `TabItem`, `GroupBox`, `Expander` ;
- `Slider`, `ProgressBar`, `ScrollBar`, `StatusBar`.

Les templates couvrent aussi des éléments internes (headers et cellules de
`DataGrid`, `GridViewColumnHeader`, `StatusBarItem`, `Thumb`) et le `ToolTip`.

```xml
<StackPanel>
    <TextBox Width="240" Text="Search"/>
    <Button Content="Apply" Margin="0,8,0,0"/>
    <ProgressBar Value="65" Height="14"/>
</StackPanel>
```

## 10. Utiliser les composites

Ajoute le namespace XAML :

```xml
xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls"
```

Les 13 composites livrés :

- `Card` : conteneur avec header, body et footer optionnels.
- `IconButton` : bouton avec icône vectorielle et label optionnel.
- `Badge` : pastille de statut compacte.
- `Chip` : jeton sélectionnable, optionnellement supprimable.
- `ToggleSwitch` : switch basé sur `ToggleButton`.
- `Avatar` : initiales ou image dans un cercle.
- `SearchBox` : champ de recherche avec placeholder, clear et commande.
- `Toast` : notification éphémère avec titre, message et sévérité.
- `ToastHost` : pile verticale qui héberge et retire les toasts.
- `Breadcrumb` : fil d'Ariane cliquable.
- `Dialog` : surface de dialogue avec header, contenu, footer et accent.
- `NumericUpDown` : saisie numérique avec boutons d'incrémentation.
- `SegmentedControl` : groupe de segments à sélection unique.

```xml
<StackPanel xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls">
    <dfc:Card Header="Profil">
        <TextBlock Text="Compte synchronisé."/>
    </dfc:Card>
    <dfc:IconButton Label="Save" Margin="0,8,0,0"/>
    <dfc:Badge Content="Ready" Severity="Success"/>
</StackPanel>
```

Accessibilité : 14 `AutomationPeer` custom couvrent les composites et les
conteneurs d'items (`BreadcrumbItem`, `SegmentItem`). Si `Content` ou `Header`
reçoit autre chose qu'une chaîne, le peer ne fabrique pas de nom accessible pour
éviter d'annoncer un nom de type .NET ; définis alors `AutomationProperties.Name`.

```xml
<dfc:Chip AutomationProperties.Name="Filtre actif">
    <Rectangle Width="12" Height="12"/>
</dfc:Chip>
```

## 11. Utiliser les design tokens

Les tokens non-couleur sont centralisés dans
`src/ThemeForge.Theme/Themes/Shared/DesignTokens.xaml`, mergé par chaque variante.

- `SpacingNone` à `SpacingXxxl` (`Thickness`)
- `RadiusNone` à `RadiusFull` (`CornerRadius`)
- `FontSizeXs` à `FontSizeXl` (`Double`)

Les brushes canoniques (`BackgroundBrush`, `ForegroundBrush`, `CommentBrush`,
`CyanBrush`, etc.) et sémantiques (`SurfaceBrush`, `AccentBrush`,
`TextPrimaryBrush`, `BorderBrush`, `SuccessBrush`, etc.) sont déclarés dans chaque
fichier de thème. Utilise `DynamicResource` pour suivre le swap de thème au
runtime.

```xml
<Style x:Key="PanelTitle" TargetType="{x:Type TextBlock}">
    <Setter Property="Foreground" Value="{DynamicResource AccentBrush}"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="FontSize" Value="{DynamicResource FontSizeLg}"/>
    <Setter Property="Margin" Value="{DynamicResource SpacingMd}"/>
</Style>
```

Les couleurs sont aussi accessibles avec le suffixe `Color` (`AccentColor`,
`SurfaceColor`, etc.).

## 12. Basculer de thème au runtime

`IThemeService.ApplyTheme(string name)` applique un thème connu ;
`AvailableThemes` donne la liste stable des noms. `ApplyTheme` est idempotent.

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    public IReadOnlyList<string> Themes => _themeService.AvailableThemes;

    public ShellViewModel(IThemeService themeService) => _themeService = themeService;

    [ObservableProperty]
    private string? _selectedTheme;

    partial void OnSelectedThemeChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _themeService.ApplyTheme(value);
        }
    }
}
```

```xml
<ComboBox ItemsSource="{Binding Themes}"
          SelectedItem="{Binding SelectedTheme, Mode=TwoWay}"/>
```

`ThemeRevision` est incrémenté avant `ThemeChanged`, utile pour forcer des
converters, multi-bindings ou caches visuels à se ré-évaluer.

```csharp
_themeService.ThemeChanged += (_, e) =>
{
    CurrentThemeName = e.CurrentTheme;
    CurrentThemeRevision = e.Revision;
};
```

> Avec la persistance active (section 7), pense à refléter `IsFollowingSystem`
> dans ton toggle "Suivre Windows" : choisir un thème manuellement coupe le suivi,
> et l'auto-save persiste ce choix. La `MainWindow` du template montre ce câblage.

## 13. Créer ta propre variante

- crée `src/ThemeForge.Theme/Themes/<Name>.xaml` (nom PascalCase, ASCII, sans
  espace) ;
- ajoute le header d'attribution obligatoire (`NOTICE` est la source canonique
  d'attribution des palettes) ;
- déclare les mêmes clés `Color` et `Brush` que les thèmes existants ;
- ajoute le nom dans `ThemeNames.cs` et `ThemeNames.All`.

Pour une liste de thèmes custom à l'enregistrement :

```csharp
services.AddThemeForge(this, options =>
{
    options.AvailableThemes = ThemeNames.All.Append("MyTheme").ToArray();
    options.DefaultTheme = "MyTheme";
});
```

## 14. Limitations connues

ThemeForge cible WPF desktop. Il ne cible pas WinForms, UWP, WinUI 3, Avalonia ni
MAUI.

Le designer Visual Studio ne prévisualise pas fidèlement la bascule runtime : il
travaille en design-time, alors que `ThemeService` agit en runtime sur
`Application.Resources.MergedDictionaries`. Si un style semble absent dans le
designer, lance l'app et vérifie le rendu réel.

Le theming de barre de titre est best-effort selon la version de Windows
(section 8).

Liens utiles :

- [README](../README.md)
- [NOTICE](../NOTICE)

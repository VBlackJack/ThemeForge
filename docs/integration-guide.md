# Guide d'intégration ThemeForge

Ce guide t'aide à brancher ThemeForge dans une application WPF tierce.

## 1. Préambule

Ce guide est écrit pour toi si :

- tu développes une app WPF desktop ;
- tu veux une base Dracula propre sans refaire les styles à la main ;
- tu veux pouvoir changer de thème au runtime ;
- tu veux utiliser les styles natifs et les composites ThemeForge.

À la fin, tu auras :

- un thème par défaut chargé au démarrage ;
- les styles WPF natifs appliqués automatiquement ;
- un `IThemeService` injectable ;
- une bascule runtime via `ApplyTheme`;
- les tokens ThemeForge utilisables dans tes propres styles.

Prérequis :

- .NET 10 SDK ;
- une app WPF ciblant `net10.0-windows` ;
- un `.csproj` SDK-style moderne.

ThemeForge ne publie pas encore de package NuGet.

Pour l'instant, l'intégration se fait par `ProjectReference`.

## 2. Référencer ThemeForge dans ton projet

Ajoute les deux projets à ton `.csproj` consommateur.

Adapte le chemin relatif à ta structure.

```xml
<ItemGroup>
  <ProjectReference Include="..\ThemeForge\src\ThemeForge.Theme\ThemeForge.Theme.csproj" />
  <ProjectReference Include="..\ThemeForge\src\ThemeForge.Controls\ThemeForge.Controls.csproj" />
</ItemGroup>
```

`ThemeForge.Theme` contient le moteur :

- `IThemeService` ;
- `ThemeService` ;
- `ThemeNames` ;
- les 16 `ResourceDictionary` de thèmes.

`ThemeForge.Controls` contient :

- les styles WPF natifs dans `src/ThemeForge.Controls/Styles/` ;
- les composites dans `src/ThemeForge.Controls/Composites/` ;
- l'index WPF des composites dans `Themes/Generic.xaml`.

Si ton app n'a pas déjà de container DI, ajoute aussi :

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
```

Le Studio utilise ce package avec un `ServiceCollection` brut.

## 3. Bootstrap App.xaml

Dans `App.xaml`, merge le dictionnaire de styles natifs.

Tu peux aussi merger un thème par défaut.

`ThemeService.ApplyTheme(...)` remplacera ensuite le thème actif au runtime.

```xml
<Application
    x:Class="YourApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/ThemeForge.Controls;component/Styles/Studio.xaml"/>
                <ResourceDictionary Source="pack://application:,,,/ThemeForge.Theme;component/Themes/Drakul.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Le dictionnaire `Styles/Studio.xaml` agrège les styles natifs.

Les styles natifs sont implicites.

Tu écris un `<Button>`, un `<TextBox>` ou un `<DataGrid>` normal.

WPF applique le style ThemeForge si le dictionnaire est mergé.

## 4. Bootstrap App.xaml.cs (DI minimal)

`ThemeService` reçoit l'instance WPF `Application`.

Le pattern du Studio utilise donc une factory DI.

```csharp
private ServiceProvider? _services;

protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    var services = new ServiceCollection();
    services.AddSingleton<IThemeService>(_ => new ThemeService(this));
    _services = services.BuildServiceProvider();
    var theme = _services.GetRequiredService<IThemeService>();
    theme.ApplyTheme(ThemeNames.Drakul);
}
```

Ajoute les `using` nécessaires :

```csharp
using Microsoft.Extensions.DependencyInjection;
using ThemeForge.Theme;
```

Le singleton est important.

`IThemeService` porte un état partagé :

- `CurrentTheme` ;
- `ThemeRevision` ;
- l'événement `ThemeChanged`.

Si tu crées plusieurs instances, tes vues peuvent observer des états différents.

Garde donc une seule instance par application.

Tu peux choisir `ThemeNames.Dracula` si tu veux la palette canonique.

Tu peux choisir `ThemeNames.Drakul` si tu veux le sibling AA-compliant.

## 5. Utiliser les contrôles natifs stylés

Les styles natifs vivent dans `src/ThemeForge.Controls/Styles/`.

Le point d'entrée consommateur est :

```text
ThemeForge.Controls;component/Styles/Studio.xaml
```

Le README annonce 23 contrôles WPF natifs stylés.

Cette liste couvre les contrôles utilisés directement le plus souvent :

- `Button`
- `ToggleButton`
- `RepeatButton`
- `CheckBox`
- `RadioButton`
- `TextBox`
- `PasswordBox`
- `ComboBox`
- `ComboBoxItem`
- `ListBox`
- `ListBoxItem`
- `ListView`
- `TreeView`
- `TreeViewItem`
- `DataGrid`
- `TabControl`
- `TabItem`
- `GroupBox`
- `Expander`
- `Slider`
- `ProgressBar`
- `ScrollBar`
- `StatusBar`

Les templates couvrent aussi des éléments internes utiles :

- headers et cellules de `DataGrid` ;
- `GridViewColumnHeader` pour `ListView` ;
- `StatusBarItem` et `Thumb`.

Tu n'as rien à référencer par clé.

Écris du WPF normal :

```xml
<StackPanel>
    <TextBox Width="240" Text="Search"/>
    <Button Content="Apply" Margin="0,8,0,0"/>
    <ProgressBar Value="65" Height="14"/>
</StackPanel>
```

Note sur `ToolTip` : le popup lui-même n'a pas de style dédié ThemeForge.

Il reste sur le comportement WPF/Windows par défaut.

## 6. Utiliser les composites

Les composites vivent dans `src/ThemeForge.Controls/Composites/`.

Tu ajoutes le namespace XAML :

```xml
xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls"
```

Les 9 composites livrés :

- `Card` : conteneur avec header, body et footer optionnels.
- `IconButton` : bouton avec icône vectorielle et label optionnel.
- `Badge` : pastille de statut compacte.
- `Chip` : jeton sélectionnable, optionnellement supprimable.
- `ToggleSwitch` : switch basé sur `ToggleButton`.
- `Avatar` : initiales ou image dans un cercle.
- `SearchBox` : champ de recherche avec placeholder, clear et commande.
- `Toast` : notification éphémère avec titre, message et sévérité.
- `ToastHost` : pile verticale qui héberge et retire les toasts.

Exemple minimal :

```xml
<StackPanel xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls">
    <dfc:Card Header="Profil">
        <TextBlock Text="Compte synchronisé."/>
    </dfc:Card>
    <dfc:IconButton Label="Save" Margin="0,8,0,0"/>
    <dfc:Badge Content="Ready" Severity="Success"/>
</StackPanel>
```

Huit composites exposent un `AutomationPeer` custom.

Cela aide les screen readers et les tests UIA.

## 7. Utiliser les design tokens dans tes propres styles

Les tokens non-couleur communs sont centralisés dans :

`src/ThemeForge.Theme/Themes/Shared/DesignTokens.xaml`.

Chaque variante de thème merge ce fichier shared. Tu peux donc utiliser les
mêmes tokens de spacing, radius et taille de police quel que soit le thème
actif.

Les tokens partagés :

- `SpacingNone`, `SpacingXxs`, `SpacingXs`, `SpacingSm`, `SpacingMd`,
  `SpacingLg`, `SpacingXl`, `SpacingXxl`, `SpacingXxxl` (`Thickness`)
- `RadiusNone`, `RadiusXs`, `RadiusSm`, `RadiusMd`, `RadiusLg`, `RadiusXl`,
  `RadiusFull` (`CornerRadius`)
- `FontSizeXs`, `FontSizeSm`, `FontSizeMd`, `FontSizeLg`, `FontSizeXl`
  (`Double`)

Les couleurs restent déclarées dans chaque fichier de thème.

Les brushes canoniques :

- `BackgroundBrush`
- `CurrentLineBrush`
- `SelectionBrush`
- `ForegroundBrush`
- `CommentBrush`
- `CyanBrush`
- `GreenBrush`
- `OrangeBrush`
- `PinkBrush`
- `PurpleBrush`
- `RedBrush`
- `YellowBrush`

Les brushes sémantiques :

- `SurfaceBrush`
- `SurfaceAltBrush`
- `AccentBrush`
- `AccentHoverBrush`
- `AccentPressedBrush`
- `TextPrimaryBrush`
- `TextSecondaryBrush`
- `BorderBrush`
- `SuccessBrush`
- `WarningBrush`
- `ErrorBrush`
- `InfoBrush`

Utilise `DynamicResource` dans tes vues et tes styles.

`StaticResource` résout la valeur une fois.

`DynamicResource` suit le remplacement du `ResourceDictionary` au runtime.

Exemple de style local :

```xml
<Style x:Key="PanelTitle" TargetType="{x:Type TextBlock}">
    <Setter Property="Foreground" Value="{DynamicResource AccentBrush}"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="FontSize" Value="{DynamicResource FontSizeLg}"/>
    <Setter Property="Margin" Value="{DynamicResource SpacingMd}"/>
</Style>
```

Tu peux aussi consommer les couleurs avec le suffixe `Color`.

Exemples : `AccentColor`, `SurfaceColor`, `TextPrimaryColor`.

## 8. Basculer de thème au runtime

L'API publique est courte.

`IThemeService.ApplyTheme(string name)` applique un thème connu.

`AvailableThemes` donne la liste stable des noms applicables.

Exemple ViewModel avec `CommunityToolkit.Mvvm` :

```csharp
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    public IReadOnlyList<string> Themes => _themeService.AvailableThemes;

    public ShellViewModel(IThemeService themeService)
        => _themeService = themeService;

    [ObservableProperty]
    private string? _selectedTheme;
}
```

Ajoute la réaction au changement :

```csharp
partial void OnSelectedThemeChanged(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return;
    }

    _themeService.ApplyTheme(value);
}
```

Bind une `ComboBox` :

```xml
<ComboBox ItemsSource="{Binding Themes}"
          SelectedItem="{Binding SelectedTheme, Mode=TwoWay}"/>
```

`ApplyTheme` est idempotent.

Si `value` est déjà le thème courant, le service ne refait rien.

Pour observer les changements :

```csharp
_themeService.ThemeChanged += (_, e) =>
{
    CurrentThemeName = e.CurrentTheme;
    CurrentThemeRevision = e.Revision;
};
```

`ThemeRevision` est incrémenté avant `ThemeChanged`.

C'est utile pour forcer des converters, multi-bindings ou caches visuels à se
ré-évaluer.

Tu peux exposer `ThemeRevision` dans ton ViewModel si ta vue en dépend.

## 9. Aller plus loin

Studio est le bac à sable du repo.

Lance-le depuis la racine ThemeForge :

```bash
dotnet run --project src/ThemeForge.Studio
```

Tu peux y vérifier :

- les 16 variantes ;
- les styles natifs ;
- les composites ;
- l'éditeur live des 24 slots hex.

Pour créer ta propre variante :

- crée un fichier `src/ThemeForge.Theme/Themes/<Name>.xaml` ;
- garde un nom PascalCase, ASCII, sans espace ;
- ajoute le header d'attribution obligatoire ;
- déclare les mêmes clés `Color` et `Brush` que les thèmes existants ;
- ajoute le nom dans `ThemeNames.cs`.

Le header d'attribution est obligatoire.

Voir le fichier NOTICE pour la règle d'attribution.

`NOTICE` reste la source canonique pour l'attribution des palettes.

Pour que ta variante apparaisse dans `AvailableThemes`, ajoute-la à
`ThemeNames.All`.

Autre option avancée : injecte ta propre liste au constructeur :

```csharp
var names = ThemeNames.All.Append("MyTheme").ToArray();
services.AddSingleton<IThemeService>(_ => new ThemeService(this, names));
```

Reviens ensuite au [README](../README.md) pour la vue d'ensemble.

Consulte aussi [NOTICE](../NOTICE) pour les attributions.

## 10. Limitations connues

ThemeForge cible WPF desktop.

Il ne cible pas :

- WinForms ;
- UWP ;
- WinUI 3 ;
- Avalonia ;
- MAUI.

Il n'y a pas encore de package NuGet publié : utilise `ProjectReference`.

Le designer Visual Studio ne prévisualise pas fidèlement la bascule runtime.

C'est une limite classique WPF.

Le designer travaille en design-time ; `ThemeService` agit en runtime sur
`Application.Resources.MergedDictionaries`.

Si un style semble absent dans le designer, lance l'app.

Vérifie le rendu réel au runtime.

Liens utiles :

- [racine du repo](../)
- [README](../README.md)
- [NOTICE](../NOTICE)

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

ThemeForge est publié sur GitHub Packages (compte VBlackJack).

L'intégration recommandée se fait par `PackageReference`. La section 2 détaille le
feed et l'authentification.

## 2. Référencer ThemeForge dans ton projet

ThemeForge est distribué sur GitHub Packages. L'intégration se fait par
`PackageReference`.

### 2.1 Déclarer le feed

Ajoute un `nuget.config` à côté de ta solution :

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/VBlackJack/index.json" />
  </packageSources>
</configuration>
```

### 2.2 S'authentifier (obligatoire, même en lecture)

GitHub Packages n'autorise aucun restore anonyme : il faut un PAT GitHub avec le
scope `read:packages`, même pour une simple consommation.

Fournis-le de l'une de ces deux façons :

- par variable d'environnement, sans écrire le token sur disque (lance ensuite
  `dotnet restore` dans le même shell) :

  ```pwsh
  $env:NuGetPackageSourceCredentials_github = "Username=<user>;Password=<PAT>"
  ```

- ou dans le `nuget.config`, via un bloc `packageSourceCredentials` (garde alors
  le fichier hors du contrôle de version) :

  ```xml
  <packageSourceCredentials>
    <github>
      <add key="Username" value="<user>" />
      <add key="ClearTextPassword" value="<PAT>" />
    </github>
  </packageSourceCredentials>
  ```

### 2.3 Référencer les packages

```xml
<ItemGroup>
  <PackageReference Include="ThemeForge.Theme" Version="1.2.0" />
  <PackageReference Include="ThemeForge.Controls" Version="1.2.0" />
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

Dans `App.xaml`, merge UNIQUEMENT le dictionnaire de styles natifs. Le thème
initial se pose au runtime via `ThemeService.ApplyTheme(...)` dans `OnStartup`
(section 4).

```xml
<Application
    x:Class="YourApp.App"
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

> **Avertissement.** Ne jamais merger un thème en statique dans `App.xaml` si tu
> utilises `ApplyTheme`. Un thème mergé en statique n'est pas marqué par
> `ThemeService`, donc jamais retiré, et il reste en dernière position du merge :
> sur une clé dupliquée, WPF donne la priorité au dernier dictionnaire. Ses
> brushes l'emportent alors sur le thème appliqué au runtime, et le switch de
> thème change `CurrentTheme` sans aucun effet visible sur les couleurs. Le thème
> initial se pose donc exclusivement via `ThemeService.ApplyTheme(...)`.

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

Note sur `ToolTip` : il est aussi thémé par ThemeForge.

Le style implicite vit dans `Styles/ToolTip.xaml`.

`Styles/Studio.xaml` le merge déjà avec les autres styles natifs.

Chaque `ToolTip` attaché à un contrôle récupère donc le style automatiquement.

Il reprend la surface, la bordure, le texte, la typo et le rayon du thème actif.

## 6. Utiliser les composites

Les composites vivent dans `src/ThemeForge.Controls/Composites/`.

Tu ajoutes le namespace XAML :

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
- `Breadcrumb` : fil d'Ariane cliquable pour la navigation hiérarchique.
- `Dialog` : surface de dialogue avec header, contenu, footer et accent sémantique.
- `NumericUpDown` : saisie numérique avec boutons d'incrémentation.
- `SegmentedControl` : groupe de segments à sélection unique.

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

Le dossier `Composites` contient désormais 14 `AutomationPeer` custom.

Ces peers couvrent les contrôles composites qui ont une sémantique UIA dédiée.

Ils couvrent aussi les conteneurs d'items `BreadcrumbItem` et `SegmentItem`.

Cela aide les screen readers et les tests UIA.

Si `Content` ou `Header` reçoit autre chose qu'une chaîne, le peer ne fabrique pas de nom accessible.

Il ignore cet objet pour éviter d'annoncer un nom de type .NET.

Une icône, une shape ou un élément WPF ne devient donc pas un nom accessible automatique.

Dans ce cas, définis `AutomationProperties.Name` sur le composite.

```xml
<dfc:Chip AutomationProperties.Name="Filtre actif">
    <Rectangle Width="12" Height="12"/>
</dfc:Chip>
```

### Breadcrumb

`Breadcrumb` hérite de `ItemsControl`.

Il peut recevoir des `BreadcrumbItem` en XAML ou une collection via `ItemsSource`.

`BreadcrumbItem` hérite de `ButtonBase`.

Il expose donc `Click`, `Command` et `CommandParameter`.

Propriété dédiée :

- `IsCurrent` (`bool`) : marque le segment actif.

Exemple minimal :

```xml
<dfc:Breadcrumb>
    <dfc:BreadcrumbItem Content="Accueil" Command="{Binding GoHomeCommand}"/>
    <dfc:BreadcrumbItem Content="Projet" Command="{Binding GoProjectCommand}"/>
    <dfc:BreadcrumbItem Content="Détails" IsCurrent="True"/>
</dfc:Breadcrumb>
```

### Dialog

`Dialog` hérite de `HeaderedContentControl`.

`Header` porte le titre.

`Content` porte le corps principal.

Propriétés dédiées :

- `Footer` (`object?`) : affiche un contenu d'action ou d'état en bas.
- `FooterTemplate` (`DataTemplate?`) : template le contenu de `Footer`.
- `Severity` (`DialogSeverity`) : choisit l'accent sémantique.
- `IconGeometry` (`Geometry?`) : affiche une icône avant le header.
- `IsClosable` (`bool`) : affiche ou masque le bouton de fermeture.
- `CloseCommand` (`ICommand?`) : commande exécutée par `Close()` quand elle peut s'exécuter.

Événement dédié :

- `Closed` (`EventHandler?`) : levé une seule fois quand le dialogue se ferme.

Méthode utile :

- `Close()` : ferme le dialogue, exécute `CloseCommand`, puis lève `Closed`.

Exemple minimal :

```xml
<dfc:Dialog Header="Supprimer le projet"
            Severity="Warning"
            CloseCommand="{Binding CloseDialogCommand}">
    <dfc:Dialog.Content>
        <TextBlock Text="Cette action est irréversible."/>
    </dfc:Dialog.Content>
    <dfc:Dialog.Footer>
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="Annuler" Command="{Binding CloseDialogCommand}"/>
            <Button Content="Supprimer" Command="{Binding DeleteProjectCommand}" Margin="8,0,0,0"/>
        </StackPanel>
    </dfc:Dialog.Footer>
</dfc:Dialog>
```

### NumericUpDown

`NumericUpDown` hérite de `RangeBase`.

Il reprend `Value`, `Minimum`, `Maximum`, `SmallChange` et `LargeChange`.

Propriétés dédiées :

- `DecimalPlaces` (`int`) : définit le nombre de décimales affichées, de `0` à `15`.
- `IsReadOnly` (`bool`) : empêche la modification par saisie, molette et boutons.

Méthodes utiles :

- `IncreaseValue()` : ajoute `SmallChange` à `Value`.
- `DecreaseValue()` : retire `SmallChange` à `Value`.

Exemple minimal :

```xml
<dfc:NumericUpDown Minimum="0"
                   Maximum="100"
                   Value="{Binding OpacityPercent, Mode=TwoWay}"
                   SmallChange="1"
                   LargeChange="10"
                   DecimalPlaces="0"/>
```

### SegmentedControl

`SegmentedControl` hérite de `ListBox`.

Il force `SelectionMode` à `Single`.

`SegmentItem` hérite de `ListBoxItem`.

Propriétés utiles héritées :

- `ItemsSource` (`IEnumerable`) : fournit les segments depuis une collection.
- `SelectedItem` (`object?`) : porte l'élément sélectionné.
- `SelectedIndex` (`int`) : porte l'index sélectionné.

Exemple minimal :

```xml
<dfc:SegmentedControl SelectedIndex="{Binding SelectedViewIndex, Mode=TwoWay}">
    <dfc:SegmentItem Content="Aperçu"/>
    <dfc:SegmentItem Content="Détails"/>
    <dfc:SegmentItem Content="Historique"/>
</dfc:SegmentedControl>
```

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

## 9. Suivre le thème et l'accent Windows

Depuis la v1.1.0 (suivi clair/sombre) et la v1.2.0 (suivi de l'accent),
ThemeForge peut s'aligner automatiquement sur les réglages Windows.

Ces capacités sont opt-in et vivent sur des interfaces SÉPARÉES,
`ISystemThemeFollower` et `ISystemAccentFollower`, pour ne pas modifier
`IThemeService` (gelé pour la stabilité SemVer). `ThemeService` les implémente
toutes les deux ; tu y accèdes par un cast de ton instance.

### Suivre le mode clair/sombre de Windows

```csharp
((ISystemThemeFollower)themeService).EnableSystemFollow(ThemeNames.Folio, ThemeNames.Drakul);
// ...
((ISystemThemeFollower)themeService).DisableSystemFollow();
```

`EnableSystemFollow(lightTheme, darkTheme)` applique `lightTheme` quand Windows
est en mode clair et `darkTheme` en mode sombre, puis suit les changements de
mode. `IsFollowingSystem` expose l'état courant.

### Suivre la couleur d'accent de Windows

```csharp
((ISystemAccentFollower)themeService).EnableSystemAccentFollow();
// ...
((ISystemAccentFollower)themeService).DisableSystemAccentFollow();
```

`IsFollowingSystemAccent` expose l'état courant.

### Interactions à connaître

- Le suivi d'accent et `ApplyAccentTint(...)` sont mutuellement exclusifs :
  appeler `ApplyAccentTint` désactive automatiquement le suivi d'accent.
- Un `ApplyTheme(...)` manuel désactive le suivi clair/sombre (tu reprends la
  main sur le thème), mais conserve le suivi d'accent s'il est actif.

> **Note ergonomie.** Le cast vers `ISystemThemeFollower` /
> `ISystemAccentFollower` est volontaire : ces interfaces sont additives et
> non-breaking. Une façade ou une extension DI exposant les trois interfaces sur
> le même singleton est envisagée pour une version ultérieure.

## 10. Aller plus loin

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

## 11. Limitations connues

ThemeForge cible WPF desktop.

Il ne cible pas :

- WinForms ;
- UWP ;
- WinUI 3 ;
- Avalonia ;
- MAUI.

La consommation passe par GitHub Packages et exige une authentification, même en
lecture seule (voir section 2).

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

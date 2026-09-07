# Guide d'intégration ThemeForge

[English](../integration-guide.md) | [Introduction](integration-guide.md) | [Initialisation](bootstrap.md) | [Contrôles](controls.md) | [Thèmes personnalisés](custom-themes.md)

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
  <PackageReference Include="ThemeForge.Theme" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Controls" Version="2.2.0" />
  <PackageReference Include="ThemeForge.Theme.DependencyInjection" Version="2.2.0" />
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


# Guide d’intégration : partie 4

[English](../custom-themes.md) | [Introduction](integration-guide.md) | [Initialisation](bootstrap.md) | [Contrôles](controls.md) | [Thèmes personnalisés](custom-themes.md)

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

## 13. Palettes externes

Créez et exportez une palette dans Studio, puis chargez son JSON sans recompiler
ThemeForge. Consultez le [guide des palettes externes](external-palettes.md) pour
le format, la validation, le chargement et la persistance.

Ajouter une variante intégrée au framework reste possible : créez son dictionnaire
dans `src/ThemeForge.Theme/Themes/`, conservez son attribution et ajoutez son nom
dans `ThemeNames`. Cette procédure sert aux contributions au catalogue intégré.

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

- [README](../../README.fr.md)
- [NOTICE](../../NOTICE)

## Durée de vie des modifications Studio

Studio conserve un document de palette avec historique Annuler/Rétablir et indicateur
de modifications. Un changement manuel de thème ou d’accent demande confirmation avant
abandon du brouillon. Un changement externe, notamment Windows, conserve le brouillon.
Fermer la fenêtre, ouvrir un autre fichier ou créer une nouvelle palette protège aussi
les modifications. Réinitialiser restaure le document d’origine et reste annulable.
Les ressources sans rapport avec l’éditeur sont conservées.

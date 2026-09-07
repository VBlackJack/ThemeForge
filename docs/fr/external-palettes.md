# Palettes externes et Studio

[English](../external-palettes.md) | Français

Ces API nécessitent ThemeForge **2.2.0** ou ultérieur. Alignez les versions des packages ThemeForge.

## Créer une palette réutilisable

1. Lancez Studio, sélectionnez le thème de départ, puis ouvrez **Edit**.
2. Choisissez un identifiant unique : majuscule ASCII, puis lettres ou chiffres.
   Un nom de thème intégré ne peut pas être remplacé.
3. Modifiez les couleurs opaques `#RRGGBB`. Une saisie invalide est signalée et bloque l’enregistrement.
4. Comparez les échantillons et panneaux avant/après, puis les diagnostics de contraste.
5. Utilisez **Enregistrer** (Ctrl+S) ou **Exporter** pour écrire le document JSON.
6. Fermez-le puis utilisez **Ouvrir** (Ctrl+O). Métadonnées, attribution et couleurs sont conservées.

**Annuler/Rétablir** conserve les 100 dernières modifications valides du document.
La réinitialisation est annulable. Les diagnostics mesurent les paires texte/fond brutes :
ils ne certifient pas toute l’application et ne remplacent pas la recette clavier et lecteur
d’écran. Le modèle Button peut adapter sa couleur de texte indépendamment.

Studio conserve l’attribution importée. Un document dérivé d’un thème intégré identifie
sa source et renvoie à [NOTICE](../../NOTICE). Vérifiez droits et attribution avant
distribution. Aucun XAML exécutable n’est importé.

## Charger dans une application

Chargez les données de façon asynchrone avant d’enregistrer le bootstrap :

```csharp
using ThemeForge.Theme.DependencyInjection;
using ThemeForge.Theme.Palettes;

ThemePalette palette = await PaletteJson.LoadAsync(palettePath, cancellationToken);
services.AddThemeForge(application, options =>
{
    options.Palettes = new[] { palette };
    options.DefaultTheme = palette.Name;
    options.ApplicationName = applicationName;
});
```

Appelez ensuite `UseThemeForge()` sur le fournisseur de services. Enregistrez la palette
externe à chaque démarrage, avant restauration des préférences. Si le nom enregistré
n’est plus disponible, le bootstrap utilise le thème par défaut configuré.

Enregistrement explicite sans bootstrap :

```csharp
ThemeService service = new ThemeService(application);
service.RegisterPalette(palette);
service.ApplyTheme(palette.Name);
```

`IExternalThemeCatalog` expose l’enregistrement par injection de dépendances. Enregistrez
avant de lier un sélecteur à la liste des noms disponibles. Le moteur copie les données
validées. Remplacer un nom intégré ou actif est refusé ; changez de thème avant de remplacer
une palette externe. Appliquez les ressources sur le dispatcher WPF.

## Format et comportement en erreur

- [Schéma JSON](../palette.schema.json), [exemple complet](../../samples/palettes/SampleFolio.json).
- La version 1 exige `version`, `name`, `displayName`, `family`, `attribution`, `colors`.
- Exactement 25 slots : 12 canoniques, 12 sémantiques et Blue. Chacun produit une couleur
  et une brosse figée ; les tokens d’espacement, police et rayon restent ceux de ThemeForge.
- Couleurs acceptées : `#RRGGBB` ou `#FFRRGGBB` opaque, normalisées en `#RRGGBB`.
- Maximum : 64 Kio, profondeur JSON 4. Propriétés inconnues ou dupliquées, slots absents,
  versions/familles inconnues, transparence et identifiants invalides sont refusés.
- La validation du moteur refuse aussi les titres et attributions composés d’espaces.
- Un chargement invalide conserve le document courant. L’enregistrement valide avant écriture,
  utilise un fichier temporaire voisin, puis remplace la destination. Une annulation avant
  remplacement préserve l’ancienne destination. Les erreurs remontent à l’hôte.

Studio signale les erreurs dans sa zone d’état et dans des journaux quotidiens asynchrones
sous `%LocalAppData%/ThemeForge/Studio/logs`. `StudioLogOptions` configure emplacement,
niveau et activation. Une fermeture normale termine les écritures en attente.

[Démarrage](bootstrap.md) | [Contrôles](controls.md) | [Vérifications qualité](quality.md)

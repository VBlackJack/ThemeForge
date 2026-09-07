# Guide d’intégration : partie 3

[English](../controls.md) | [Introduction](integration-guide.md) | [Initialisation](bootstrap.md) | [Contrôles](controls.md) | [Thèmes personnalisés](custom-themes.md)

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

## Accessibilité et animations

Le texte des boutons conserve la couleur demandée si son contraste atteint 4,5:1 face
au fond uni réel. Sinon, le modèle choisit un texte sombre ou clair. Cela couvre les
palettes et accents fournis ; un contenu personnalisé imposant sa propre couleur reste
sous la responsabilité de l’application hôte.

Les notifications persistantes (`Duration="0"`) ont un bouton de fermeture accessible
au clavier avec une bordure de focus visible. Son nom accessible par défaut est
"Close notification". Redéfinissez la ressource chaîne `ToastCloseButtonName` dans les
ressources de l’application pour la traduire ; Studio utilise le français.

ToggleSwitch, Toast et ProgressBar indéterminée respectent la préférence Windows
d’animation de la zone cliente. L’hôte peut aussi désactiver les animations d’un sous-arbre :

```xml
<StackPanel xmlns:theming="clr-namespace:ThemeForge.Controls.Theming;assembly=ThemeForge.Controls"
            theming:Motion.ReduceMotion="True">
    <ProgressBar IsIndeterminate="True"/>
</StackPanel>
```

Ce mode affiche un segment de progression statique, des notifications entièrement
visibles et des positions de commutateur immédiates. Un changement de préférence
arrête les animations actives des modèles.

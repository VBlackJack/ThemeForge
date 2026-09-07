# Controls and design tokens

English | [Français](fr/controls.md)

[Introduction](integration-guide.md) | [Startup](bootstrap.md) | [Custom themes](custom-themes.md)

## 9. Styled native controls

Merge `ThemeForge.Controls;component/Styles/Studio.xaml`. Styles are implicit and need no explicit style key.

The 23 styled native controls include:

- `Button`, `ToggleButton`, `RepeatButton`, `CheckBox`, `RadioButton`.
- `TextBox`, `PasswordBox`, `ComboBox`, `ComboBoxItem`.
- `ListBox`, `ListBoxItem`, `ListView`, `TreeView`, `TreeViewItem`, `DataGrid`.
- `TabControl`, `TabItem`, `GroupBox`, `Expander`.
- `Slider`, `ProgressBar`, `ScrollBar`, `StatusBar`.

Templates also cover internal elements such as DataGrid headers and cells, `GridViewColumnHeader`, `StatusBarItem`, `Thumb`, and `ToolTip`.

```xml
<StackPanel>
    <TextBox Width="240" Text="Search"/>
    <Button Content="Apply" Margin="0,8,0,0"/>
    <ProgressBar Value="65" Height="14"/>
</StackPanel>
```

## 10. Composite controls

Add the namespace:

```xml
xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls"
```

| Control | Purpose |
|---|---|
| `Card` | Container with optional header, body, and footer. |
| `IconButton` | Vector icon button with an optional label. |
| `Badge` | Compact status indicator. |
| `Chip` | Selectable token with optional removal. |
| `ToggleSwitch` | Switch based on `ToggleButton`. |
| `Avatar` | Initials or an image in a circle. |
| `SearchBox` | Search field with placeholder, clear action, and command. |
| `Toast` | Temporary notification with title, message, and severity. |
| `ToastHost` | Vertical host that manages toast removal. |
| `Breadcrumb` | Clickable navigation trail. |
| `Dialog` | Dialog surface with header, content, footer, and accent. |
| `NumericUpDown` | Numeric input with increment/decrement buttons. |
| `SegmentedControl` | Single-selection segment group. |

```xml
<StackPanel xmlns:dfc="clr-namespace:ThemeForge.Controls.Composites;assembly=ThemeForge.Controls">
    <dfc:Card Header="Profile">
        <TextBlock Text="Account synchronized."/>
    </dfc:Card>
    <dfc:IconButton Label="Save" Margin="0,8,0,0"/>
    <dfc:Badge Content="Ready" Severity="Success"/>
</StackPanel>
```

Fourteen custom automation peers cover composites and item containers such as `BreadcrumbItem` and `SegmentItem`. A non-string `Content` or `Header` does not automatically become an accessible name, avoiding .NET type names in screen reader output. Set `AutomationProperties.Name` explicitly:

```xml
<dfc:Chip AutomationProperties.Name="Active filter">
    <Rectangle Width="12" Height="12"/>
</dfc:Chip>
```

## 11. Design tokens

Non-colour tokens are centralized in `src/ThemeForge.Theme/Themes/Shared/DesignTokens.xaml`, merged by each variant:

- `SpacingNone` through `SpacingXxxl`: `Thickness`.
- `RadiusNone` through `RadiusFull`: `CornerRadius`.
- `FontSizeXs` through `FontSizeXl`: `Double`.

Each theme defines canonical brushes (`BackgroundBrush`, `ForegroundBrush`, `CommentBrush`, `CyanBrush`) and semantic brushes (`SurfaceBrush`, `AccentBrush`, `TextPrimaryBrush`, `BorderBrush`, `SuccessBrush`). Use `DynamicResource` to follow runtime changes:

```xml
<Style x:Key="PanelTitle" TargetType="{x:Type TextBlock}">
    <Setter Property="Foreground" Value="{DynamicResource AccentBrush}"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="FontSize" Value="{DynamicResource FontSizeLg}"/>
    <Setter Property="Margin" Value="{DynamicResource SpacingMd}"/>
</Style>
```

Colours are also exposed with a `Color` suffix, for example `AccentColor` and `SurfaceColor`.

## Accessibility and motion

Button text preserves the requested foreground when it has a contrast ratio of at least
4.5:1 against the actual solid background. Otherwise the template selects a dark or light
foreground. This covers the bundled palette and accent states; custom content with its
own foreground remains the host's responsibility.

Persistent notifications (`Duration="0"`) have a focusable close button with a visible
focus border. Its default accessible name is "Close notification". Override the string
resource `ToastCloseButtonName` in application resources to localize it; Studio uses French.

ToggleSwitch, Toast and indeterminate ProgressBar honor the live Windows client-area
animation preference. A host can additionally disable motion for a subtree:

```xml
<StackPanel xmlns:theming="clr-namespace:ThemeForge.Controls.Theming;assembly=ThemeForge.Controls"
            theming:Motion.ReduceMotion="True">
    <ProgressBar IsIndeterminate="True"/>
</StackPanel>
```

Reduced motion shows a static progress segment, fully visible notifications and immediate
switch positions. Changing the preference stops the active template storyboards.

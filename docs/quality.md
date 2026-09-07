# Consumer, visual and performance verification

English | [Français](fr/quality.md)

## Shared gates

PRs, `main` and tag publication call the same command on Windows with PowerShell 7:

```pwsh
./tools/ci-verify.ps1 -OutputDirectory artifacts/check-001
```

Use a new output directory each time. The pipeline validates headers, explicit C# types,
200-line C#/XAML source limits, Release build and sequential unit tests. It then packs
all four packages, checks their exact attribution files, runs the Studio document round
trip, verifies a generated consumer and enforces performance budgets. Any failure stops
publication. This command never uploads packages.

The consumer fixture installs `tf-wpf` in an isolated template hive, uses a fresh NuGet
cache and maps `ThemeForge.*` exclusively to the local packages. Package metadata is
checked after restore. Only fixture configuration is changed: the palette and preference
paths point into its output directory and the window is hidden off-screen. The real
template startup runs twice, checking initial window creation, theme switching, external
colours and persisted restoration. No user template installation or preferences are changed.

NuGet source mapping does not recheck already cached packages, which is why this fixture
uses a new cache. [Microsoft documentation](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)

## Visual and accessibility matrix

After building:

```pwsh
dotnet tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll visual tools/quality-settings.json artifacts/visual
```

The configured matrix covers Dracula, Folio and Magellan at 100%, 125%, 150% and 200%
WPF layout scale. It renders long text, default and disabled buttons, error feedback,
numeric input, switch, progress, notification, DataGrid and the Studio editor. PNGs
capture the gallery, editor top and editor diagnostics. Reports record automation
patterns/names and programmatic focus acceptance. PR CI uploads this evidence.

Layout scale is not a physical monitor DPI transition. PNGs are review artifacts, not
a claim of pixel equality across Windows/font versions. Complete the manual matrix:

| Check | Expected result |
|---|---|
| Tab / Shift+Tab through every enabled editor action | Predictable order, visible focus, no trap. |
| Enter / Space on actions, Ctrl+S/O/Z/Y | Action works and disabled actions remain unavailable. |
| Narrator on fields, colour errors, dirty state and notifications | Meaningful names, values, order and announcements. |
| Monitor DPI changes and smallest supported window | No missing content; scrolling and wrapping remain usable. |
| Windows reduced-motion preference | Static readable states; active storyboards stop. |

Record pass/fail and OS/DPI for each theme/scale. Automated peer tests do not replace
this physical keyboard and Narrator pass.

## Performance workload and budgets

```pwsh
dotnet tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll performance tools/quality-settings.json artifacts/performance
```

The workload measures first-gallery render, 200 theme switches, average allocation per
switch and retained managed memory after 25 editor-window open/close cycles. Built-in
themes and one editor are warmed before retained-memory measurement. Closed windows
and editor view models must all become unreachable. Private bytes are reported separately.

Initial local .NET 10 x64 measurements were around 0.8 s for the first gallery, 0.5 ms
for p95 switching and 80 KB allocated per switch. WPF retained roughly 8.5 MB after the
window cycle workload while all tracked windows/editors were collected. This includes
framework caches; it is not a native-memory leak proof.

Budgets in `tools/quality-settings.json` provide headroom: first gallery 5 s, switch p95
10 ms, 160,000 bytes per switch and 16 MiB retained managed memory. These are regression
guards calibrated from this workload, not universal performance guarantees. Run alone
on a comparable machine. To collect a candidate baseline without passing the guard,
append `record-only`; normal CI never supplies that argument.

The report states every measured value and whether budgets pass. A cold operating-system
process launch, native leak analysis and a full application workload require separate profiling.

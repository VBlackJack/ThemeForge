# Vérification des consommateurs, du rendu et des performances

[English](../quality.md) | Français

## Contrôles communs

Les PR, `main` et la publication des tags exécutent la même commande sous Windows et PowerShell 7 :

```pwsh
./tools/ci-verify.ps1 -OutputDirectory artifacts/check-001
```

Utilisez un nouveau répertoire de sortie à chaque exécution. La chaîne contrôle les en-têtes,
les types C# explicites, la limite de 200 lignes C#/XAML, la compilation Release et les tests
séquentiels. Elle crée les quatre packages, vérifie leurs attributions exactes, exécute
l’aller-retour Studio, teste un consommateur généré et impose les budgets de performance.
Tout échec bloque la publication. Cette commande n’envoie aucun package.

Le test consommateur installe `tf-wpf` dans une configuration de modèles isolée, utilise
un cache NuGet vierge et réserve `ThemeForge.*` aux packages locaux. Les métadonnées
de provenance sont vérifiées. Seule la configuration du test change : chemins de palette
et préférences dans sa sortie, fenêtre hors écran. Le véritable démarrage du modèle tourne
deux fois : création de fenêtre, changement de thème, couleurs externes et restauration.
L’installation de modèles et les préférences de l’utilisateur restent intactes.

NuGet ne revérifie pas la source d’un package déjà en cache, d’où ce cache vierge.
[Documentation Microsoft](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)

## Matrice visuelle et accessibilité

Après compilation :

```pwsh
dotnet tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll visual tools/quality-settings.json artifacts/visual
```

La matrice couvre Dracula, Folio et Magellan aux échelles de mise en page WPF 100, 125,
150 et 200 %. Elle affiche textes longs, boutons par défaut et désactivés, erreur de saisie,
champ numérique, commutateur, progression, notification, DataGrid et éditeur Studio.
Les PNG montrent la galerie, le début et les diagnostics de l’éditeur. Les rapports
consignent les noms/patterns d’automatisation et l’acceptation du focus programmatique.
La CI des PR joint ces preuves.

L’échelle de mise en page ne simule pas un changement réel de DPI entre moniteurs.
Les PNG servent à la revue ; ils ne promettent pas une égalité de pixels entre versions
de Windows/polices. Complétez la matrice manuelle :

| Vérification | Résultat attendu |
|---|---|
| Tab / Maj+Tab sur les actions actives | Ordre prévisible, focus visible, aucun piège. |
| Entrée / Espace, Ctrl+S/O/Z/Y | Action correcte ; commandes désactivées indisponibles. |
| Narrator sur champs, erreurs, état modifié et notifications | Noms, valeurs, ordre et annonces utiles. |
| Changement de DPI et fenêtre minimale | Contenu accessible, défilement et retours à la ligne utilisables. |
| Préférence Windows de réduction des animations | États statiques lisibles et arrêt des animations actives. |

Consignez réussite/échec et OS/DPI par thème/échelle. Les tests de peers ne remplacent
pas cette recette au clavier physique et avec Narrator.

## Charge et budgets de performance

```pwsh
dotnet tests/ThemeForge.Quality/bin/Release/net10.0-windows/ThemeForge.Quality.dll performance tools/quality-settings.json artifacts/performance
```

La charge mesure le premier rendu de galerie, 200 changements de thème, l’allocation
moyenne par changement et la mémoire managée retenue après 25 ouvertures/fermetures
de fenêtres d’éditeur. Les thèmes et un éditeur sont préchargés avant mesure de rétention.
Toutes les fenêtres fermées et leurs modèles de vue doivent être collectés. Les octets
privés du processus sont rapportés séparément.

Les premières mesures locales .NET 10 x64 donnaient environ 0,8 s pour la galerie,
0,5 ms au percentile 95 des changements et 80 Ko alloués par changement. WPF retenait
environ 8,5 Mo après les cycles, avec collecte de tous les objets suivis. Ces chiffres
incluent des caches du framework et ne prouvent pas l’absence de fuite native.

Les budgets de `tools/quality-settings.json` gardent une marge : galerie 5 s, p95 10 ms,
160 000 octets par changement et 16 Mio de mémoire managée retenue. Ce sont des seuils
de régression calibrés sur cette charge, pas des garanties universelles. Exécutez seul
sur une machine comparable. Pour mesurer une référence candidate sans valider le seuil,
ajoutez `record-only` ; la CI normale ne le fait jamais.

Le rapport indique chaque valeur et le verdict. Démarrage à froid du processus,
analyse des fuites natives et charge d’une application complète demandent un profilage distinct.

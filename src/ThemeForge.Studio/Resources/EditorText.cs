// Copyright 2026 Julien Bombled
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Resources;

namespace ThemeForge.Studio.Resources;

/// <summary>French editor strings shared by bindings, dialogs and status messages.</summary>
public static class EditorText
{
    private static readonly ResourceManager Manager = new ResourceManager("ThemeForge.Studio.Resources.EditorText", typeof(EditorText).Assembly);
    /// <summary>Localized Title text.</summary>
    public static string Title => Manager.GetString(nameof(Title)) ?? nameof(Title);
    /// <summary>Localized Description text.</summary>
    public static string Description => Manager.GetString(nameof(Description)) ?? nameof(Description);
    /// <summary>Localized Open text.</summary>
    public static string Open => Manager.GetString(nameof(Open)) ?? nameof(Open);
    /// <summary>Localized Save text.</summary>
    public static string Save => Manager.GetString(nameof(Save)) ?? nameof(Save);
    /// <summary>Localized Export text.</summary>
    public static string Export => Manager.GetString(nameof(Export)) ?? nameof(Export);
    /// <summary>Localized Undo text.</summary>
    public static string Undo => Manager.GetString(nameof(Undo)) ?? nameof(Undo);
    /// <summary>Localized Redo text.</summary>
    public static string Redo => Manager.GetString(nameof(Redo)) ?? nameof(Redo);
    /// <summary>Localized Reset text.</summary>
    public static string Reset => Manager.GetString(nameof(Reset)) ?? nameof(Reset);
    /// <summary>Localized ResetAll text.</summary>
    public static string ResetAll => Manager.GetString(nameof(ResetAll)) ?? nameof(ResetAll);
    /// <summary>Localized New text.</summary>
    public static string New => Manager.GetString(nameof(New)) ?? nameof(New);
    /// <summary>Localized Name text.</summary>
    public static string Name => Manager.GetString(nameof(Name)) ?? nameof(Name);
    /// <summary>Localized NameHint text.</summary>
    public static string NameHint => Manager.GetString(nameof(NameHint)) ?? nameof(NameHint);
    /// <summary>Localized Modified text.</summary>
    public static string Modified => Manager.GetString(nameof(Modified)) ?? nameof(Modified);
    /// <summary>Localized Clean text.</summary>
    public static string Clean => Manager.GetString(nameof(Clean)) ?? nameof(Clean);
    /// <summary>Localized Before text.</summary>
    public static string Before => Manager.GetString(nameof(Before)) ?? nameof(Before);
    /// <summary>Localized After text.</summary>
    public static string After => Manager.GetString(nameof(After)) ?? nameof(After);
    /// <summary>Localized Original text.</summary>
    public static string Original => Manager.GetString(nameof(Original)) ?? nameof(Original);
    /// <summary>Localized Canonical text.</summary>
    public static string Canonical => Manager.GetString(nameof(Canonical)) ?? nameof(Canonical);
    /// <summary>Localized Semantic text.</summary>
    public static string Semantic => Manager.GetString(nameof(Semantic)) ?? nameof(Semantic);
    /// <summary>Localized Extended text.</summary>
    public static string Extended => Manager.GetString(nameof(Extended)) ?? nameof(Extended);
    /// <summary>Localized Attribution text.</summary>
    public static string Attribution => Manager.GetString(nameof(Attribution)) ?? nameof(Attribution);
    /// <summary>Localized Diagnostics text.</summary>
    public static string Diagnostics => Manager.GetString(nameof(Diagnostics)) ?? nameof(Diagnostics);
    /// <summary>Localized InvalidColor text.</summary>
    public static string InvalidColor => Manager.GetString(nameof(InvalidColor)) ?? nameof(InvalidColor);
    /// <summary>Localized InvalidDocument text.</summary>
    public static string InvalidDocument => Manager.GetString(nameof(InvalidDocument)) ?? nameof(InvalidDocument);
    /// <summary>Localized Saved text.</summary>
    public static string Saved => Manager.GetString(nameof(Saved)) ?? nameof(Saved);
    /// <summary>Localized Opened text.</summary>
    public static string Opened => Manager.GetString(nameof(Opened)) ?? nameof(Opened);
    /// <summary>Localized Failed text.</summary>
    public static string Failed => Manager.GetString(nameof(Failed)) ?? nameof(Failed);
    /// <summary>Localized Discard text.</summary>
    public static string Discard => Manager.GetString(nameof(Discard)) ?? nameof(Discard);
    /// <summary>Localized DialogTitle text.</summary>
    public static string DialogTitle => Manager.GetString(nameof(DialogTitle)) ?? nameof(DialogTitle);
    /// <summary>Localized Filter text.</summary>
    public static string Filter => Manager.GetString(nameof(Filter)) ?? nameof(Filter);
    /// <summary>Localized Kept text.</summary>
    public static string Kept => Manager.GetString(nameof(Kept)) ?? nameof(Kept);
    /// <summary>Localized HexHelp text.</summary>
    public static string HexHelp => Manager.GetString(nameof(HexHelp)) ?? nameof(HexHelp);
    /// <summary>Localized Sample text.</summary>
    public static string Sample => Manager.GetString(nameof(Sample)) ?? nameof(Sample);
    /// <summary>Localized DefaultAttribution text.</summary>
    public static string DefaultAttribution => Manager.GetString(nameof(DefaultAttribution)) ?? nameof(DefaultAttribution);
    /// <summary>Localized Busy text.</summary>
    public static string Busy => Manager.GetString(nameof(Busy)) ?? nameof(Busy);
    /// <summary>Localized ContrastPass text.</summary>
    public static string ContrastPass => Manager.GetString(nameof(ContrastPass)) ?? nameof(ContrastPass);
    /// <summary>Localized ContrastFail text.</summary>
    public static string ContrastFail => Manager.GetString(nameof(ContrastFail)) ?? nameof(ContrastFail);
}

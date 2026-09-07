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

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThemeForge.Theme.Palettes;

/// <summary>Bounded, strict JSON loading and atomic saves. Errors propagate to the host's error sink.</summary>
public static class PaletteJson
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, MaxDepth = 4,
    };

    /// <summary>Reads a palette without executing XAML or loading external resources.</summary>
    public static async Task<ThemePalette> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        byte[] bytes = new byte[PaletteValidation.MaximumFileBytes + 1];
        int count = await stream.ReadAtLeastAsync(bytes, bytes.Length, throwOnEndOfStream: false, cancellationToken);
        return Parse(bytes.AsMemory(0, count));
    }

    /// <summary>Validates length, duplicate properties and schema before returning a palette snapshot.</summary>
    public static ThemePalette Parse(ReadOnlyMemory<byte> json)
    {
        if (json.Length > PaletteValidation.MaximumFileBytes) { throw new ArgumentException("Palette file exceeds the size limit."); }
        using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4 });
        RejectDuplicateProperties(document.RootElement);
        ThemePalette palette = document.RootElement.Deserialize<ThemePalette>(Options)
            ?? throw new ArgumentException("Palette document cannot be null.");
        return PaletteValidation.Normalize(palette);
    }

    /// <summary>Produces normalized JSON; invalid data never reaches disk.</summary>
    public static byte[] Serialize(ThemePalette palette)
        => JsonSerializer.SerializeToUtf8Bytes(PaletteValidation.Normalize(palette), Options);

    /// <summary>Writes a sibling temporary file and replaces the destination only after a complete write.</summary>
    public static async Task SaveAsync(string path, ThemePalette palette, CancellationToken cancellationToken = default)
    {
        byte[] bytes = Serialize(palette);
        string destination = Path.GetFullPath(path);
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, destination, overwrite: true);
        }
        finally { if (File.Exists(temporary)) { File.Delete(temporary); } }
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) { return; }
        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!names.Add(property.Name)) { throw new ArgumentException($"Duplicate JSON property '{property.Name}'."); }
            RejectDuplicateProperties(property.Value);
        }
    }
}

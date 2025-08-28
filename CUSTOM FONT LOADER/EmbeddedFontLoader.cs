using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

public class EmbeddedFontLoader : IDisposable
{
    // Map font name -> font collection and family
    private readonly Dictionary<string, (PrivateFontCollection collection, FontFamily family)> _fonts
        = new Dictionary<string, (PrivateFontCollection, FontFamily)>();

    // Store temp files for cleanup
    private readonly List<string> _tempFiles = new List<string>();

    /// <summary>
    /// Loads multiple embedded fonts from resource names.
    /// </summary>
    public void LoadFontsFromResources(params string[] fontResourceNames)
    {
        foreach (var resourceName in fontResourceNames)
        {
            LoadFontFromResource(resourceName);
        }
    }

    private void LoadFontFromResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        using (var fontStream = assembly.GetManifestResourceStream(resourceName))
        {
            if (fontStream == null)
                throw new Exception($"Resource '{resourceName}' not found.");

            byte[] fontData = new byte[fontStream.Length];
            fontStream.Read(fontData, 0, fontData.Length);

            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ttf");
            File.WriteAllBytes(tempFile, fontData);
            _tempFiles.Add(tempFile);

            // Create a new PrivateFontCollection for each font
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile(tempFile);

            if (pfc.Families.Length == 0)
                throw new Exception($"No font families found in resource '{resourceName}'.");

            FontFamily family = pfc.Families[0];

            // Map by family name
            if (!_fonts.ContainsKey(family.Name))
            {
                _fonts.Add(family.Name, (pfc, family));
            }
            else
            {
                // If duplicate font family names, optionally throw or ignore
                throw new Exception($"Duplicate font family name '{family.Name}'.");
            }
        }
    }

    /// <summary>
    /// Get FontFamily by internal font family name.
    /// </summary>
    public FontFamily GetFontFamilyByName(string name)
    {
        if (_fonts.TryGetValue(name, out var tuple))
        {
            return tuple.family;
        }
        throw new Exception($"Font family '{name}' not found.");
    }

    /// <summary>
    /// Dispose all font collections and clean temp files.
    /// </summary>
    public void Dispose()
    {
        foreach (var tuple in _fonts.Values)
        {
            tuple.collection?.Dispose();
        }

        foreach (var file in _tempFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // Ignore
            }
        }

        _fonts.Clear();
        _tempFiles.Clear();
    }
}

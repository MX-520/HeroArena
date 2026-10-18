using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Apply audited source rectangles without changing scenes or animation assets.</summary>
public static class TestPlayerSheetSlicing
{
    private const string Source = "Assets/Role/Test/Player/TestPlayer.png";
    private const string Record = "ArtSource/Characters/TestPlayerSheet/";
    [Serializable] private sealed class Layout { public string source_sha256; public Frame[] frames; }
    [Serializable] private sealed class Frame { public string name; public int[] source_rect; public float pivotX, pivotY; }

    [MenuItem("Tools/HeroArena/Characters/Slice TestPlayer Sheet")]
    public static void Slice()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Slice the sheet in Edit Mode.");
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Record + "slicing-layout.json"));
        if (layout.frames.Length != 54 || layout.frames.Select(f => f.name).Distinct().Count() != 54)
            throw new InvalidDataException("Expected 54 uniquely named source poses.");
        using (var sha = SHA256.Create())
            if (!string.Equals(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Source))).Replace("-", ""), layout.source_sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Sheet changed since the rectangles were audited.");
        AssetDatabase.ImportAsset(Source, ImportAssetOptions.ForceSynchronousImport);
        string guid = AssetDatabase.AssetPathToGUID(Source);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Source);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        var slices = layout.frames.Select(f =>
        {
            var r = f.source_rect;
            if (r.Length != 4 || r[0] < 0 || r[1] < 0 || r[2] <= 0 || r[3] <= 0 || r[0]+r[2] > width || r[1]+r[3] > height)
                throw new InvalidDataException("Invalid rectangle: " + f.name);
            return new SpriteMetaData { name = f.name, rect = new Rect(r[0], height-r[1]-r[3], r[2], r[3]),
                alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(f.pivotX, f.pivotY) };
        }).ToArray();
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 2048;
        // Preserve existing PPU; ground anchors are provisional until action integration.
        foreach (string platform in new[] { "Standalone", "Android", "WebGL", "iPhone" })
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            if (!settings.overridden) continue;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.format = TextureImporterFormat.RGBA32;
            settings.maxTextureSize = 2048;
            importer.SetPlatformTextureSettings(settings);
        }
        importer.spritesheet = slices;
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Sprite>().ToArray();
        if (sprites.Length != slices.Length || AssetDatabase.AssetPathToGUID(Source) != guid)
            throw new InvalidDataException("Sprite count or texture GUID changed unexpectedly.");
        foreach (var expected in slices)
        {
            var actual = sprites.Single(s => s.name == expected.name);
            var pivot = Vector2.Scale(expected.pivot, expected.rect.size);
            if (actual.rect != expected.rect || (actual.pivot-pivot).sqrMagnitude > 0.001f)
                throw new InvalidDataException("Imported rectangle/pivot differs: " + expected.name);
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(actual, out string spriteGuid, out long id) || id == 0 || spriteGuid != guid)
                throw new InvalidDataException("Invalid Sprite reference: " + expected.name);
        }
        string result = "PASS: 54 Sprite subassets; rows 8/8/8/7/8/8/7; source SHA256, names, rectangles, pivots and local file references verified in Unity. Point, no compression/mipmaps. Texture GUID and PPU preserved. No scene, controller or animation modified. Attack-row touching artwork remains in source and overlapping slices may contain neighbour fragments.";
        File.WriteAllText(Record + "unity-import-report.txt", result);
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(Source);
        EditorGUIUtility.PingObject(Selection.activeObject);
        Debug.Log(result);
    }
}

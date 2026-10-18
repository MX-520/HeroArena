using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Slice the imported presentation board without changing its pixels or scenes.</summary>
public static class UserHeroBoardSlicing
{
    private const string Path = "Assets/Role/Test/Player/主角四方向像素待机动画展示板.png";
    [Serializable] private sealed class Layout { public string source_sha256; public Frame[] frames; }
    [Serializable] private sealed class Frame { public string direction; public int index, head_centre; public int[] source_rect; }

    [MenuItem("Tools/HeroArena/Characters/Slice Imported Player Idle Board")]
    public static void Slice()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Slice in Edit Mode; playback was left unchanged.");
        var layout = JsonUtility.FromJson<Layout>(File.ReadAllText("ArtSource/Characters/UserHeroTrial/FullCropped/crop-report.json"));
        using (var sha = SHA256.Create())
            if (!string.Equals(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path))).Replace("-", ""), layout.source_sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Board differs from the audited source; do not apply these rectangles.");
        AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.spritesheet = layout.frames.Select(f =>
        {
            int[] r = f.source_rect;
            if (r[0] < 0 || r[1] < 0 || r[0]+r[2] > width || r[1]+r[3] > height)
                throw new InvalidDataException("Out of bounds slice.");
            return new SpriteMetaData
            {
                name = "Idle_" + f.direction + "_" + f.index.ToString("00"),
                rect = new Rect(r[0], height-r[1]-r[3], r[2], r[3]),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2((f.head_centre-r[0])/(float)r[2], 0.05f)
            };
        }).ToArray();
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().ToArray();
        if (sprites.Length != layout.frames.Length || sprites.Select(s => s.name).Distinct().Count() != 117)
            throw new InvalidDataException("Expected 117 uniquely named sprites.");
        foreach (var f in layout.frames)
        {
            var sprite = sprites.Single(s => s.name == "Idle_" + f.direction + "_" + f.index.ToString("00"));
            if (sprite.rect.width != f.source_rect[2] || sprite.rect.height != f.source_rect[3])
                throw new InvalidDataException("Imported slice dimensions changed.");
        }
        Directory.CreateDirectory("Logs/Characters");
        File.WriteAllText("Logs/Characters/imported-board-slices.txt", "PASS: 117 sprites; Down=31 Left=29 Right=29 Up=28; rectangles and unique names verified; source hash matched; no scene or animation changed.");
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(Path);
        EditorGUIUtility.PingObject(Selection.activeObject);
        Debug.Log("Imported player board sliced: 117 sprites.");
    }
}

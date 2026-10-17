using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Imports reviewed body frames into a separate candidate folder. Never changes live actors.</summary>
public static class CharacterActionPreparation
{
    private const string LayoutPath = "ArtSource/Characters/Actions/import-layout.json";
    private const string OutputRoot = "Assets/Art/Characters/Generated";
    private const string ReportPath = "Logs/Characters/action-import-report.json";

    [Serializable] private sealed class Layout { public Sheet[] sheets; public Clip[] clips; }
    [Serializable] private sealed class Sheet
    {
        public string name, source;
        public float pixelsPerUnit;
        public Frame[] frames;
    }
    [Serializable] private sealed class Frame
    {
        public string name;
        public int x, y, width, height;
        public float pivotX, pivotY;
    }
    [Serializable] private sealed class Clip
    {
        public string actor, name, source, sheet;
        public string[] frames;
        public float[] phases;
    }
    [Serializable] private sealed class Report
    {
        public string status, error;
        public int sprites, clips;
        public bool liveActorsChanged;
        public List<string> checks = new List<string>();
    }

    [MenuItem("Tools/HeroArena/Characters/Prepare Candidate Actions")]
    public static void PrepareLive()
    {
        var report = new Report { status = "failed", liveActorsChanged = false };
        Directory.CreateDirectory("Logs/Characters");
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Prepare candidate actions in Edit Mode; Play Mode was left unchanged.");
            var layout = JsonUtility.FromJson<Layout>(File.ReadAllText(LayoutPath));
            if (layout == null || layout.sheets == null || layout.clips == null)
                throw new InvalidDataException("Missing sheet/clip layout.");
            EnsureFolder(OutputRoot);
            var sprites = new Dictionary<string, Sprite>();
            foreach (var sheet in layout.sheets)
                ImportSheet(sheet, sprites, report);
            foreach (var clip in layout.clips)
                CreateClip(clip, sprites, report);
            AssetDatabase.SaveAssets();
            report.status = "passed";
            report.checks.Add("Imported candidates only; no scene, prefab or controller was changed.");
            Debug.Log("Character actions prepared: " + report.sprites + " sprites, " + report.clips + " clips.");
        }
        catch (Exception exception)
        {
            report.error = exception.ToString();
            Debug.LogException(exception);
        }
        File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
    }

    private static void ImportSheet(Sheet sheet, Dictionary<string, Sprite> sprites, Report report)
    {
        if (sheet.frames == null || sheet.frames.Length == 0 || sheet.pixelsPerUnit <= 0)
            throw new InvalidDataException("Invalid frame layout: " + sheet.name);
        string destination = OutputRoot + "/" + sheet.name + ".png";
        byte[] source = File.ReadAllBytes(sheet.source);
        if (!File.Exists(destination) || !File.ReadAllBytes(destination).SequenceEqual(source))
            File.WriteAllBytes(destination, source);
        AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(destination);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = sheet.pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        importer.GetSourceTextureWidthAndHeight(out int textureWidth, out int textureHeight);
        importer.spritesheet = sheet.frames.Select(frame =>
        {
            if (frame.x < 0 || frame.y < 0 || frame.width <= 0 || frame.height <= 0 ||
                frame.x + frame.width > textureWidth || frame.y + frame.height > textureHeight)
                throw new InvalidDataException("Frame outside texture: " + frame.name);
            return new SpriteMetaData
            {
                name = frame.name,
                rect = new Rect(frame.x, textureHeight - frame.y - frame.height, frame.width, frame.height),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2(frame.pivotX, frame.pivotY)
            };
        }).ToArray();
        importer.SaveAndReimport();
        var loaded = AssetDatabase.LoadAllAssetsAtPath(destination).OfType<Sprite>().ToArray();
        if (loaded.Length != sheet.frames.Length)
            throw new InvalidDataException("Sprite count mismatch: " + sheet.name);
        foreach (var frame in sheet.frames)
        {
            var sprite = loaded.Single(value => value.name == frame.name);
            if (Mathf.Abs(sprite.pixelsPerUnit - sheet.pixelsPerUnit) > 0.01f)
                throw new InvalidDataException("PPU mismatch: " + frame.name);
            sprites.Add(sheet.name + "/" + frame.name, sprite);
        }
        report.sprites += loaded.Length;
        report.checks.Add(sheet.name + ": " + loaded.Length + " sprites; point filtering; explicit rects/pivots.");
    }

    private static void CreateClip(Clip spec, Dictionary<string, Sprite> sprites, Report report)
    {
        var original = AssetDatabase.LoadAssetAtPath<AnimationClip>(spec.source);
        if (original == null || spec.frames == null || spec.frames.Length == 0)
            throw new InvalidDataException("Missing original clip or replacement frames: " + spec.name);
        if (spec.phases == null || spec.phases.Length != spec.frames.Length || spec.phases[0] != 0)
            throw new InvalidDataException("Invalid frame timing: " + spec.name);
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(original);
        var binding = bindings.Single(value => value.type == typeof(SpriteRenderer) && value.propertyName == "m_Sprite");
        var keys = AnimationUtility.GetObjectReferenceCurve(original, binding);
        var clip = UnityEngine.Object.Instantiate(original);
        clip.name = spec.name;
        for (int i = 0; i < keys.Length; i++)
        {
            float phase = keys[i].time / original.length;
            int index = 0;
            for (int j = 1; j < spec.phases.Length; j++)
                if (phase + 0.0001f >= spec.phases[j]) index = j;
            keys[i].value = sprites[spec.sheet + "/" + spec.frames[index]];
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        string folder = OutputRoot + "/" + spec.actor;
        EnsureFolder(folder);
        string path = folder + "/" + spec.name + ".anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            UnityEngine.Object.DestroyImmediate(clip);
            clip = existing;
            EditorUtility.SetDirty(clip);
        }
        else AssetDatabase.CreateAsset(clip, path);
        if (Mathf.Abs(clip.length - original.length) > 0.0001f || clip.frameRate != original.frameRate)
            throw new InvalidDataException("Clip duration/rate changed: " + path);
        var before = AnimationUtility.GetAnimationEvents(original);
        var after = AnimationUtility.GetAnimationEvents(clip);
        if (before.Length != after.Length || before.Where((value, i) =>
            value.time != after[i].time || value.functionName != after[i].functionName ||
            value.stringParameter != after[i].stringParameter || value.floatParameter != after[i].floatParameter ||
            value.intParameter != after[i].intParameter || value.objectReferenceParameter != after[i].objectReferenceParameter ||
            value.messageOptions != after[i].messageOptions).Any())
            throw new InvalidDataException("Animation events changed: " + path);
        if (AnimationUtility.GetAnimationClipSettings(clip).loopTime != AnimationUtility.GetAnimationClipSettings(original).loopTime)
            throw new InvalidDataException("Loop setting changed: " + path);
        foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
            if (key.value == null || !AssetDatabase.GetAssetPath(key.value).StartsWith(OutputRoot + "/", StringComparison.Ordinal))
                throw new InvalidDataException("Invalid candidate sprite reference: " + path);
        report.clips++;
        report.checks.Add(path + ": original timing, frame rate, loop and events preserved.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = path.Substring(0, path.LastIndexOf('/'));
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
    }
}

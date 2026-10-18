using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Prepare only the user-specified test walking sheet and existing clip.</summary>
public static class TestPlayerWalkPreparation
{
    [Serializable] private sealed class Layout
    {
        public string source, source_sha256, clip;
        public int fps;
        public Frame[] frames;
    }
    [Serializable] private sealed class Frame
    {
        public string name;
        public int[] source_rect;
        public float pivotX, pivotY;
    }

    [MenuItem("Tools/HeroArena/Characters/Prepare TestPlayer WalkDown")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Prepare walking assets in Edit Mode.");
        var spec = JsonUtility.FromJson<Layout>(File.ReadAllText("ArtSource/Characters/UserWalkDown/slicing-layout.json"));
        using (var hash = SHA256.Create())
            if (!string.Equals(BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(spec.source))).Replace("-", ""), spec.source_sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Source sheet differs from the inspected layout.");
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(spec.clip);
        if (clip == null) throw new InvalidDataException("Existing walkdown clip not found.");
        string originalGuid = AssetDatabase.AssetPathToGUID(spec.clip);
        var events = AnimationUtility.GetAnimationEvents(clip);
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        if (AnimationUtility.GetObjectReferenceCurveBindings(clip).Any(b => b.path != binding.path || b.type != binding.type || b.propertyName != binding.propertyName))
            throw new InvalidDataException("Unexpected additional object curves; review the existing clip first.");
        AssetDatabase.ImportAsset(spec.source, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(spec.source);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048;
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.spritesheet = spec.frames.Select(f =>
        {
            int[] r = f.source_rect;
            if (r[0] < 0 || r[1] < 0 || r[2] <= 0 || r[3] <= 0 || r[0]+r[2] > width || r[1]+r[3] > height)
                throw new InvalidDataException("Invalid sprite rectangle: " + f.name);
            return new SpriteMetaData { name = f.name, rect = new Rect(r[0], height-r[1]-r[3], r[2], r[3]),
                alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(f.pivotX, f.pivotY) };
        }).ToArray();
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spec.source).OfType<Sprite>().ToArray();
        if (sprites.Length != 8) throw new InvalidDataException("Expected eight walking sprites.");
        var ordered = spec.frames.Select(f => sprites.Single(s => s.name == f.name)).ToArray();
        var keys = new ObjectReferenceKeyframe[ordered.Length];
        for (int i = 0; i < ordered.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i/(float)spec.fps, value = ordered[i] };
        // Unity includes the last sprite's frame interval in the clip length.
        clip.frameRate = spec.fps;
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        settings.startTime = 0;
        settings.stopTime = ordered.Length/(float)spec.fps;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AnimationUtility.SetAnimationEvents(clip, events);
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssetIfDirty(clip);
        // Validate the saved/imported clip, including Unity's native sprite bindings.
        AssetDatabase.ImportAsset(spec.clip, ImportAssetOptions.ForceSynchronousImport);
        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(spec.clip);
        sprites = AssetDatabase.LoadAllAssetsAtPath(spec.source).OfType<Sprite>().ToArray();
        ordered = spec.frames.Select(f => sprites.Single(s => s.name == f.name)).ToArray();
        if (AssetDatabase.AssetPathToGUID(spec.clip) != originalGuid || Mathf.Abs(clip.length-8f/spec.fps) > 0.0001f || !AnimationUtility.GetAnimationClipSettings(clip).loopTime)
            throw new InvalidDataException("Clip GUID, length or loop verification failed.");
        var actualKeys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        if (actualKeys.Length != 8 || actualKeys.Where((k, i) => k.value != ordered[i] || Mathf.Abs(k.time-i/(float)spec.fps) > 0.0001f).Any())
            throw new InvalidDataException("Sprite order/timing verification failed.");
        var previewScene = EditorSceneManager.NewPreviewScene();
        var probe = new GameObject("WalkDownVerification");
        SceneManager.MoveGameObjectToScene(probe, previewScene);
        try
        {
            var renderer = probe.AddComponent<SpriteRenderer>();
            renderer.sprite = ordered[0];
            probe.AddComponent<Animator>();
            for (int i = 0; i < ordered.Length; i++)
            {
                clip.SampleAnimation(probe, (i+0.25f)/spec.fps);
                if (renderer.sprite != ordered[i]) throw new InvalidDataException("Clip sampling failed at frame " + i + "; actual=" + (renderer.sprite == null ? "null" : renderer.sprite.name));
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(previewScene); }
        Directory.CreateDirectory("Logs/Characters");
        File.WriteAllText("Logs/Characters/testplayer-walkdown.txt", "PASS: 8 sprites, row-major order, 12 fps, 0.666667s looping, 8 equally spaced keys. All eight poses sampled on SpriteRenderer. Existing clip GUID and events preserved. No scene, controller or live actor modified.");
        Selection.activeObject = clip;
        EditorGUIUtility.PingObject(clip);
        Debug.Log("TestPlayer walkdown prepared: eight frames, 12 fps loop.");
    }
}

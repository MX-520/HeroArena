using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates and opens an isolated idle scene without saving the gameplay scene.</summary>
[InitializeOnLoad]
public static class CharacterIdlePreviewSetup
{
    public const string ScenePath = "Assets/Scenes/CharacterIdlePreview.unity";
    private const string Folder = "Assets/Art/Characters/Generated/UserHeroTrial";
    private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };
    private static double began;
    private static bool captured;
    private static readonly Dictionary<string, HashSet<string>> Seen = new Dictionary<string, HashSet<string>>();

    static CharacterIdlePreviewSetup() { EditorApplication.update += ObserveLivePreview; }

    [Serializable] private sealed class PreviewReport
    {
        public string status, error, scene, gameplaySceneHashBefore, gameplaySceneHashAfter;
        public bool playing;
        public int directions;
        public List<string> checks = new List<string>();
    }

    [MenuItem("Tools/HeroArena/Characters/Create Idle Preview Scene")]
    public static void CreateAndVerify()
    {
        var report = new PreviewReport { status = "failed", scene = ScenePath };
        Directory.CreateDirectory("Logs/Characters");
        report.gameplaySceneHashBefore = HashGameplayScene();
        try
        {
            RequireEditableWorkspace();
            CharacterActionPreparation.PrepareUserHeroTrial();
            if (!File.ReadAllText("Logs/Characters/user-hero-import-report.json").Contains("\"status\": \"passed\""))
                throw new Exception("Trial sprite import failed; inspect user-hero-import-report.json.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("WhiteHairIdlePreview");
            var animators = new List<Animator>();
            var clips = new List<AnimationClip>();
            var cameraObject = new GameObject("PreviewCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.7f;
            camera.transform.position = new Vector3(0, 0.2f, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f, 0.24f, 0.17f);
            camera.tag = "MainCamera";
            for (int index = 0; index < Directions.Length; index++)
            {
                string direction = Directions[index];
                string clipPath = Folder + "/Idle" + direction + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null || !AnimationUtility.GetAnimationClipSettings(clip).loopTime)
                    throw new InvalidDataException("Missing looping idle clip: " + clipPath);
                var actor = new GameObject("Idle" + direction);
                actor.transform.SetParent(root.transform, false);
                actor.transform.localPosition = new Vector3(-3.3f + index * 2.2f, -1.05f, 0);
                var renderer = actor.AddComponent<SpriteRenderer>();
                var animator = actor.AddComponent<Animator>();
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.runtimeAnimatorController = PrepareController(direction, clip);
                var samples = new HashSet<Sprite>();
                for (int frame = 0; frame < 8; frame++)
                {
                    clip.SampleAnimation(actor, (frame + 0.2f) / 8f * clip.length);
                    if (renderer.sprite == null) throw new Exception("Empty sampled sprite: " + direction);
                    samples.Add(renderer.sprite);
                }
                if (samples.Count < 6) throw new Exception("Insufficient sampled animation changes: " + direction);
                clip.SampleAnimation(actor, 0);
                AddText(direction == "Up" ? "Up / Back" : direction, new Vector3(actor.transform.position.x, 1.05f, 0), 0.06f);
                report.checks.Add(direction + ": looping Animator controller; " + samples.Count + " distinct sampled sprites.");
                animators.Add(animator);
                clips.Add(clip);
            }
            AddText("WHITE-HAIRED HERO", new Vector3(0, 1.85f, 0), 0.06f);
            var controls = root.AddComponent<CharacterIdlePreviewControls>();
            controls.animators = animators.ToArray();
            controls.previewCamera = camera;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save preview scene.");
            AssetDatabase.SaveAssets();
            Render(camera, "Logs/Characters/unity-idle-preview-first.png");
            for (int index = 0; index < clips.Count; index++) clips[index].SampleAnimation(animators[index].gameObject, 0.35f);
            Render(camera, "Logs/Characters/unity-idle-preview-later.png");
            for (int index = 0; index < clips.Count; index++) clips[index].SampleAnimation(animators[index].gameObject, 0);
            report.gameplaySceneHashAfter = HashGameplayScene();
            if (report.gameplaySceneHashBefore != report.gameplaySceneHashAfter)
                throw new Exception("Gameplay scene file unexpectedly changed.");
            report.status = "passed";
            report.directions = animators.Count;
            report.checks.Add("Dedicated scene saved; original SampleScene bytes unchanged.");
            Debug.Log("Four-direction idle preview created and verified.");
        }
        catch (Exception exception)
        {
            report.error = exception.ToString();
            File.WriteAllText("Logs/Characters/unity-idle-preview-report.json", JsonUtility.ToJson(report, true));
            throw;
        }
        File.WriteAllText("Logs/Characters/unity-idle-preview-report.json", JsonUtility.ToJson(report, true));
    }

    [MenuItem("Tools/HeroArena/Characters/Show Idle Preview")]
    public static void Show()
    {
        RequireEditableWorkspace();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }

    private static void RequireEditableWorkspace()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Leave Play Mode before opening this preview; current state was left unchanged.");
        if (Enumerable.Range(0, SceneManager.sceneCount).Any(index => SceneManager.GetSceneAt(index).isDirty))
            throw new InvalidOperationException("Unsaved scene detected; preview did not save or close it.");
    }

    private static AnimatorController PrepareController(string direction, AnimationClip clip)
    {
        string path = Folder + "/Preview" + direction + ".controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
        var machine = controller.layers[0].stateMachine;
        var state = machine.states.Select(child => child.state).FirstOrDefault(value => value.name == "Idle") ?? machine.AddState("Idle");
        state.motion = clip;
        state.speed = 1;
        machine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddText(string text, Vector3 position, float size)
    {
        var label = new GameObject(text + "Label");
        label.transform.position = position;
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mesh.fontSize = 64;
        mesh.characterSize = size;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.color = new Color(0.94f, 0.95f, 0.86f);
        mesh.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
    }

    private static void Render(Camera camera, string path)
    {
        var previous = RenderTexture.active;
        var target = new RenderTexture(1120, 600, 24);
        var image = new Texture2D(1120, 600, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1120, 600), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    private static string HashGameplayScene()
    {
        using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes("Assets/Scenes/SampleScene.unity"))).Replace("-", "");
    }

    private static void ObserveLivePreview()
    {
        if (!EditorApplication.isPlaying || SceneManager.GetActiveScene().path != ScenePath)
        {
            began = 0; captured = false; Seen.Clear(); return;
        }
        if (captured) return;
        if (began == 0) began = EditorApplication.timeSinceStartup;
        var controls = UnityEngine.Object.FindObjectOfType<CharacterIdlePreviewControls>();
        if (controls == null || controls.animators == null) return;
        foreach (var animator in controls.animators)
        {
            if (!Seen.ContainsKey(animator.name)) Seen.Add(animator.name, new HashSet<string>());
            var sprite = animator.GetComponent<SpriteRenderer>().sprite;
            if (sprite != null) Seen[animator.name].Add(sprite.name);
        }
        if (EditorApplication.timeSinceStartup - began < 2.5) return;
        var report = new PreviewReport { status = Seen.Count == 4 && Seen.All(pair => pair.Value.Count >= 4) ? "passed" : "failed",
            scene = ScenePath, playing = true, directions = Seen.Count };
        foreach (var entry in Seen) report.checks.Add(entry.Key + ": " + entry.Value.Count + " different sprites observed during Play Mode.");
        Directory.CreateDirectory("Logs/Characters");
        File.WriteAllText("Logs/Characters/unity-idle-live-report.json", JsonUtility.ToJson(report, true));
        ScreenCapture.CaptureScreenshot("Logs/Characters/unity-idle-live.png");
        captured = true;
    }
}

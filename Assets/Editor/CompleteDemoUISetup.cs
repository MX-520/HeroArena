using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

public static partial class CompleteDemoSetup
{
    private static TMP_FontAsset font;
    private static readonly Color Ink = new Color(0.035f, 0.075f, 0.085f, 0.96f);
    private static readonly Color Gold = new Color(0.96f, 0.74f, 0.34f);
    public static void SetupUI()
    {
        if (!System.IO.File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            AssetDatabase.importPackageCompleted += TmpImported;
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            return;
        }
        BuildUI();
    }
    private static void TmpImported(string name)
    {
        if (name != "TMP Essential Resources") return;
        AssetDatabase.importPackageCompleted -= TmpImported;
        EditorApplication.delayCall += BuildUI;
    }
    private static RectTransform Box(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        root.GetComponent<UnityEngine.UI.Image>().color = color;
        root.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        return rect;
    }
    private static RectTransform Cover(string name, Transform parent, Color color, bool block)
    {
        var rect = Box(name, parent, Vector2.one * 0.5f, Vector2.zero, Vector2.zero, color);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.GetComponent<UnityEngine.UI.Image>().raycastTarget = block;
        return rect;
    }
    private static TMP_Text Label(string name, Transform parent, Vector2 position, Vector2 size, string value, float scale, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        root.transform.SetParent(parent, false);
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
        rect.anchoredPosition = position; rect.sizeDelta = size;
        var label = root.GetComponent<TextMeshProUGUI>();
        label.font = font; label.fontSize = scale; label.color = color; label.alignment = alignment;
        label.text = value; label.raycastTarget = false;
        return label;
    }
    private static UnityEngine.UI.Button Button(string name, Transform parent, Vector2 position, string value, Color tint, UnityEngine.Events.UnityAction callback, float width = 300)
    {
        var rect = Box(name, parent, Vector2.one * 0.5f, position, new Vector2(width, 52), tint);
        var image = rect.GetComponent<UnityEngine.UI.Image>();
        image.raycastTarget = true;
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1, 0.94f, 0.72f);
        colors.pressedColor = new Color(0.65f, 0.72f, 0.74f);
        button.colors = colors;
        Label("Label", rect, Vector2.zero, new Vector2(width - 12, 48), value, 20, tint == Gold ? Ink : Color.white);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, callback);
        return button;
    }
    public static void FinalizeUIVisuals()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Object.FindObjectOfType<GameUI>().GetComponent<Canvas>().sortingOrder = 1000;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        CompleteDemoVerification.RunVisualUI();
    }

    private static void BuildUI()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) throw new Exception("TMP font import not ready");
        var wave = Object.FindObjectOfType<WaveController>();
        var waveData = new SerializedObject(wave);
        waveData.FindProperty("autoStart").boolValue = false;
        waveData.ApplyModifiedPropertiesWithoutUndo();
        var player = Object.FindObjectOfType<PlayerHealth>();
        var playerData = new SerializedObject(player);
        playerData.FindProperty("autoRespawn").boolValue = false;
        playerData.ApplyModifiedPropertiesWithoutUndo();
        var flowRoot = GameObject.Find("GameFlow") ?? new GameObject("GameFlow");
        var flow = flowRoot.GetComponent<GameFlow>() ?? flowRoot.AddComponent<GameFlow>();
        Set(flow, "waves", wave); Set(flow, "player", player);
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas.GetComponent<GameUI>() != null) throw new Exception("UI already exists; edit existing UI");
        var panel = canvas.transform.Find("StatusPanel");
        if (panel != null) Object.DestroyImmediate(panel.gameObject);
        canvas.name = "GameUI";
        canvas.sortingOrder = 1000;
        canvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = new Vector2(1280, 720);
        var ui = canvas.gameObject.AddComponent<GameUI>();
        Set(ui, "flow", flow); Set(ui, "health", player.GetComponent<Health>()); Set(ui, "effects", player.GetComponent<PlayerEffects>());
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var system = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            system.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        var hud = Cover("HUD", canvas.transform, Color.clear, false);
        var hp = Box("HealthPanel", hud, new Vector2(0, 1), new Vector2(160, -63), new Vector2(272, 86), Ink);
        var healthText = Label("HealthText", hp, new Vector2(0, 15), new Vector2(240, 35), "HEALTH 100 / 100", 19, Color.white);
        var track = Box("HealthTrack", hp, Vector2.one * 0.5f, new Vector2(0, -22), new Vector2(232, 10), new Color(0.16f, 0.24f, 0.25f));
        var fill = Box("Fill", track, Vector2.one * 0.5f, Vector2.zero, new Vector2(232, 10), new Color(0.3f, 0.85f, 0.67f)).GetComponent<UnityEngine.UI.Image>();
        fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fill.type = UnityEngine.UI.Image.Type.Filled; fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        var wavePanel = Box("WavePanel", hud, new Vector2(0.5f, 1), new Vector2(0, -63), new Vector2(430, 86), Ink);
        var waveText = Label("WaveText", wavePanel, new Vector2(0, 15), new Vector2(400, 36), "WAVE 1 / 3", 24, Gold);
        var hint = Label("WaveHint", wavePanel, new Vector2(0, -20), new Vector2(400, 28), "GET READY", 16, Color.white);
        var clockPanel = Box("RunPanel", hud, new Vector2(1, 1), new Vector2(-160, -63), new Vector2(272, 86), Ink);
        var timer = Label("Timer", clockPanel, new Vector2(0, 17), new Vector2(250, 30), "00:00 / 0 KILLS", 18, Color.white);
        Button("PauseButton", clockPanel, new Vector2(0, -20), "PAUSE  [ ESC ]", new Color(0.12f, 0.25f, 0.27f), flow.Pause, 240).GetComponent<RectTransform>().sizeDelta = new Vector2(240, 32);
        var controls = Box("Controls", hud, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1000, 42), Ink);
        Label("ControlsText", controls, Vector2.zero, new Vector2(970, 34), "WASD move   /   LMB shoot   /   RMB/J melee   /   SPACE roll   /   SHIFT run   /   ESC pause", 17, Color.white);
        var buffLabels = new TMP_Text[3];
        string[] types = { "Barrage", "Giant", "Shield" };
        for (int i = 0; i < 3; i++)
        {
            var card = Box(types[i] + "Card", hud, new Vector2(0.5f, 0), new Vector2((i - 1) * 106, 108), new Vector2(94, 94), Ink);
            var icon = Box("Icon", card, Vector2.one * 0.5f, new Vector2(0, 16), new Vector2(48, 48), Color.white).GetComponent<UnityEngine.UI.Image>();
            icon.sprite = Icon(types[i]); icon.preserveAspect = true;
            buffLabels[i] = Label("Time", card, new Vector2(0, -26), new Vector2(86, 24), "--", 17, Gold);
        }
        var menu = Cover("MainMenu", canvas.transform, new Color(0.01f, 0.03f, 0.04f, 0.48f), true);
        var frame = Box("MenuFrame", menu, Vector2.one * 0.5f, Vector2.zero, new Vector2(1060, 590), Ink);
        Label("Eyebrow", frame, new Vector2(-180, 237), new Vector2(620, 28), "A THREE-WAVE FOREST TRIAL", 17, Gold, TextAlignmentOptions.Left);
        Label("Title", frame, new Vector2(-180, 168), new Vector2(620, 100), "HERO ARENA", 64, Color.white, TextAlignmentOptions.Left);
        Label("Subtitle", frame, new Vector2(-180, 89), new Vector2(620, 65), "Hold your ground. Break the ambush.\nMaster steel, arrows and forest relics.", 21, new Color(0.68f, 0.8f, 0.79f), TextAlignmentOptions.Left);
        Button("StartButton", frame, new Vector2(-330, -30), "ENTER THE FOREST", Gold, flow.BeginRun);
        Button("QuitButton", frame, new Vector2(-330, -100), "QUIT", new Color(0.13f, 0.23f, 0.25f), flow.Quit);
        var guide = Box("FieldGuide", frame, Vector2.one * 0.5f, new Vector2(320, 15), new Vector2(340, 460), new Color(0.06f, 0.13f, 0.14f));
        Label("GuideTitle", guide, new Vector2(0, 182), new Vector2(300, 40), "FIELD GUIDE", 25, Gold);
        Label("GuideText", guide, new Vector2(0, 12), new Vector2(290, 300), "01  FIRST CONTACT\nThree goblins. Learn to aim.\n\n02  THE HUNT\nRunners share their prey.\n\n03  CROSSFIRE\nMarked shots. Keep moving.\n\nBreak obstacles for relics.\nOne healing drop per wave.", 18, Color.white, TextAlignmentOptions.TopLeft);
        string[] relics = { "Heal", "Barrage", "Giant", "Shield" };
        for (int i = 0; i < 4; i++)
        {
            var icon = Box(relics[i], frame, Vector2.one * 0.5f, new Vector2(-440 + i * 135, -220), new Vector2(50, 50), Color.white).GetComponent<UnityEngine.UI.Image>();
            icon.sprite = Icon(relics[i]); icon.preserveAspect = true;
            Label("Name", frame, new Vector2(-440 + i * 135, -260), new Vector2(120, 25), relics[i].ToUpperInvariant(), 13, new Color(0.68f, 0.8f, 0.79f));
        }
        var modal = Cover("Modal", canvas.transform, new Color(0.01f, 0.025f, 0.03f, 0.78f), true);
        var dialog = Box("Dialog", modal, Vector2.one * 0.5f, Vector2.zero, new Vector2(650, 480), Ink);
        var heading = Label("Heading", dialog, new Vector2(0, 165), new Vector2(610, 70), "TAKE A BREATH", 36, Gold);
        var detail = Label("Detail", dialog, new Vector2(0, 90), new Vector2(570, 85), "", 20, Color.white);
        var resume = Button("ResumeButton", dialog, new Vector2(0, -10), "RESUME", Gold, flow.Resume);
        Button("RestartButton", dialog, new Vector2(0, -80), "RESTART CHALLENGE", new Color(0.12f, 0.26f, 0.28f), flow.Restart);
        Button("MenuButton", dialog, new Vector2(0, -150), "MAIN MENU", new Color(0.12f, 0.26f, 0.28f), flow.MainMenu);
        Set(ui, "menu", menu.gameObject); Set(ui, "hud", hud.gameObject); Set(ui, "modal", modal.gameObject);
        Set(ui, "resumeButton", resume.gameObject); Set(ui, "heading", heading); Set(ui, "detail", detail);
        Set(ui, "waveText", waveText); Set(ui, "hint", hint); Set(ui, "healthText", healthText); Set(ui, "healthFill", fill); Set(ui, "timer", timer);
        var uiData = new SerializedObject(ui);
        var buffArray = uiData.FindProperty("buffTimes"); buffArray.arraySize = 3;
        for (int i = 0; i < 3; i++) buffArray.GetArrayElementAtIndex(i).objectReferenceValue = buffLabels[i];
        uiData.ApplyModifiedPropertiesWithoutUndo();
        menu.gameObject.SetActive(true); hud.gameObject.SetActive(false); modal.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunUI();
    }
}

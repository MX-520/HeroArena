using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

public static class ChineseUISetup
{
    public const string FontPath = "Assets/UI/Fonts/HeroArenaChinese SDF.asset";
    private static readonly Dictionary<string,string> labels = new Dictionary<string,string> {
        {"Eyebrow","三波森林试炼"}, {"Title","英雄竞技场"},
        {"Subtitle","守住阵地，击退伏兵。\n善用近战、射击与森林遗物。"},
        {"GuideTitle","战斗指南"},
        {"GuideText","第一波　初次交锋\n三只哥布林，熟悉瞄准。\n\n第二波　群体追猎\n疾行哥布林会呼唤同伴。\n\n第三波　交叉火力\n留意预警，保持移动。\n\n击碎障碍物可获得道具。\n每波至少掉落一瓶治疗药水。"},
        {"ControlsText","WASD 移动　/　左键射击　/　右键/J 近战　/　空格翻滚　/　Shift 奔跑"},
        {"HealthText","生命　100 / 100"}, {"WaveText","第 1 / 3 波　剩余 3"},
        {"WaveHint","准备战斗"}, {"Timer","00:00　击败 0"},
        {"Heading","已暂停"}, {"Detail","按 Esc 继续战斗。"}, {"Time","—"}
    };
    private static readonly Dictionary<string,string> buttons = new Dictionary<string,string> {
        {"StartButton","进入森林"},{"QuitButton","退出游戏"},{"PauseButton","暂停 [Esc]"},
        {"ResumeButton","继续游戏"},{"RestartButton","重新挑战"},{"MenuButton","返回主菜单"}
    };
    public static void OpenPreview()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.isPlaying=true;
    }
    public static void Apply()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        ApplyToOpenScene();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunVisualUI();
    }
    public static void ApplyToOpenScene()
    {
        var ui = Object.FindObjectOfType<GameUI>();
        var font = GetFont();
        var data = new SerializedObject(ui);
        var fill = (UnityEngine.UI.Image)data.FindProperty("healthFill").objectReferenceValue;
        var all = ui.GetComponentsInChildren<TMP_Text>(true);
        string[] relicNames = {"治疗","多弹道","巨化","护罩"};
        var names = all.Where(t=>t.name=="Name").OrderBy(t=>t.rectTransform.anchoredPosition.x).ToArray();
        for(int i=0;i<names.Length;i++) names[i].text=relicNames[i];
        foreach(var text in all)
        {
            if(labels.TryGetValue(text.name,out string value)) text.text=value;
            if(text.name=="Label" && buttons.TryGetValue(text.transform.parent.name,out string buttonText)) text.text=buttonText;
            text.font=font;
            if(text.color.r+text.color.g+text.color.b<0.8f) text.color=Color.white;
            if(text.name=="GuideText")
            {
                text.rectTransform.sizeDelta=new Vector2(300,330);
                text.rectTransform.anchoredPosition=new Vector2(0,-10);
                text.enableAutoSizing=true; text.fontSizeMin=14; text.fontSizeMax=18;
            }
            EditorUtility.SetDirty(text);
        }
        foreach(var image in ui.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            bool graphic=image==fill || image.sprite!=null && (image.name=="MenuArtwork" || image.name=="Icon" || new[]{"Heal","Barrage","Giant","Shield"}.Contains(image.name));
            if(!graphic)
            {
                var opacity = BackgroundOpacity(image.name);
                image.color = new Color(0.025f,0.055f,0.065f,opacity);
                if(opacity>0 && image.name!="HealthTrack")
                {
                    // A filled Image's Outline also repeats its interior; keep the backdrop a single translucent layer.
                    var border=image.GetComponent<UnityEngine.UI.Outline>();
                    if(border!=null) Object.DestroyImmediate(border);
                }
            }
            EditorUtility.SetDirty(image);
        }
        foreach(var button in ui.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            var text=button.GetComponentInChildren<TMP_Text>(true);
            button.targetGraphic=text;
            var colors=button.colors; colors.normalColor=Color.white;
            colors.highlightedColor=new Color(1,0.84f,0.4f,1);
            colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(0.8f,0.8f,0.8f,1);
            button.colors=colors; EditorUtility.SetDirty(button);
            if(button.name=="PauseButton") data.FindProperty("pauseButton").objectReferenceValue=button.gameObject;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        ApplyReadableLayout(ui);
        DecoratedUISetup.ApplyHUD(ui);
        ForestMenuSetup.Apply(ui);
        DecoratedUISetup.ApplyButtonFeedback(ui);
        var wave=new SerializedObject(Object.FindObjectOfType<WaveController>());
        string[] titles={"初次交锋","群体追猎","交叉火力"};
        for(int i=0;i<3;i++) wave.FindProperty("waves").GetArrayElementAtIndex(i).FindPropertyRelative("title").stringValue=titles[i];
        wave.ApplyModifiedPropertiesWithoutUndo();
        Canvas.ForceUpdateCanvases();
    }
    private static float BackgroundOpacity(string name)
    {
        switch(name)
        {
            case "HealthPanel": case "WavePanel": case "RunPanel": return 0.42f;
            case "BarrageCard": case "GiantCard": case "ShieldCard": return 0.34f;
            case "Controls": return 0.24f;
            case "HealthTrack": return 0.28f;
            case "MenuFrame": return 0.52f;
            case "FieldGuide": return 0.25f;
            case "Dialog": return 0.89f;
            case "StartButton": case "QuitButton": case "PauseButton":
            case "ResumeButton": case "RestartButton": case "MenuButton": return 0.32f;
            default: return 0;
        }
    }
    private static void ApplyReadableLayout(GameUI ui)
    {
        var rects=ui.GetComponentsInChildren<RectTransform>(true);
        Action<string,Vector2,Vector2> place=(name,position,size)=> {
            var rect=rects.First(r=>r.name==name);
            rect.anchoredPosition=position;rect.sizeDelta=size;EditorUtility.SetDirty(rect);
        };
        place("HealthPanel",new Vector2(143,-49),new Vector2(242,70));
        place("HealthText",new Vector2(0,12),new Vector2(220,30));
        place("HealthTrack",new Vector2(0,-17),new Vector2(212,8));
        place("Fill",Vector2.zero,new Vector2(212,8));
        place("WavePanel",new Vector2(0,-49),new Vector2(320,70));
        place("WaveText",new Vector2(0,12),new Vector2(300,32));
        place("WaveHint",new Vector2(0,-17),new Vector2(300,24));
        place("RunPanel",new Vector2(-136,-49),new Vector2(228,70));
        place("Timer",new Vector2(0,14),new Vector2(210,28));
        place("PauseButton",new Vector2(0,-17),new Vector2(202,28));
        place("Controls",new Vector2(0,18),new Vector2(820,30));
        place("ControlsText",Vector2.zero,new Vector2(800,26));
        string[] cards={"BarrageCard","GiantCard","ShieldCard"};
        for(int i=0;i<3;i++)
        {
            place(cards[i],new Vector2((i-1)*94,76),new Vector2(80,72));
            var card=rects.First(r=>r.name==cards[i]);
            var icon=(RectTransform)card.Find("Icon");icon.anchoredPosition=new Vector2(0,12);icon.sizeDelta=new Vector2(48,48);
            var time=(RectTransform)card.Find("Time");time.anchoredPosition=new Vector2(0,-24);time.sizeDelta=new Vector2(76,22);
            EditorUtility.SetDirty(icon);EditorUtility.SetDirty(time);
        }
        foreach(var text in ui.GetComponentsInChildren<TMP_Text>(true))
        {
            bool accent=new[]{"Eyebrow","GuideTitle","Heading","WaveText"}.Contains(text.name);
            text.color=accent?new Color(1,0.91f,0.68f):Color.white;
            if(new[]{"HealthText","Timer","WaveText"}.Contains(text.name) || text.transform.parent.name=="PauseButton") text.color=new Color(1,0.86f,0.45f);
            if(text.name=="WaveHint") text.color=new Color(0.98f,0.89f,0.68f);
            bool bold=new[]{"HealthText","WaveText","Timer","Heading","Title","GuideTitle","Label","Time"}.Contains(text.name);
            text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
            if(text.name=="WaveHint") text.fontStyle=FontStyles.Bold;
            if(text.name=="WaveText") text.fontSize=21;
            if(text.name=="WaveHint") text.fontSize=14;
            if(text.name=="Timer" || text.transform.parent.name=="PauseButton") text.fontSize=17;
            if(text.name=="Time") text.fontSize=16;
            if(text.name=="ControlsText") text.fontSize=14;
            if(text.name=="ControlsText")
            {
                text.color=new Color(0.04f,0.18f,0.20f,0.82f);
                text.fontStyle=FontStyles.Normal;
                const string path="Assets/UI/Fonts/ControlsText.mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null) { material=new Material(text.font.material);material.name="ControlsText";AssetDatabase.CreateAsset(material,path); }
                material.SetTexture(ShaderUtilities.ID_MainTex,text.font.atlasTexture);
                material.SetFloat(ShaderUtilities.ID_OutlineWidth,0);
                material.DisableKeyword("OUTLINE_ON");
                text.fontSharedMaterial=material;EditorUtility.SetDirty(material);
            }
            if(text.transform.parent.name=="PauseButton") text.rectTransform.sizeDelta=new Vector2(198,26);
            EditorUtility.SetDirty(text);EditorUtility.SetDirty(text.rectTransform);
        }
    }
    private static TMP_FontAsset GetFont()
    {
        AssetDatabase.Refresh();
        var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if(asset==null)
        {
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/NotoSansCJKsc-Regular.otf");
            if(source==null) throw new Exception("Chinese font source is missing");
            asset=TMP_FontAsset.CreateFontAsset(source,48,5,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic);
            asset.name="HeroArenaChinese SDF";
            AssetDatabase.CreateAsset(asset,FontPath);
        }
        string content=string.Join("",labels.Values)+string.Join("",buttons.Values)+"治疗多弹道巨化护罩初次交锋群体追猎交叉火力森林试炼"+File.ReadAllText("Assets/Scripts/UI/GameUI.cs")+File.ReadAllText("Assets/Editor/ForestMenuSetup.cs");
        string characters=new string(content.Concat(Enumerable.Range(32,95).Select(i=>(char)i)).Where(c=>!char.IsControl(c)).Distinct().ToArray());
        // TMP 3.0.x reports false when all requested glyphs already exist; add only new characters.
        string additions=new string(characters.Where(c=>!asset.HasCharacter(c)).ToArray());
        if(additions.Length>0)
        {
            asset.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            if(!asset.TryAddCharacters(additions,out string missing)) throw new Exception("Missing Chinese glyphs: "+missing);
        }
        asset.atlasPopulationMode=AtlasPopulationMode.Static;
        foreach(var texture in asset.atlasTextures) if(!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture,asset);
        if(!AssetDatabase.Contains(asset.material)) AssetDatabase.AddObjectToAsset(asset.material,asset);
        asset.material.EnableKeyword("OUTLINE_ON");
        asset.material.SetFloat(ShaderUtilities.ID_OutlineWidth,0.24f);
        asset.material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(0,0,0,0.95f));
        EditorUtility.SetDirty(asset);EditorUtility.SetDirty(asset.material);
        return asset;
    }
}

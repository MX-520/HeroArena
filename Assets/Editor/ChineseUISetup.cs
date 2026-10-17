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
            bool graphic=image==fill || image.sprite!=null && (image.name=="Icon" || new[]{"Heal","Barrage","Giant","Shield"}.Contains(image.name));
            if(!graphic) { var color=image.color;color.a=0;image.color=color; }
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
        var wave=new SerializedObject(Object.FindObjectOfType<WaveController>());
        string[] titles={"初次交锋","群体追猎","交叉火力"};
        for(int i=0;i<3;i++) wave.FindProperty("waves").GetArrayElementAtIndex(i).FindPropertyRelative("title").stringValue=titles[i];
        wave.ApplyModifiedPropertiesWithoutUndo();
        Canvas.ForceUpdateCanvases();
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
        string content=string.Join("",labels.Values)+string.Join("",buttons.Values)+"治疗多弹道巨化护罩初次交锋群体追猎交叉火力森林试炼"+File.ReadAllText("Assets/Scripts/UI/GameUI.cs");
        string characters=new string(content.Concat(Enumerable.Range(32,95).Select(i=>(char)i)).Where(c=>!char.IsControl(c)).Distinct().ToArray());
        asset.atlasPopulationMode=AtlasPopulationMode.Dynamic;
        if(!asset.TryAddCharacters(characters,out string missing)) throw new Exception("Missing Chinese glyphs: "+missing);
        asset.atlasPopulationMode=AtlasPopulationMode.Static;
        foreach(var texture in asset.atlasTextures) if(!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture,asset);
        if(!AssetDatabase.Contains(asset.material)) AssetDatabase.AddObjectToAsset(asset.material,asset);
        asset.material.EnableKeyword("OUTLINE_ON");
        asset.material.SetFloat(ShaderUtilities.ID_OutlineWidth,0.16f);
        asset.material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(0,0,0,0.95f));
        EditorUtility.SetDirty(asset);EditorUtility.SetDirty(asset.material);
        return asset;
    }
}

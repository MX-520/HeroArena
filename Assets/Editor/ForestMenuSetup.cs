using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Full-screen key art and left-side menu, reusing the existing buttons and GameFlow.</summary>
public static class ForestMenuSetup
{
    private const string ArtworkPath = "Assets/UI/Artwork/ForestTrial.png";
    public static void Apply(GameUI ui)
    {
        var menu = ui.transform.Find("MainMenu") as RectTransform;
        if(menu == null) throw new InvalidOperationException("Existing MainMenu is missing");
        var importer = AssetImporter.GetAtPath(ArtworkPath) as TextureImporter;
        if(importer == null) throw new InvalidOperationException("Forest menu artwork is missing");
        if(importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        var art = menu.Find("MenuArtwork") as RectTransform;
        if(art == null)
        {
            art = new GameObject("MenuArtwork", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.AspectRatioFitter)).GetComponent<RectTransform>();
            art.SetParent(menu, false);
        }
        art.SetAsFirstSibling();
        var image = art.GetComponent<UnityEngine.UI.Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtworkPath);
        image.color = Color.white; image.raycastTarget = false;
        var fit = art.GetComponent<UnityEngine.UI.AspectRatioFitter>();
        fit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        EditorUtility.SetDirty(image); EditorUtility.SetDirty(fit);

        var frame = menu.Find("MenuFrame") as RectTransform;
        Place(frame, new Vector2(0,0.5f), new Vector2(258,0), new Vector2(424,630));
        frame.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
        Text(frame,"Eyebrow","森林遗迹 · 三波试炼",new Vector2(0,238),new Vector2(388,28),15,new Color(0.76f,0.87f,0.78f));
        Text(frame,"Title","英雄竞技场",new Vector2(0,172),new Vector2(414,92),59,new Color(1,0.91f,0.67f),true);
        Text(frame,"Subtitle","守住阵地，击退伏兵。\n善用近战、射击与森林遗物。",new Vector2(0,96),new Vector2(388,64),19,new Color(0.83f,0.90f,0.88f));
        var start = frame.Find("StartButton") as RectTransform;
        Place(start,Vector2.one*0.5f,new Vector2(0,-6),new Vector2(316,56));
        start.GetComponent<UnityEngine.UI.Image>().color=new Color(0.035f,0.12f,0.12f,0.56f);
        var startText=start.GetComponentInChildren<TMP_Text>(true);
        startText.rectTransform.sizeDelta=new Vector2(304,50);startText.fontSize=23;
        startText.color=new Color(1,0.92f,0.72f);
        var quit=frame.Find("QuitButton") as RectTransform;
        Place(quit,Vector2.one*0.5f,new Vector2(0,-78),new Vector2(316,44));
        var quitText=quit.GetComponentInChildren<TMP_Text>(true);
        quitText.rectTransform.sizeDelta=new Vector2(304,40);quitText.fontSize=18;
        quitText.color=new Color(0.79f,0.87f,0.85f);

        string[] relics={"Heal","Barrage","Giant","Shield"};
        for(int i=0;i<relics.Length;i++)
            Place(frame.Find(relics[i]) as RectTransform,Vector2.one*0.5f,new Vector2(-135+i*90,-166),new Vector2(38,38));
        var names=frame.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="Name").OrderBy(t=>t.rectTransform.anchoredPosition.x).ToArray();
        for(int i=0;i<names.Length;i++)
        {
            Place(names[i].rectTransform,Vector2.one*0.5f,new Vector2(-135+i*90,-205),new Vector2(86,24));
            names[i].fontSize=13;names[i].color=new Color(0.76f,0.85f,0.82f);
        }
        var guide=frame.Find("FieldGuide") as RectTransform;
        Place(guide,Vector2.one*0.5f,new Vector2(0,-271),new Vector2(404,68));
        guide.GetComponent<UnityEngine.UI.Image>().color=Color.clear;
        Text(guide,"GuideTitle","近战 · 射击 · 生存",new Vector2(0,17),new Vector2(398,27),15,new Color(0.91f,0.80f,0.54f),true);
        Text(guide,"GuideText","翻滚避险，收集遗物，迎战三波伏兵。",new Vector2(0,-17),new Vector2(398,26),13,new Color(0.77f,0.85f,0.81f));
        DecoratedUISetup.Frame(menu,new Color(0.87f,0.76f,0.48f,0.38f),28);
        var trim=menu.Find("ArenaTrim") as RectTransform;
        trim.offsetMin=new Vector2(18,18);trim.offsetMax=new Vector2(-18,-18);
        foreach(var component in menu.GetComponentsInChildren<Component>(true))
            if(component!=null) EditorUtility.SetDirty(component);
    }

    private static void Place(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size)
    {
        rect.anchorMin=rect.anchorMax=anchor;rect.pivot=Vector2.one*0.5f;
        rect.anchoredPosition=position;rect.sizeDelta=size;
    }
    private static void Text(Transform parent,string name,string value,Vector2 position,Vector2 size,float fontSize,Color color,bool bold=false)
    {
        var label=parent.Find(name).GetComponent<TMP_Text>();
        Place(label.rectTransform,Vector2.one*0.5f,position,size);
        label.text=value;label.enableAutoSizing=false;label.fontSize=fontSize;
        label.color=color;label.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
        label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
    }
}

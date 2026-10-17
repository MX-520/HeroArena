using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Idempotent decoration of existing UI; leaves gameplay and the Controls subtree untouched.</summary>
public static class DecoratedUISetup
{
    public static void ApplyHUD(GameUI ui)
    {
        string[] panels = { "HealthPanel", "WavePanel", "RunPanel", "BarrageCard", "GiantCard", "ShieldCard", "Dialog" };
        foreach (var panel in ui.GetComponentsInChildren<RectTransform>(true).Where(r => panels.Contains(r.name)))
        {
            Color tint = new Color(0.82f, 0.72f, 0.46f, 0.70f);
            if (panel.name == "HealthPanel" || panel.name == "ShieldCard") tint = new Color(0.39f, 0.86f, 0.79f, 0.72f);
            if (panel.name == "GiantCard") tint = new Color(0.76f, 0.54f, 0.98f, 0.65f);
            Frame(panel, tint);
        }
        foreach (var button in ui.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            Frame(button.GetComponent<RectTransform>(), new Color(0.87f, 0.77f, 0.50f, 0.65f), 8);
        ApplyModal(ui);
    }

    private static void ApplyModal(GameUI ui)
    {
        var dialog = ui.transform.Find("Modal/Dialog") as RectTransform;
        dialog.sizeDelta = new Vector2(500,440);
        dialog.anchoredPosition = new Vector2(0,14);
        dialog.GetComponent<UnityEngine.UI.Image>().color = new Color(0.025f,0.055f,0.065f,0.89f);
        var font = dialog.Find("Heading").GetComponent<TMP_Text>().font;
        var emblem = Child(dialog,"StatusEmblem",new Vector2(0,160),new Vector2(64,64));
        var badge = emblem.GetComponent<ArenaStatusEmblem>() ?? emblem.gameObject.AddComponent<ArenaStatusEmblem>();
        badge.Show(GameFlow.ScreenState.Paused);
        var stats = Child(dialog,"ModalStats",new Vector2(0,-12),new Vector2(412,60));
        var image = stats.GetComponent<UnityEngine.UI.Image>() ?? stats.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.08f,0.17f,0.17f,0.56f);image.raycastTarget=false;
        Frame(stats,new Color(0.45f,0.79f,0.72f,0.45f),7);
        var textRect = Child(stats,"ModalSummary",Vector2.zero,new Vector2(394,56));
        var summary=textRect.GetComponent<TextMeshProUGUI>() ?? textRect.gameObject.AddComponent<TextMeshProUGUI>();
        summary.font=font;summary.fontSize=17;summary.fontStyle=FontStyles.Bold;
        summary.color=new Color(1,0.86f,0.45f);summary.alignment=TextAlignmentOptions.Center;
        summary.raycastTarget=false;summary.text="第 1 / 3 波　·　击败 0\n战斗用时　0.0 秒";
        var data = new SerializedObject(ui);
        data.FindProperty("statusEmblem").objectReferenceValue=badge;
        data.FindProperty("modalSummary").objectReferenceValue=summary;
        data.ApplyModifiedPropertiesWithoutUndo();
        SetText(dialog.Find("Heading").GetComponent<TMP_Text>(),new Vector2(0,94),new Vector2(450,52),34);
        SetText(dialog.Find("Detail").GetComponent<TMP_Text>(),new Vector2(0,48),new Vector2(446,38),17);
        foreach(var text in dialog.GetComponentsInChildren<TMP_Text>(true))
        {
            text.color=new Color(1,0.86f,0.45f);
            text.fontStyle=FontStyles.Bold;
            if(text.name=="Detail") text.color=new Color(0.97f,0.87f,0.66f);
            EditorUtility.SetDirty(text);
        }
        foreach(var button in dialog.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            button.GetComponent<RectTransform>().sizeDelta=new Vector2(360,48);
            var label=button.GetComponentInChildren<TMP_Text>(true);
            label.rectTransform.sizeDelta=new Vector2(346,44);label.fontSize=20;
            var colors=button.colors;colors.normalColor=Color.white;
            colors.highlightedColor=new Color(1,1,0.80f);colors.selectedColor=colors.highlightedColor;
            button.colors=colors;
        }
        foreach(var component in dialog.GetComponentsInChildren<Component>(true))
            if(component!=null) EditorUtility.SetDirty(component);
    }

    private static RectTransform Child(Transform parent,string name,Vector2 position,Vector2 size)
    {
        var child=parent.Find(name) as RectTransform;
        if(child==null) { child=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();child.SetParent(parent,false); }
        child.anchorMin=child.anchorMax=Vector2.one*0.5f;
        child.anchoredPosition=position;child.sizeDelta=size;
        return child;
    }
    private static void SetText(TMP_Text text,Vector2 position,Vector2 size,float fontSize)
    {
        text.rectTransform.anchoredPosition=position;text.rectTransform.sizeDelta=size;
        text.fontSize=fontSize;text.enableAutoSizing=false;
    }

    public static void Frame(RectTransform parent, Color tint, float corners = 12)
    {
        var child = parent.Find("ArenaTrim") as RectTransform;
        if (child == null)
        {
            child = new GameObject("ArenaTrim", typeof(RectTransform), typeof(ArenaPanelFrame)).GetComponent<RectTransform>();
            child.SetParent(parent, false);
        }
        child.anchorMin = Vector2.zero; child.anchorMax = Vector2.one;
        if(child.GetComponent<CanvasRenderer>()==null) child.gameObject.AddComponent<CanvasRenderer>();
        child.offsetMin = child.offsetMax = Vector2.zero;
        child.GetComponent<ArenaPanelFrame>().Configure(tint, corners);
        child.SetAsLastSibling();
        EditorUtility.SetDirty(child); EditorUtility.SetDirty(child.GetComponent<ArenaPanelFrame>());
    }
}

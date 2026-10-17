using System.Linq;
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

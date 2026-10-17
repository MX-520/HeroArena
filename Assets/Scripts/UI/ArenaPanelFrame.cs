using UnityEngine;
using UnityEngine.UI;

/// <summary>Transparent-center panel trim. Geometry only: no repeated filled Image or input target.</summary>
[AddComponentMenu("HeroArena/UI/Arena Panel Frame")]
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArenaPanelFrame : MaskableGraphic
{
    [SerializeField] private Color accent = new Color(0.84f, 0.72f, 0.43f, 0.75f);
    [SerializeField] private float cornerLength = 12;

    public void Configure(Color tint, float corners = 12)
    {
        if(accent==tint && Mathf.Approximately(cornerLength,corners)) return;
        accent = tint;
        cornerLength = corners;
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        raycastTarget = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        if (r.width < 4 || r.height < 4) return;
        float length = Mathf.Min(cornerLength, Mathf.Min(r.width, r.height) * 0.25f);
        Color faint = accent; faint.a *= 0.28f;
        // Fine perimeter, with brighter short corner inlays. The center contains no geometry.
        Quad(vh, r.xMin, r.yMin, r.width, 0.65f, faint);
        Quad(vh, r.xMin, r.yMax - 0.65f, r.width, 0.65f, faint);
        Quad(vh, r.xMin, r.yMin, 0.65f, r.height, faint);
        Quad(vh, r.xMax - 0.65f, r.yMin, 0.65f, r.height, faint);
        foreach (int x in new[] { -1, 1 })
        foreach (int y in new[] { -1, 1 })
        {
            float left = x < 0 ? r.xMin : r.xMax - length;
            float bottom = y < 0 ? r.yMin : r.yMax - 1.4f;
            Quad(vh, left, bottom, length, 1.4f, accent);
            left = x < 0 ? r.xMin : r.xMax - 1.4f;
            bottom = y < 0 ? r.yMin : r.yMax - length;
            Quad(vh, left, bottom, 1.4f, length, accent);
        }
        Quad(vh, r.center.x - 12, r.yMax - 0.8f, 24, 0.8f, accent);
    }

    private static void Quad(VertexHelper vh, float x, float y, float width, float height, Color tint)
    {
        int start = vh.currentVertCount;
        vh.AddVert(new Vector3(x, y), tint, Vector2.zero);
        vh.AddVert(new Vector3(x, y + height), tint, Vector2.zero);
        vh.AddVert(new Vector3(x + width, y + height), tint, Vector2.zero);
        vh.AddVert(new Vector3(x + width, y), tint, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start);
    }
}

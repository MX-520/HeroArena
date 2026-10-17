using UnityEngine;
using UnityEngine.UI;

/// <summary>Small geometric pause / trophy / crossed-blades badge; has no input or gameplay behavior.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArenaStatusEmblem : MaskableGraphic
{
    [SerializeField] private GameFlow.ScreenState state = GameFlow.ScreenState.Paused;

    public void Show(GameFlow.ScreenState value)
    {
        state = value;
        color = value == GameFlow.ScreenState.Defeat
            ? new Color(1, 0.61f, 0.39f) : new Color(1, 0.86f, 0.45f);
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnEnable() { base.OnEnable(); raycastTarget = false; }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float scale = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 64;
        Vector2 center = rectTransform.rect.center;
        Color dim = color; dim.a = 0.4f;
        for (int i = 0; i < 40; i++)
        {
            float a = i * Mathf.PI * 2 / 40, b = (i + 1) * Mathf.PI * 2 / 40;
            Polygon(vh, dim, center, scale,
                new Vector2(Mathf.Cos(a), Mathf.Sin(a))*29,
                new Vector2(Mathf.Cos(b), Mathf.Sin(b))*29,
                new Vector2(Mathf.Cos(b), Mathf.Sin(b))*27.8f,
                new Vector2(Mathf.Cos(a), Mathf.Sin(a))*27.8f);
        }
        if (state == GameFlow.ScreenState.Paused)
        {
            Rectangle(vh, center, scale, -10,-12,7,24);
            Rectangle(vh, center, scale, 3,-12,7,24);
        }
        else if (state == GameFlow.ScreenState.Victory)
        {
            Polygon(vh,color,center,scale,new Vector2(-13,12),new Vector2(13,12),new Vector2(8,-3),new Vector2(-8,-3));
            Rectangle(vh,center,scale,-2,-11,4,10);
            Rectangle(vh,center,scale,-10,-15,20,4);
            Polygon(vh,color,center,scale,new Vector2(-19,10),new Vector2(-14,10),new Vector2(-10,-2),new Vector2(-16,2));
            Polygon(vh,color,center,scale,new Vector2(14,10),new Vector2(19,10),new Vector2(16,2),new Vector2(10,-2));
        }
        else
        {
            Polygon(vh,color,center,scale,new Vector2(-14,11),new Vector2(-10,15),new Vector2(14,-11),new Vector2(10,-15));
            Polygon(vh,color,center,scale,new Vector2(-14,-11),new Vector2(-10,-15),new Vector2(14,11),new Vector2(10,15));
        }
    }
    private void Rectangle(VertexHelper vh, Vector2 center,float scale,float x,float y,float w,float h)
        => Polygon(vh,color,center,scale,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
    private static void Polygon(VertexHelper vh,Color tint,Vector2 center,float scale,params Vector2[] points)
    {
        int start=vh.currentVertCount;
        foreach(var point in points) vh.AddVert(center+point*scale,tint,Vector2.zero);
        vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
    }
}

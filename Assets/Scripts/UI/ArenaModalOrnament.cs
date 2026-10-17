using UnityEngine;
using UnityEngine.UI;

/// <summary>Low-opacity forest runes and leaf inlays around the dialog margins.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArenaModalOrnament : MaskableGraphic
{
    protected override void OnEnable() { base.OnEnable();raycastTarget=false; }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();Rect r=rectTransform.rect;
        Color gold=new Color(0.94f,0.80f,0.42f,0.35f), dim=new Color(0.55f,0.80f,0.68f,0.19f);
        float cy=r.yMax-64;
        foreach(int side in new[]{-1,1})
        {
            float x=side*(r.width/2-22);
            Line(vh,new Vector2(x,r.yMin+24),new Vector2(x,r.yMax-24),0.7f,dim);
            Line(vh,new Vector2(side*52,cy),new Vector2(side*(r.width/2-42),cy),0.8f,gold);
            Diamond(vh,new Vector2(side*143,cy),5,gold);
            for(int i=0;i<5;i++)
            {
                float y=r.yMin+45+i*(r.height-90)/4;
                Line(vh,new Vector2(x,y),new Vector2(x-side*10,y+10),1.2f,dim);
                Line(vh,new Vector2(x,y),new Vector2(x+side*8,y+8),0.8f,dim);
            }
        }
        // A broken halo sits behind the status badge, leaving a quiet center for the icon.
        for(int i=0;i<48;i++)
        {
            if(i%12>8) continue;
            float a=i*Mathf.PI*2/48,b=(i+1)*Mathf.PI*2/48;
            Line(vh,new Vector2(Mathf.Cos(a)*44,cy+Mathf.Sin(a)*44),new Vector2(Mathf.Cos(b)*44,cy+Mathf.Sin(b)*44),0.8f,dim);
        }
    }
    private static void Diamond(VertexHelper vh,Vector2 p,float size,Color tint)
    {
        int n=vh.currentVertCount;
        foreach(var v in new[]{Vector2.up,Vector2.right,Vector2.down,Vector2.left}) vh.AddVert(p+v*size,tint,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
    private static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint)
    {
        Vector2 d=(b-a).normalized;Vector2 p=new Vector2(-d.y,d.x)*width/2;
        int n=vh.currentVertCount;
        foreach(var v in new[]{a+p,b+p,b-p,a-p}) vh.AddVert(v,tint,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
}

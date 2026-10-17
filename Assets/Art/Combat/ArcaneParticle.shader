Shader "HeroArena/ArcaneParticle"
{
    Properties { _Shape ("Shape (glow / spark / trail)", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Shape;
            struct input { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            struct output { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            output vert(input v) { output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o; }
            fixed4 frag(output i):SV_Target
            {
                float2 p=i.uv*2-1;
                float glow=pow(saturate(1-length(p)),2);
                float star=pow(saturate(1-abs(p.x)),16)*saturate(1-abs(p.y)) + pow(saturate(1-abs(p.y)),16)*saturate(1-abs(p.x));
                float mask=_Shape>1.5?pow(saturate(1-abs(p.y)),1.6):(_Shape>0.5?saturate(star+glow*0.4):glow);
                return fixed4(i.color.rgb,i.color.a*mask);
            }
            ENDCG
        }
    }
}

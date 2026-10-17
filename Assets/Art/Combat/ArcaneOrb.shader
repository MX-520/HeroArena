Shader "HeroArena/ArcaneOrb"
{
    Properties { _Phase ("Scaled animation phase", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Phase;
            struct input { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            output vert(input v) { output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o; }
            fixed4 frag(output i):SV_Target
            {
                float2 p=(i.uv*2-1)/(0.965+0.015*sin(_Phase*7));
                float r=length(p);float a=atan2(p.y,p.x);
                float z=sqrt(saturate(1-r*r));
                float light=saturate(dot(float3(p,z),normalize(float3(-0.4,0.55,1))));
                float3 body=lerp(float3(0.055,0.005,0.18),float3(0.38,0.025,0.78),light);
                float flow=sin(a*4+r*14-_Phase*4+sin(a*11-r*6+_Phase*2)*0.8);
                float veins=pow(saturate(1-abs(flow)/0.22),2)*smoothstep(0.18,0.4,r);
                body=lerp(body,float3(0.78,0.18,1),veins*0.75);
                float rim=smoothstep(0.76,0.94,r);
                body=lerp(body,float3(0.74,0.14,1),rim);
                float star=pow(saturate(1-abs(p.x)*1.5),18)*saturate(1-abs(p.y)*1.25)
                    +pow(saturate(1-abs(p.y)*1.5),18)*saturate(1-abs(p.x)*1.25);
                float core=exp(-r*r*48);
                body=lerp(body,float3(1,0.91,1),saturate(star*0.95+core));
                return fixed4(body,1-smoothstep(0.95,1,r));
            }
            ENDCG
        }
    }
}

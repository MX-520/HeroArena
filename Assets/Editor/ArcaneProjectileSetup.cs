using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ArcaneProjectileSetup
{
    private const string PrefabPath="Assets/Prefabs/Combat/EnemyEnergyBolt.prefab";
    public static void Apply()
    {
        var root=PrefabUtility.LoadPrefabContents(PrefabPath);
        Configure(root);
        PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunProjectileVfx();
    }
    public static void Configure(GameObject root)
    {
        var glow=Material("ArcaneGlow",0);var spark=Material("ArcaneSpark",1);var ribbon=Material("ArcaneRibbon",2);
        // Replace the old flat disk visually; retain the root scale and original Projectile collision contract.
        root.GetComponent<SpriteRenderer>().enabled=false;
        var effects=Child(root.transform,"ArcaneEffects");
        effects.localScale=Vector3.one/root.transform.localScale.x;
        var vfx=root.GetComponent<ArcaneBoltVfx>();if(vfx==null) vfx=root.AddComponent<ArcaneBoltVfx>();
        var data=new SerializedObject(vfx);
        const string bodyPath="Assets/Art/Combat/ArcaneOrb.mat";
        var bodyMaterial=AssetDatabase.LoadAssetAtPath<Material>(bodyPath);
        if(bodyMaterial==null) { bodyMaterial=new Material(Shader.Find("HeroArena/ArcaneOrb"));AssetDatabase.CreateAsset(bodyMaterial,bodyPath); }
        var body=Child(effects,"OrbBody");body.localScale=Vector3.one*0.70f;
        var mesh=body.GetComponent<MeshFilter>();if(mesh==null) mesh=body.gameObject.AddComponent<MeshFilter>();
        mesh.sharedMesh=Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        var bodyRenderer=body.GetComponent<MeshRenderer>();if(bodyRenderer==null) bodyRenderer=body.gameObject.AddComponent<MeshRenderer>();
        bodyRenderer.sharedMaterial=bodyMaterial;bodyRenderer.sortingOrder=32;
        data.FindProperty("orbBody").objectReferenceValue=bodyRenderer;
        var oldTrail=effects.Find("ArcaneTrail");
        if(oldTrail!=null && oldTrail.GetComponent<TrailRenderer>()!=null) Object.DestroyImmediate(oldTrail.gameObject);
        var trail=Particles(effects,"ArcaneTrail",glow,64,0.40f,0.53f,0,true,29);
        var emission=trail.emission;emission.rateOverTime=80;
        var main=trail.main;main.startColor=new Color(0.71f,0.23f,1,0.95f);
        data.FindProperty("trail").objectReferenceValue=trail;
        var flame=Particles(effects,"CometFlame",glow,24,0.28f,0.22f,0.24f,true,30);
        emission=flame.emission;emission.rateOverTime=48;
        var flameShape=flame.shape;flameShape.enabled=true;flameShape.shapeType=ParticleSystemShapeType.Circle;flameShape.radius=0.13f;
        var wisps=Particles(effects,"OrbitSparks",spark,32,0.48f,0.15f,0.65f,true,34);
        var shape=wisps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=0.22f;
        emission=wisps.emission;emission.rateOverTime=55;
        var twinkle=wisps.colorOverLifetime;
        var flashes=new Gradient();
        flashes.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(0.62f,0.12f,1),1)},
            new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,0.1f),new GradientAlphaKey(0.08f,0.27f),new GradientAlphaKey(1,0.43f),new GradientAlphaKey(0.1f,0.60f),new GradientAlphaKey(0.9f,0.75f),new GradientAlphaKey(0,1)});
        twinkle.color=flashes;
        var aura=Particles(effects,"CoreGlow",glow,1,3.2f,0.94f,0,true,28);
        emission=aura.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,1)});
        main=aura.main;main.startColor=new Color(0.6f,0.2f,1,0.65f);
        var hot=Particles(effects,"HotCore",glow,1,3.2f,0.43f,0,true,33);
        main=hot.main;main.startColor=new Color(1,0.86f,1,1);
        emission=hot.emission;emission.SetBursts(new[]{new ParticleSystem.Burst(0,1)});
        hot.gameObject.SetActive(false);
        var outer=Arc(effects,"OuterRune",ribbon,0.35f,300,0.035f,new Color(0.75f,0.25f,1,0.95f));
        var inner=Arc(effects,"InnerRune",ribbon,0.30f,190,0.026f,new Color(0.92f,0.58f,1,0.95f));
        data.FindProperty("outerArc").objectReferenceValue=outer;
        data.FindProperty("innerArc").objectReferenceValue=inner;
        var impact=Particles(effects,"ArcaneImpact",spark,28,0.4f,0.11f,2.2f,false,34);
        shape=impact.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=0.04f;
        emission=impact.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,22)});
        var flash=Particles(impact.transform,"ImpactFlash",glow,1,0.19f,1.15f,0,false,33);
        emission=flash.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,1)});
        data.FindProperty("impact").objectReferenceValue=impact;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    private static Material Material(string name,float shape)
    {
        string path="Assets/Art/Combat/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null) { material=new Material(Shader.Find("HeroArena/ArcaneParticle"));material.name=name;AssetDatabase.CreateAsset(material,path); }
        material.SetFloat("_Shape",shape);EditorUtility.SetDirty(material);return material;
    }
    private static Transform Child(Transform parent,string name)
    {
        var child=parent.Find(name);
        if(child==null){child=new GameObject(name).transform;child.SetParent(parent,false);}
        return child;
    }
    private static Transform Arc(Transform parent,string name,Material material,float radius,float angle,float width,Color color)
    {
        var root=Child(parent,name);
        var line=root.GetComponent<LineRenderer>();if(line==null) line=root.gameObject.AddComponent<LineRenderer>();
        line.sharedMaterial=material;line.useWorldSpace=false;line.positionCount=21;
        line.widthMultiplier=width;line.startColor=line.endColor=color;line.numCapVertices=3;line.sortingOrder=32;
        for(int i=0;i<21;i++) { float a=(i*angle/20+40)*Mathf.Deg2Rad;float r=radius+(i%3-1)*0.013f;line.SetPosition(i,new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0)); }
        return root;
    }
    private static ParticleSystem Particles(Transform parent,string name,Material material,int maximum,float lifetime,float size,float speed,bool loop,int order)
    {
        var root=Child(parent,name);var ps=root.GetComponent<ParticleSystem>();if(ps==null) ps=root.gameObject.AddComponent<ParticleSystem>();
        ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.loop=loop;main.duration=loop?3.2f:0.15f;main.playOnAwake=loop;
        main.startLifetime=lifetime;main.startSize=new ParticleSystem.MinMaxCurve(size*0.65f,size);
        main.startSpeed=speed;main.maxParticles=maximum;main.useUnscaledTime=false;
        main.simulationSpace=loop && maximum>1?ParticleSystemSimulationSpace.World:ParticleSystemSimulationSpace.Local;
        main.scalingMode=ParticleSystemScalingMode.Shape;
        main.startColor=new ParticleSystem.MinMaxGradient(new Color(0.65f,0.28f,1,0.8f),new Color(0.97f,0.75f,1,1));
        var shape=ps.shape;shape.enabled=false;
        var emission=ps.emission;emission.enabled=true;emission.rateOverTime=0;
        var col=ps.colorOverLifetime;col.enabled=true;col.color=Fade(Color.white,new Color(0.5f,0.1f,1,0));
        var sol=ps.sizeOverLifetime;sol.enabled=true;sol.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=order;
        renderer.renderMode=ParticleSystemRenderMode.Billboard;
        return ps;
    }
    private static Gradient Fade(Color start,Color end)
    {
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(start,0),new GradientColorKey(end,1)},new[]{new GradientAlphaKey(start.a,0),new GradientAlphaKey(end.a,1)});return gradient;
    }
}

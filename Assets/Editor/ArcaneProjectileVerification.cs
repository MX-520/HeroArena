using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunProjectileVfx()=>Run("projectile-vfx");
    private static IEnumerator ProjectileVfx()
    {
        Object.FindObjectOfType<WaveController>().enabled=false;
        var ui=Object.FindObjectOfType<GameUI>();ui.gameObject.SetActive(false);
        var shader=Shader.Find("HeroArena/ArcaneParticle");
        Require(shader!=null && !ShaderUtil.ShaderHasError(shader),"Arcane shader failed");
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/EnemyEnergyBolt.prefab");
        Require(prefab.transform.localScale==Vector3.one*2.7f,"Bolt scale changed");
        var player=Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(false);
        player.GetComponent<Health>().RestoreFull();
        var source=new GameObject("VfxShooter");
        Vector3 from=player.transform.position+Vector3.up*3;
        var shot=Object.Instantiate(prefab,from,Quaternion.identity).GetComponent<Projectile>();
        shot.Initialize(source.transform,Vector2.down,8,6.5f,LayerMask.GetMask("Player"),LayerMask.GetMask("Default","Destructible"));
        float until=Time.time+0.15f;
        while(Time.time<until) yield return null;
        Require(shot!=null && shot.GetComponentsInChildren<ParticleSystem>().First(p=>p.name=="ArcaneTrail").particleCount>=2,"Missing moving particle trail");
        var ps=shot.GetComponentsInChildren<ParticleSystem>().First(p=>p.name=="OrbitSparks");
        Require(ps.particleCount>0,"Flight sparks did not emit");
        var pos=shot.transform.position;float particleTime=ps.time;
        var trail=shot.GetComponentsInChildren<ParticleSystem>().First(p=>p.name=="ArcaneTrail");float trailTime=trail.time;
        Time.timeScale=0;
        double wait=EditorApplication.timeSinceStartup+0.15;
        while(EditorApplication.timeSinceStartup<wait) yield return null;
        Require(shot.transform.position==pos && Mathf.Abs(ps.time-particleTime)<0.001f,"Pause did not freeze projectile particles");
        Require(Mathf.Abs(trail.time-trailTime)<0.001f && trail.particleCount>0,"Pause did not preserve particle tail");
        CaptureArcane("flight");
        Time.timeScale=1;
        until=Time.time+1;
        while(shot!=null && Time.time<until) yield return null;
        Require(shot==null && player.GetComponent<Health>().CurrentHP==92,"VFX changed actual bolt damage/hit");
        yield return null;
        var impact=Object.FindObjectsOfType<ParticleSystem>().FirstOrDefault(p=>p.name=="ArcaneImpact" && p.transform.parent==null);
        Require(impact!=null && impact.particleCount>0,"Impact sparks missing");
        until=Time.time+0.08f;while(Time.time<until) yield return null;
        CaptureArcane("impact");
        until=Time.time+0.85f;
        while(Time.time<until) yield return null;
        Require(!Object.FindObjectsOfType<ParticleSystem>().Any(p=>p.name=="ArcaneImpact"),"Impact effect leaked");
        Require(!Object.FindObjectsOfType<ParticleSystem>().Any(t=>t.name=="ArcaneTrail"),"Detached trail leaked");
        var expired=Object.Instantiate(prefab,from,Quaternion.identity).GetComponent<Projectile>();
        expired.Initialize(source.transform,Vector2.right,8,0,LayerMask.GetMask("Player"),0,0.05f);
        until=Time.time+0.6f;
        while(Time.time<until) yield return null;
        Require(expired==null && !Object.FindObjectsOfType<ParticleSystem>().Any(t=>t.name=="ArcaneTrail"),"Expired VFX did not clean up");
        Object.Destroy(source);
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        // Existing tests verify real enemy 1.2-second attacks and the player's original projectile contract.
        var ranged=FeedbackEnemies();while(ranged.MoveNext()) yield return ranged.Current;
        var shooting=Shooting();while(shooting.MoveNext()) yield return shooting.Current;
    }
    private static void CaptureArcane(string name)
    {
        var camera=Camera.main;var previous=camera.targetTexture;var active=RenderTexture.active;
        var oldPosition=camera.transform.position;float oldSize=camera.orthographicSize;
        var target=RenderTexture.GetTemporary(1280,720,24);var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
            File.WriteAllBytes("Logs/arcane-"+name+".png",pixels.EncodeToPNG());
            var bolt=Object.FindObjectsOfType<Projectile>().FirstOrDefault(p=>p.GetComponent<ArcaneBoltVfx>()!=null);
            Vector3 focus=bolt!=null?bolt.transform.position:Object.FindObjectOfType<PlayerHealth>().transform.position+Vector3.up*0.6f;
            camera.transform.position=new Vector3(focus.x,focus.y,oldPosition.z);camera.orthographicSize=1.4f;
            camera.Render();pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
            File.WriteAllBytes("Logs/arcane-"+name+"-detail.png",pixels.EncodeToPNG());
        }
        finally { camera.transform.position=oldPosition;camera.orthographicSize=oldSize;camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);Object.Destroy(pixels); }
    }
}

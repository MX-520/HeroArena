using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunAudioUI()=>Run("audio-ui");
    public static void RunAudioAll()=>Run("audio-all");
    static IEnumerator AudioUI()
    {
        double until=EditorApplication.timeSinceStartup+1.2;
        while(EditorApplication.timeSinceStartup<until) yield return null;
        var audio=GameAudio.Current;var flow=Object.FindObjectOfType<GameFlow>();
        Require(audio!=null && Object.FindObjectsOfType<GameAudio>().Length==1,"Audio singleton missing/duplicated");
        Require(audio.GetComponentsInChildren<AudioSource>().Length==14,"Voice pool not bounded to 14 sources");
        Require(Object.FindObjectsOfType<AudioListener>().Length==1,"Expected one listener");
        Require(audio.MusicSource.clip.name=="ForestWhisper" && audio.MusicSource.isPlaying,"Menu music not playing");
        audio.enabled=false;Require(!AudioListener.pause,"Disabling audio left the global listener paused");
        audio.enabled=true;yield return null;
        Require(GameAudio.Current==audio && audio.MusicSource.isPlaying && AudioListener.pause,"Re-enabled audio did not restore menu playback");
        int menuSample=audio.MusicSource.timeSamples;
        var button=FindButton("StartButton");
        var pointer=new PointerEventData(EventSystem.current);
        int hover=audio.Played(GameSound.UIHover),click=audio.Played(GameSound.UIClick);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
        Require(audio.Played(GameSound.UIHover)==hover+1 && audio.Played(GameSound.UIClick)==click+1,"Pointer feedback audio missing at timeScale zero");
        Require(!GameAudio.Play(GameSound.PlayerShot),"Gameplay cue allowed in menu");
        until=EditorApplication.timeSinceStartup+.2;while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(audio.MusicSource.timeSamples>menuSample,"Menu music frozen with game time");
        button.onClick.Invoke();
        Require(audio.MusicSource.clip.name=="BattleThemeII","Battle music not switched");
        Object.FindObjectOfType<PlayerHealth>().GetComponent<DamageReceiver>().SetInvincible(true);
        until=EditorApplication.timeSinceStartup+1.2;while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(audio.MusicSource.isPlaying && audio.MusicSource.volume>0,"Battle source silent/stopped");
        float battleVolume=audio.MusicSource.volume;
        Require(GameAudio.Play(GameSound.MagicImpact),"World cue failed while playing");
        var world=audio.GetComponentsInChildren<AudioSource>().First(s=>s.clip!=null && s.clip.name=="MagicImpact");
        flow.Pause();int frozen=world.timeSamples;int musicSample=audio.MusicSource.timeSamples;
        Require(AudioListener.pause,"Pause did not pause listener");
        until=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(world.timeSamples==frozen,"Gameplay audio advanced during pause");
        Require(audio.MusicSource.timeSamples>musicSample && audio.MusicSource.volume<battleVolume,"Paused music not continuing/ducking");
        Require(!GameAudio.Play(GameSound.PlayerShot) && GameAudio.Play(GameSound.UIClick),"Pause did not separate UI and world audio");
        flow.Resume();Require(!AudioListener.pause,"Listener stayed paused on resume");
        float master=audio.MasterVolume,music=audio.MusicVolume,sfx=audio.SfxVolume;
        audio.SetVolumes(-1,2,-1,false);yield return null;
        Require(audio.MasterVolume==0 && audio.MusicVolume==1 && audio.SfxVolume==0,"Volume clamp failed");
        Require(audio.GetComponentsInChildren<AudioSource>().All(s=>s.volume==0),"Master mute failed");
        audio.SetVolumes(master,music,sfx,false);
        flow.MainMenu();yield return null;
        while(GameAudio.Current==null || Object.FindObjectOfType<GameFlow>().State!=GameFlow.ScreenState.Menu) yield return null;
        Require(Object.FindObjectsOfType<GameAudio>().Length==1 && GameAudio.Current.GetComponentsInChildren<AudioSource>().Length==14,"Reload leaked audio voices");
        var ui=UI();while(ui.MoveNext()) yield return ui.Current;
        Require(Object.FindObjectsOfType<GameAudio>().Length==1 && GameAudio.Current.MusicSource.clip.name=="ForestWhisper","Full UI flow leaked/misrouted music");
        Debug.Log("PASS music, bounded voices, timeScale-zero UI, pause freeze/duck, mute, reload and full three-wave UI");
    }
    static IEnumerator AudioAll()
    {
        var check=AudioUI();while(check.MoveNext()) yield return check.Current;
        var flow=Object.FindObjectOfType<GameFlow>();flow.BeginRun();flow.Waves.enabled=false;
        var audio=GameAudio.Current;
        var player=Object.FindObjectOfType<PlayerHealth>();
        var health=player.GetComponent<Health>();var receiver=player.GetComponent<DamageReceiver>();
        var effects=player.GetComponent<PlayerEffects>();var movement=player.GetComponent<PlayerMovement>();
        Require(player.GetComponent<ActorAudio>()!=null,"Player health audio not configured");
        foreach(string name in new[]{"Gobling","GoblinRunner","GoblinArcher"})
            Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/"+name+".prefab").GetComponent<ActorAudio>()!=null,"Enemy prefab audio missing: "+name);
        check=FeedbackInput();while(check.MoveNext()) yield return check.Current;
        Require(audio.Played(GameSound.PlayerShot)==1 && audio.Played(GameSound.PlayerSwing)==1,"Multishot/right-click sound missing or repeated per projectile");
        effects.Clear();player.GetComponent<PlayerAttack>().CancelAttack();movement.ExitHurt();
        double until=EditorApplication.timeSinceStartup+.6;while(EditorApplication.timeSinceStartup<until) yield return null;
        check=Ranged();while(check.MoveNext()) yield return check.Current;
        Require(audio.Played(GameSound.MagicCast)>0 && audio.Played(GameSound.MagicImpact)>0,"Real archer cast/impact sound missing");
        Require(audio.Played(GameSound.EnemyHurt)>0 && audio.Played(GameSound.EnemyDeath)>0,"Enemy hurt/death sound missing");
        until=EditorApplication.timeSinceStartup+.6;while(EditorApplication.timeSinceStartup<until) yield return null;
        movement.ExitHurt();
        var field=typeof(PlayerMovement).GetField("moveInput",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
        field.SetValue(movement,Vector2.right);
        until=EditorApplication.timeSinceStartup+.32;while(EditorApplication.timeSinceStartup<until) yield return null;
        field.SetValue(movement,Vector2.zero);yield return null;
        int steps=audio.Played(GameSound.Footstep);Require(steps>0,"Moving player did not produce footsteps");
        until=EditorApplication.timeSinceStartup+.3;while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(audio.Played(GameSound.Footstep)==steps,"Idle player produced footsteps");
        var roll=(IEnumerator)typeof(PlayerMovement).GetMethod("RollCoroutine",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(movement,null);
        movement.StartCoroutine(roll);Require(audio.Played(GameSound.Roll)==1,"Roll sound missing");
        until=EditorApplication.timeSinceStartup+.4;while(EditorApplication.timeSinceStartup<until) yield return null;
        var potion=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Heal.prefab"),player.transform.position+Vector3.up*3,Quaternion.identity).GetComponent<Pickup>();
        Require(potion.TryCollect(effects) && !potion.TryCollect(effects) && audio.Played(GameSound.Heal)==1,"Successful heal not exactly once");
        potion=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Heal.prefab"),player.transform.position+Vector3.up*3,Quaternion.identity).GetComponent<Pickup>();
        Require(!potion.TryCollect(effects) && audio.Played(GameSound.Heal)==1,"Rejected heal played audio");Object.Destroy(potion.gameObject);
        var pickup=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Shield.prefab"),player.transform.position+Vector3.up*3,Quaternion.identity).GetComponent<Pickup>();
        Require(pickup.TryCollect(effects) && audio.Played(GameSound.Pickup)==1,"Buff pickup sound missing");
        int hurt=audio.Played(GameSound.PlayerHurt);receiver.TakeDamage(5);
        Require(audio.Played(GameSound.PlayerHurt)==hurt && health.CurrentHP==100,"Shield caused hurt sound/damage");
        effects.Clear();receiver.SetInvincible(true);receiver.TakeDamage(5);
        Require(audio.Played(GameSound.PlayerHurt)==hurt,"Invincible damage caused hurt sound");
        receiver.SetInvincible(false);receiver.TakeDamage(1);
        Require(audio.Played(GameSound.PlayerHurt)==hurt+1,"Actual player hurt sound missing");
        foreach(var kind in new[]{ActorAudio.ActorKind.WoodObstacle,ActorAudio.ActorKind.StoneObstacle})
        {
            until=EditorApplication.timeSinceStartup+.5;while(EditorApplication.timeSinceStartup<until) yield return null;
            var obstacle=Object.FindObjectsOfType<DestructibleObstacle>().First(o=>new SerializedObject(o.GetComponent<ActorAudio>()).FindProperty("kind").enumValueIndex==(int)kind);
            var sound=kind==ActorAudio.ActorKind.WoodObstacle?GameSound.WoodBreak:GameSound.StoneBreak;
            int before=audio.Played(sound);obstacle.GetComponent<DamageReceiver>().TakeDamage(1000);
            Require(audio.Played(sound)==before+1,"Obstacle break sound missing: "+kind);
        }
        until=EditorApplication.timeSinceStartup+.6;while(EditorApplication.timeSinceStartup<until) yield return null;
        int cast=audio.Played(GameSound.MagicCast);for(int i=0;i<100;i++) GameAudio.Play(GameSound.MagicCast);
        Require(audio.Played(GameSound.MagicCast)==cast+1 && audio.GetComponentsInChildren<AudioSource>().Length==14,"Spam bypassed cue cooldown/voice bound");
        health.TakeDamage(1000);
        Require(flow.State==GameFlow.ScreenState.Defeat && audio.Played(GameSound.Defeat)==1 && audio.Played(GameSound.PlayerDeath)==1,"Fatal damage lost result/death audio");
        flow.MainMenu();yield return null;
        while(GameAudio.Current==null || Object.FindObjectOfType<GameFlow>().State!=GameFlow.ScreenState.Menu) yield return null;
        Require(Object.FindObjectsOfType<GameAudio>().Length==1 && GameAudio.Current.GetComponentsInChildren<AudioSource>().Length==14,"Combat reload leaked audio");
        Debug.Log("PASS actual multishot, right-click melee, archer cast/impact, actor damage/death, movement/roll, pickups, invincibility, two obstacle materials, spam bound and reload");
    }
}

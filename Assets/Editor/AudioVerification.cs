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
    }
}

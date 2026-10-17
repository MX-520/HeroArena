using System.Collections.Generic;
using UnityEngine;

/// <summary>Scene-owned audio, with two music channels and bounded UI/game voices. No persistent clone on reload.</summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(GameFlow))]
public sealed class GameAudio : MonoBehaviour
{
    public static GameAudio Current { get; private set; }
    [SerializeField] private GameAudioLibrary library;
    private GameFlow flow;
    private AudioSource[] music=new AudioSource[2];
    private readonly float[] musicGain=new float[2];
    private sealed class Voice { public AudioSource source;public GameSound sound;public float gain;public int priority; }
    private readonly Voice[] voices=new Voice[12];
    private readonly Dictionary<GameSound,GameAudioLibrary.Cue> cues=new Dictionary<GameSound,GameAudioLibrary.Cue>();
    private readonly float[] nextAllowed=new float[(int)GameSound.Count];
    private readonly int[] played=new int[(int)GameSound.Count];
    private readonly System.Random variation=new System.Random(17017);
    private int activeMusic;
    private int lastWave;
    private GameFlow.ScreenState state=(GameFlow.ScreenState)(-1);
    private float duck=1;
    private float actionDuck=1;
    private float actionDuckUntil;
    private Camera audioCamera;
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public AudioSource MusicSource=>music[activeMusic];
    public bool MusicEnabled=>library!=null && library.musicEnabled;
    public int Played(GameSound sound)=>played[(int)sound];
    public static bool Play(GameSound sound,Vector3 position=default)=>Current!=null && Current.PlayCue(sound,position);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Current=null;AudioListener.pause=false; }
    private void Awake()
    {
        if(Current!=null && Current!=this) { Destroy(this);return; }
        Current=this;flow=GetComponent<GameFlow>();audioCamera=Camera.main;
        if(library==null) { Debug.LogError("GameAudio requires its audio library",this);enabled=false;return; }
        foreach(var cue in library.cues) cues[cue.sound]=cue;
        for(int i=0;i<2;i++) {music[i]=Source("Music"+i,true);music[i].loop=true;}
        for(int i=0;i<voices.Length;i++) voices[i]=new Voice{source=Source(i<3?"InterfaceVoice"+i:"WorldVoice"+i,i<3)};
        SetVolumes(PlayerPrefs.GetFloat("HeroArena.Audio.Master",0.8f),PlayerPrefs.GetFloat("HeroArena.Audio.Music",0.35f),PlayerPrefs.GetFloat("HeroArena.Audio.Sfx",0.75f),false);
    }
    private AudioSource Source(string name,bool unpaused)
    {
        var child=new GameObject(name);child.transform.SetParent(transform,false);
        var source=child.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
        source.ignoreListenerPause=unpaused;source.dopplerLevel=0;return source;
    }
    private void OnEnable()
    {
        if(Current==null && music[0]!=null) {Current=this;state=(GameFlow.ScreenState)(-1);}
        if(Current!=this || flow==null) return;
        flow.Changed+=Refresh;flow.Waves.Changed+=WaveChanged;
        Refresh();
    }
    private void Start() { Refresh(); }
    private void OnDisable()
    {
        if(flow!=null) {flow.Changed-=Refresh;flow.Waves.Changed-=WaveChanged;}
        if(Current!=this) return;
        foreach(var source in music) if(source!=null) source.Stop();
        foreach(var voice in voices) if(voice?.source!=null) voice.source.Stop();
        AudioListener.pause=false;Current=null;
    }
    public void SetVolumes(float master,float musicLevel,float sfx,bool persist=true)
    {
        MasterVolume=Mathf.Clamp01(master);MusicVolume=Mathf.Clamp01(musicLevel);SfxVolume=Mathf.Clamp01(sfx);
        if(!persist) return;
        PlayerPrefs.SetFloat("HeroArena.Audio.Master",MasterVolume);
        PlayerPrefs.SetFloat("HeroArena.Audio.Music",MusicVolume);
        PlayerPrefs.SetFloat("HeroArena.Audio.Sfx",SfxVolume);PlayerPrefs.Save();
    }
    private void Refresh()
    {
        if(flow.State==state || library==null) return;
        state=flow.State;
        AudioListener.pause=state!=GameFlow.ScreenState.Playing;
        if(state==GameFlow.ScreenState.Paused) return;
        SwitchMusic(state==GameFlow.ScreenState.Playing?library.battleMusic:library.menuMusic);
        if(state!=GameFlow.ScreenState.Playing)
            for(int i=3;i<voices.Length;i++) voices[i].source.Stop();
        if(state==GameFlow.ScreenState.Victory) PlayCue(GameSound.Victory,Vector3.zero);
        if(state==GameFlow.ScreenState.Defeat) PlayCue(GameSound.Defeat,Vector3.zero);
    }
    private void WaveChanged()
    {
        if(flow.Waves.State!=WaveController.ChallengeState.Fighting || flow.Waves.CurrentWave==lastWave) return;
        lastWave=flow.Waves.CurrentWave;PlayCue(GameSound.WaveStart,Vector3.zero);
    }
    private void SwitchMusic(AudioClip clip)
    {
        if(!MusicEnabled || clip==null) return;
        if(music[activeMusic].clip==clip) {if(!music[activeMusic].isPlaying) music[activeMusic].Play();return;}
        activeMusic=1-activeMusic;music[activeMusic].clip=clip;music[activeMusic].volume=0;musicGain[activeMusic]=0;
        music[activeMusic].Play();
    }
    private void Update()
    {
        float target=state==GameFlow.ScreenState.Paused?0.25f:state==GameFlow.ScreenState.Playing?1:0.7f;
        duck=Mathf.MoveTowards(duck,target,Time.unscaledDeltaTime*2);
        float actionTarget=state==GameFlow.ScreenState.Playing && Time.unscaledTime<actionDuckUntil?0.68f:1;
        actionDuck=Mathf.MoveTowards(actionDuck,actionTarget,Time.unscaledDeltaTime*(actionTarget<actionDuck?12:2.5f));
        for(int i=0;i<2;i++)
        {
            if(!MusicEnabled) {music[i].Stop();music[i].clip=null;music[i].volume=0;musicGain[i]=0;continue;}
            musicGain[i]=Mathf.MoveTowards(musicGain[i],i==activeMusic?1:0,Time.unscaledDeltaTime/0.8f);
            music[i].volume=MasterVolume*MusicVolume*library.musicGain*musicGain[i]*duck*actionDuck;
            if(i!=activeMusic && musicGain[i]==0 && music[i].clip!=null) {music[i].Stop();music[i].clip=null;}
        }
        foreach(var voice in voices) voice.source.volume=MasterVolume*SfxVolume*library.sfxGain*voice.gain;
    }
    private bool PlayCue(GameSound sound,Vector3 position)
    {
        if(!isActiveAndEnabled || !cues.TryGetValue(sound,out var cue) || cue.clips==null || cue.clips.Length==0) return false;
        if(!cue.unpaused && (flow.State!=GameFlow.ScreenState.Playing || Time.timeScale<=0)) return false;
        int key=(int)sound;if(Time.unscaledTime<nextAllowed[key]) return false;
        int begin=cue.unpaused?0:3,end=cue.unpaused?3:voices.Length;
        Voice slot=null;int same=0;
        for(int i=begin;i<end;i++)
        {
            if(voices[i].source.isPlaying && voices[i].sound==sound) same++;
            if(slot==null && !voices[i].source.isPlaying) slot=voices[i];
        }
        if(same>=cue.voices) return false;
        if(slot==null)
        {
            for(int i=begin;i<end;i++)
                if(voices[i].priority<cue.priority && (slot==null || voices[i].priority<slot.priority)) slot=voices[i];
            if(slot==null) return false;
        }
        var clip=cue.clips[variation.Next(cue.clips.Length)];if(clip==null) return false;
        slot.source.Stop();slot.sound=sound;slot.priority=cue.priority;slot.gain=cue.gain;
        slot.source.clip=clip;slot.source.pitch=Mathf.Lerp(cue.pitch.x,cue.pitch.y,(float)variation.NextDouble());
        slot.source.panStereo=cue.unpaused || audioCamera==null?0:Mathf.Clamp((position.x-audioCamera.transform.position.x)/12,-0.65f,0.65f);
        slot.source.volume=MasterVolume*SfxVolume*library.sfxGain*slot.gain;slot.source.Play();
        if(!cue.unpaused && cue.priority>=2) actionDuckUntil=Time.unscaledTime+0.16f;
        nextAllowed[key]=Time.unscaledTime+cue.cooldown;played[key]++;return true;
    }
}

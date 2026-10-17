using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AudioIntegrationSetup
{
    const string Root="Assets/Audio/Candidates/";
    public static void Apply()
    {
        try
        {
            var library=AssetDatabase.LoadAssetAtPath<GameAudioLibrary>("Assets/Audio/HeroArenaAudio.asset");
            if(library==null) {library=ScriptableObject.CreateInstance<GameAudioLibrary>();AssetDatabase.CreateAsset(library,"Assets/Audio/HeroArenaAudio.asset");}
            library.menuMusic=Clip("ThirdParty/CleytonKauffman/ForestWhisper.ogg");
            library.battleMusic=Clip("ThirdParty/CleytonKauffman/BattleThemeII.ogg");
            var cues=new List<GameAudioLibrary.Cue>();
            Add(cues,GameSound.UIHover,.12f,true,"interface/select_001");
            Add(cues,GameSound.UIClick,.3f,true,"interface/click_001");
            Add(cues,GameSound.UIBack,.28f,true,"interface/back_001");
            Add(cues,GameSound.WaveStart,.3f,false,"interface/confirmation_001");
            Add(cues,GameSound.PlayerShot,.22f,false,"PlayerShot");
            Add(cues,GameSound.PlayerSwing,.25f,false,"rpg/knifeSlice","rpg/knifeSlice2");
            Add(cues,GameSound.EnemySwing,.18f,false,"rpg/knifeSlice2");
            Add(cues,GameSound.Roll,.24f,false,"rpg/cloth1");
            Add(cues,GameSound.Footstep,.10f,false,"impact/footstep_grass_000","impact/footstep_grass_001","impact/footstep_grass_002");
            Add(cues,GameSound.PlayerHurt,.45f,false,"impact/impactPunch_heavy_000");
            Add(cues,GameSound.EnemyHurt,.24f,false,"impact/impactPunch_medium_000");
            Add(cues,GameSound.EnemyDeath,.25f,true,"impact/impactPunch_heavy_000");
            Add(cues,GameSound.PlayerDeath,.3f,true,"impact/impactPunch_heavy_000");
            Add(cues,GameSound.WoodBreak,.4f,false,"impact/impactWood_heavy_000");
            Add(cues,GameSound.StoneBreak,.3f,false,"impact/impactPlate_medium_000");
            Add(cues,GameSound.Pickup,.25f,false,"rpg/handleCoins");
            Add(cues,GameSound.Heal,.35f,false,"HealPickup");
            Add(cues,GameSound.MagicCast,.3f,false,"MagicCast");
            Add(cues,GameSound.MagicImpact,.34f,false,"MagicImpact");
            Add(cues,GameSound.Victory,.5f,true,"Victory");
            Add(cues,GameSound.Defeat,.45f,true,"Defeat");
            library.cues=cues.ToArray();EditorUtility.SetDirty(library);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            var flow=UnityEngine.Object.FindObjectOfType<GameFlow>();
            var audio=flow.GetComponent<GameAudio>();if(audio==null) audio=flow.gameObject.AddComponent<GameAudio>();
            var serialized=new SerializedObject(audio);serialized.FindProperty("library").objectReferenceValue=library;serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("PASS audio setup: two music clips, 21 cue mappings");EditorApplication.Exit(0);
        }
        catch(Exception e) {Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static AudioClip Clip(string path)
    {
        var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+path);
        if(clip==null) throw new Exception("Missing audio: "+path);return clip;
    }
    static void Add(List<GameAudioLibrary.Cue> list,GameSound sound,float gain,bool unpaused,params string[] files)
    {
        var clips=new AudioClip[files.Length];
        for(int i=0;i<files.Length;i++)
        {
            int slash=files[i].IndexOf('/');
            clips[i]=Clip(slash<0?"Original/"+files[i]+".wav":"ThirdParty/Kenney/kenney-"+files[i].Substring(0,slash)+"/"+files[i].Substring(slash+1)+".ogg");
        }
        list.Add(new GameAudioLibrary.Cue{sound=sound,gain=gain,clips=clips,unpaused=unpaused,
            priority=sound==GameSound.Victory || sound==GameSound.Defeat?5:
                sound==GameSound.PlayerDeath?4:sound==GameSound.PlayerHurt || sound==GameSound.UIClick || sound==GameSound.UIBack?3:
                sound==GameSound.Footstep?0:sound==GameSound.EnemyDeath || sound==GameSound.EnemyHurt || sound==GameSound.EnemySwing?1:2,
            cooldown=sound==GameSound.Footstep?.09f:.04f,voices=sound==GameSound.Footstep?2:3});
    }
}

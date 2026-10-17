using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Align ground contact with the four-pillar arena center; leave collision/navigation data intact.</summary>
public static class CenteredSpawnSetup
{
    public static Vector2 ArenaCenter()
    {
        var pillars=UnityEngine.Object.FindObjectsOfType<PillarNavigation>();
        if(pillars.Length!=4) throw new Exception("Expected exactly four arena pillars");
        return pillars.Select(p=>(Vector2)p.GetComponent<BoxCollider2D>().bounds.center).Aggregate(Vector2.zero,(a,b)=>a+b)/4;
    }
    public static void Apply()
    {
        try
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            var player=UnityEngine.Object.FindObjectOfType<PlayerHealth>();
            var respawn=GameObject.Find("RespawnPoint").transform;
            Vector2 center=ArenaCenter();
            Vector2 footOffset=(Vector2)(player.GetComponent<Collider2D>().bounds.center-player.transform.position);
            Vector3 position=player.transform.position;position.x=center.x-footOffset.x;position.y=center.y-footOffset.y;
            player.transform.position=position;respawn.position=position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var library=AssetDatabase.LoadAssetAtPath<GameAudioLibrary>("Assets/Audio/HeroArenaAudio.asset");
            library.musicEnabled=false;EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
            System.IO.File.WriteAllText("Logs/centered-spawn-setup.txt",$"Center={center:F4}\nPlayer/Respawn={position:F4}\nBackground music disabled; SFX retained\n");
            Debug.Log("PASS centered player/respawn and disabled background music");EditorApplication.Exit(0);
        }
        catch(Exception e) {Debug.LogException(e);EditorApplication.Exit(1);}
    }
}

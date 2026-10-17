using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ActorAudioSetup
{
    public static void Apply()
    {
        try
        {
            ConfigurePrefab("Assets/Prefabs/Player/Bafu.prefab",ActorAudio.ActorKind.Player);
            foreach(string path in new[]{"Gobling","GoblinRunner","GoblinArcher"})
                ConfigurePrefab("Assets/Prefabs/Enemy/"+path+".prefab",ActorAudio.ActorKind.Enemy);
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs/Environment/Obstacles"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var root=PrefabUtility.LoadPrefabContents(path);
                if(root.GetComponent<DestructibleObstacle>()!=null)
                {
                    Configure(root,ObstacleKind(root));PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Configure(UnityEngine.Object.FindObjectOfType<PlayerMovement>().gameObject,ActorAudio.ActorKind.Player);
            foreach(var obstacle in UnityEngine.Object.FindObjectsOfType<DestructibleObstacle>()) Configure(obstacle.gameObject,ObstacleKind(obstacle.gameObject));
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("PASS actor audio setup: player, three enemy prefabs and destructibles");EditorApplication.Exit(0);
        }
        catch(Exception e) {Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static ActorAudio.ActorKind ObstacleKind(GameObject root)
    {
        // Verified atlas entries: crates=15, barrel=17, logs=18, stump=23.
        var renderer=root.GetComponentInChildren<SpriteRenderer>();
        string name=renderer!=null && renderer.sprite!=null?renderer.sprite.name:"";
        return name.EndsWith("_15") || name.EndsWith("_17") || name.EndsWith("_18") || name.EndsWith("_23")?ActorAudio.ActorKind.WoodObstacle:ActorAudio.ActorKind.StoneObstacle;
    }
    static void ConfigurePrefab(string path,ActorAudio.ActorKind kind)
    {
        var root=PrefabUtility.LoadPrefabContents(path);Configure(root,kind);
        PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
    }
    static void Configure(GameObject root,ActorAudio.ActorKind kind)
    {
        var audio=root.GetComponent<ActorAudio>();if(audio==null) audio=root.AddComponent<ActorAudio>();
        var serialized=new SerializedObject(audio);serialized.FindProperty("kind").enumValueIndex=(int)kind;serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

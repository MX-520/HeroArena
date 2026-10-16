using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit local editor commands; never modify scenes during import.
[InitializeOnLoad]
public static class HeroArenaEditorBridge
{
    static HeroArenaEditorBridge() { EditorApplication.update += Poll; }
    private static void Poll()
    {
        const string request = "Logs/editor-command.txt";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string command = File.ReadAllText(request).Trim();
        File.Delete(request);
        if (command != "close" && command != "open-scene") return;
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () => Complete(command);
    }
    private static void Complete(string command)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += () => Complete(command);
            return;
        }
        if (command == "close")
        {
            EditorSceneManager.SaveOpenScenes();
            File.WriteAllText("Logs/editor-closed.txt", "Graceful editor exit requested");
            EditorApplication.Exit(0);
            return;
        }
        const string path = "Assets/Scenes/SampleScene.unity";
        bool dirty = Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty);
        Scene scene = SceneManager.GetSceneByPath(path);
        if (!scene.IsValid() || !scene.isLoaded)
            scene = EditorSceneManager.OpenScene(path, dirty ? OpenSceneMode.Additive : OpenSceneMode.Single);
        SceneManager.SetActiveScene(scene);
        var player = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PlayerMovement>(true)).FirstOrDefault();
        if (player != null)
        {
            Selection.activeGameObject = player.gameObject;
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.in2DMode = true;
                view.LookAt(player.transform.position, Quaternion.identity, 8f);
                view.Repaint();
            }
        }
        EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        File.WriteAllText("Logs/editor-open-scene.txt", "Opened: " + scene.path + "\nLoaded: " + scene.isLoaded + "\nRoots: " + scene.rootCount + "\nPreserved dirty scenes: " + dirty + "\n" + string.Join("\n", scene.GetRootGameObjects().Select(root => root.name)));
        Debug.Log("HeroArena scene opened: " + scene.path + ", roots=" + scene.rootCount);
    }
}

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

// Local editor bridge for graceful automation, without terminating the process.
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
        if (command != "close") return;
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.SaveOpenScenes();
            File.WriteAllText("Logs/editor-closed.txt", "Graceful editor exit requested");
            EditorApplication.Exit(0);
        };
    }
}

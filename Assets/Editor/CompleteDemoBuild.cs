using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class CompleteDemoBuild
{
    public static void BuildWindows()
    {
        Directory.CreateDirectory("Builds/HeroArena");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = "Builds/HeroArena/HeroArena.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        File.WriteAllText("Logs/windows-build.txt", $"{report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}");
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CompleteDemoBuild
{
    public static void BuildWindows()
    {
        bool succeeded = BuildWindowsLive();
        if (Application.isBatchMode) EditorApplication.Exit(succeeded ? 0 : 1);
    }

    // Save a copy of the live scene; never save, close or clean the user's scene.
    public static bool BuildWindowsLive()
    {
        const string source = "Assets/Scenes/SampleScene.unity";
        string stageFolder = "Assets/__DemoBuildStaging_" + Guid.NewGuid().ToString("N");
        string stage = stageFolder + "/SampleScene.unity";
        string output = "Builds/HeroArena-Demo-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/windows-build.txt", "Preparing\nOutput: " + output);
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building the demo.");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(stageFolder));
            Scene live = SceneManager.GetSceneByPath(source);
            if (live.IsValid() && live.isLoaded)
            {
                if (!EditorSceneManager.SaveScene(live, stage, true))
                    throw new IOException("Could not save a build-only copy of SampleScene.");
            }
            else File.Copy(source, stage);

            int excluded = 0;
            string sceneText = File.ReadAllText(stage);
            sceneText = Regex.Replace(sceneText, @"(?ms)^--- !u!1 &[^\r\n]+\r?\nGameObject:.*?(?=^--- !u!|\z)", match =>
            {
                if (!Regex.IsMatch(match.Value, @"(?m)^  m_Name: Test(?:Player|Enemy(?:_0[123])?)\r?$")) return match.Value;
                excluded++;
                return Regex.Replace(match.Value, @"(?m)^  m_TagString: [^\r\n]+", "  m_TagString: EditorOnly");
            });
            File.WriteAllText(stage, sceneText);
            AssetDatabase.ImportAsset(stage, ImportAssetOptions.ForceSynchronousImport);
            Directory.CreateDirectory(output);
            File.WriteAllText("Logs/windows-build.txt", $"Building\nOutput: {output}\nExcluded preview objects: {excluded}");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { stage },
                locationPathName = output + "/HeroArena.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.DetailedBuildReport
            });
            var packed = report.packedAssets.SelectMany(a => a.contents)
                .Select(a => a.sourceAssetPath.Replace('\\', '/')).Where(p => !string.IsNullOrEmpty(p)).Distinct().OrderBy(p => p).ToArray();
            File.WriteAllLines("Logs/windows-build-assets.txt", packed);
            string[] forbidden = { "Assets/Role/Test/", "Assets/Animations/TestPlayer/", "Assets/Animations/TestEnemy/", "Assets/Art/Characters/Generated/", "Assets/Role/Skeleton/" };
            var leaked = packed.Where(p => forbidden.Any(prefix => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))).ToArray();
            bool succeeded = report.summary.result == BuildResult.Succeeded && leaked.Length == 0;
            File.WriteAllText("Logs/windows-build.txt", $"{(succeeded ? "Succeeded" : "Failed")}\nUnity result: {report.summary.result}\nOutput: {output}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nExcluded preview objects: {excluded}\nPacked source assets: {packed.Length}\nForbidden assets: {leaked.Length}\n" + string.Join("\n", leaked));
            return succeeded;
        }
        catch (Exception exception)
        {
            File.WriteAllText("Logs/windows-build.txt", "Failed\nOutput: " + output + "\n" + exception);
            Debug.LogException(exception);
            return false;
        }
        finally
        {
            if (AssetDatabase.IsValidFolder(stageFolder)) AssetDatabase.DeleteAsset(stageFolder);
        }
    }
}

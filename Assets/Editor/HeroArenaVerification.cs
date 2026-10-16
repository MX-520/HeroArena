using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

// Batch entry point: -executeMethod HeroArenaVerification.RunRunnerChecks
[InitializeOnLoad]
public static class HeroArenaVerification
{
    private const string Key = "HeroArena.Verification";
    private static double deadline;
    private static IEnumerator routine;
    private static string mode;
    private static bool runtimeError;

    static HeroArenaVerification()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
    }

    public static void RunRunnerChecks()
    {
        SessionState.SetString(Key, "runner");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.isPlaying = true;
    }

    public static void RunWaveChecks()
    {
        CompleteDemoVerification.RunWaves();
    }

    public static void RunVisualChecks()
    {
        SessionState.SetString(Key, "visual");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || SessionState.GetString(Key, "") == "")
            return;
        mode = SessionState.GetString(Key, "");
        deadline = EditorApplication.timeSinceStartup + 50;
        routine = CheckChallenge(mode != "runner");
        Application.logMessageReceived += CaptureError;
        EditorApplication.update += Tick;
    }

    private static void CaptureError(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            runtimeError = true;
    }

    private static void Tick()
    {
        try
        {
            Require(EditorApplication.timeSinceStartup < deadline, "Verification timeout");
            if (routine.MoveNext())
                return;
            Require(!runtimeError, "Unity logged a runtime error; inspect the log");
            Finish(true, mode != "runner"
                ? "Wave checks passed: 3 normal then 2 normal + 2 runners; NavMesh movement, group alert, death counts, intermission, completion, replay, disable cleanup, unexpected removal, invalid navigation and invalid configuration. Runner regression also passed."
                : "Runner checks passed: group alert, isolation, lethal hit, invincibility, disabled/dead members, hurt recovery, late animation event.");
        }
        catch (Exception exception)
        {
            Finish(false, exception.ToString());
        }
    }

    private static IEnumerator CheckChallenge(bool full)
    {
        var wave = Object.FindObjectOfType<WaveController>();
        if (wave != null)
        {
            while (wave.State != WaveController.ChallengeState.Fighting)
            {
                Require(wave.State != WaveController.ChallengeState.Failed, wave.FailureReason);
                yield return null;
            }
        }
        CheckRunners();
        yield return null;
        if (!full) yield break;

        var player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        Require(wave != null && wave.CurrentWave == 1 && wave.RemainingEnemies == 3, "First wave must contain 3 enemies");
        var first = wave.GetComponentsInChildren<EnemyController>();
        Require(first.Length == 3 && first.All(e => !e.IsRunner), "First wave composition is incorrect");
        Require(first.All(e => e.GetComponent<NavMeshAgent>().isOnNavMesh), "Spawned enemy is off NavMesh");
        var before = first[0].transform.position;
        float until = Time.time + 0.5f;
        while (Time.time < until) yield return null;
        Require(Vector2.Distance(before, first[0].transform.position) > 0.05f, "Enemy did not move on NavMesh");
        if (mode == "visual")
        {
            CaptureFrame("Logs/challenge-wave1.png");
            until = Time.time + 0.4f;
            while (Time.time < until) yield return null;
        }
        foreach (var enemy in first)
        {
            enemy.GetComponent<DamageReceiver>().TakeDamage(1000);
            enemy.GetComponent<Health>().TakeDamage(1000);
        }
        Require(wave.RemainingEnemies == 0 && wave.CurrentWave == 1, "Death counting failed or double counted");
        while (wave.State == WaveController.ChallengeState.Fighting) yield return null;
        Require(wave.State == WaveController.ChallengeState.Intermission, "Missing intermission");
        until = Time.time + 1f;
        while (Time.time < until)
        {
            Require(wave.CurrentWave == 1, "Next wave started too early");
            yield return null;
        }
        while (wave.State != WaveController.ChallengeState.Fighting) yield return null;
        Require(wave.CurrentWave == 2 && wave.RemainingEnemies == 4, "Second wave must contain 4 enemies");
        var second = wave.GetComponentsInChildren<EnemyController>().Where(e => !e.IsDead).ToArray();
        Require(second.Count(e => e.IsRunner) == 2 && second.Count(e => !e.IsRunner) == 2, "Second wave composition is incorrect");
        var runners = second.Where(e => e.IsRunner).ToArray();
        SetField(runners[0], "hasLockedTarget", false);
        SetField(runners[1], "hasLockedTarget", false);
        runners[0].GetComponent<DamageReceiver>().TakeDamage(1);
        Require(runners.All(e => e.HasLockedTarget), "Wave did not assign the Runner group");
        if (mode == "visual")
        {
            CaptureFrame("Logs/challenge-wave2.png");
            until = Time.time + 0.4f;
            while (Time.time < until) yield return null;
        }
        foreach (var enemy in second) enemy.GetComponent<DamageReceiver>().TakeDamage(1000);
        while (wave.State != WaveController.ChallengeState.Completed) yield return null;
        Require(wave.RemainingEnemies == 0, "Completed wave has living enemies");
        Require(Object.FindObjectOfType<WaveHUD>() != null, "HUD missing");
        Require(Object.FindObjectOfType<UnityEngine.UI.Text>().text.Contains("CHALLENGE COMPLETE"), "Completion is not visible");

        if (mode == "visual")
        {
            CaptureFrame("Logs/challenge-complete.png");
            until = Time.time + 0.4f;
            while (Time.time < until) yield return null;
        }
        wave.RestartChallenge();
        while (wave.State != WaveController.ChallengeState.Fighting) yield return null;
        Require(wave.CurrentWave == 1 && wave.RemainingEnemies == 3, "Replay did not reset the challenge");
        wave.enabled = false;
        Require(wave.RemainingEnemies == 0 && wave.State == WaveController.ChallengeState.Idle, "Disable did not clean up");
        wave.enabled = true;
        wave.RestartChallenge();
        while (wave.State != WaveController.ChallengeState.Fighting) yield return null;
        wave.GetComponentsInChildren<EnemyController>().First(e => !e.IsDead).gameObject.SetActive(false);
        while (wave.State == WaveController.ChallengeState.Fighting) yield return null;
        Require(wave.State == WaveController.ChallengeState.Failed && wave.RemainingEnemies == 0, "Unexpected removal left challenge stuck");

        SetField(wave, "navigationTimeout", 0.1f);
        var points = (Transform[])Field(wave, "spawnPoints");
        foreach (var point in points) point.position = new Vector3(10000, 10000, 0);
        wave.RestartChallenge();
        while (wave.State != WaveController.ChallengeState.Failed) yield return null;
        Require(wave.RemainingEnemies == 0, "Invalid navigation spawned a partial wave");
        SetField(wave, "spawnPoints", new Transform[0]);
        wave.RestartChallenge();
        Require(wave.State == WaveController.ChallengeState.Failed, "Invalid configuration did not fail clearly");
    }

    // Batch mode has no Game view framebuffer. Render the same canvas through
    // the game camera temporarily; all scene changes remain inside Play Mode.
    private static void CaptureFrame(string path)
    {
        Camera camera = Camera.main;
        Canvas canvas = Object.FindObjectOfType<WaveHUD>().GetComponent<Canvas>();
        RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24);
        RenderTexture previous = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        RenderMode previousMode = canvas.renderMode;
        Camera previousCamera = canvas.worldCamera;
        float previousDistance = canvas.planeDistance;
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 1;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = previousMode;
            canvas.worldCamera = previousCamera;
            canvas.planeDistance = previousDistance;
            camera.targetTexture = previousTarget;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.Destroy(pixels);
        }
    }

    private static EnemyController Spawn(string path, Vector3 position)
    {
        return Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), position, Quaternion.identity)
            .GetComponent<EnemyController>();
    }

    private static void CheckRunners()
    {
        var anchor = Object.FindObjectOfType<EnemyController>().transform.position;
        var a = Spawn("Assets/Prefabs/Enemy/GoblinRunner.prefab", anchor);
        var b = Spawn("Assets/Prefabs/Enemy/GoblinRunner.prefab", anchor);
        var other = Spawn("Assets/Prefabs/Enemy/GoblinRunner.prefab", anchor);
        var normal = Spawn("Assets/Prefabs/Enemy/Gobling.prefab", anchor);
        var disabled = Spawn("Assets/Prefabs/Enemy/GoblinRunner.prefab", anchor);
        var group = new RunnerGroup();
        a.AssignRunnerGroup(group);
        b.AssignRunnerGroup(group);
        normal.AssignRunnerGroup(group);
        disabled.AssignRunnerGroup(group);
        other.AssignRunnerGroup(new RunnerGroup());
        disabled.gameObject.SetActive(false);
        a.GetComponent<DamageReceiver>().SetInvincible(true);
        a.GetComponent<DamageReceiver>().TakeDamage(1);
        Require(!a.HasLockedTarget && !b.HasLockedTarget, "Invincible hit alerted runners");
        a.GetComponent<DamageReceiver>().SetInvincible(false);
        a.GetComponent<DamageReceiver>().TakeDamage(1);
        Require(a.HasLockedTarget && b.HasLockedTarget, "Same group was not alerted");
        Require(!other.HasLockedTarget && !normal.HasLockedTarget && !disabled.HasLockedTarget, "Alert crossed ownership/type/active boundary");
        Require(Field(a, "currentState").ToString() == "Hurt", "Alert interrupted hurt");
        SetField(a, "hurtTimer", 0f);
        Invoke(a, "UpdateHurt");
        Invoke(a, "UpdateIdle");
        Require(Field(a, "currentState").ToString() == "Chase", "Hurt did not resume pursuit");
        disabled.gameObject.SetActive(true);
        Require(disabled.HasLockedTarget, "Re-enabled member lost group alert");
        var lethal = new RunnerGroup();
        other.AssignRunnerGroup(lethal);
        b.AssignRunnerGroup(lethal);
        SetField(b, "hasLockedTarget", false);
        other.GetComponent<DamageReceiver>().TakeDamage(1000);
        Require(other.IsDead && b.HasLockedTarget, "Lethal hit did not alert survivors");
        other.OnAttackFinished();
        Require(other.IsDead, "Late animation callback revived a dead enemy");
        SetField(other, "hasLockedTarget", false);
        other.LockTarget();
        Require(!other.HasLockedTarget, "Dead runner accepted target lock");
        foreach (var enemy in new[] { a, b, other, normal, disabled })
            Object.Destroy(enemy.gameObject);
    }

    private static object Field(object obj, string name) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
    private static void SetField(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
    private static void Invoke(object obj, string name) => obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, null);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Finish(bool success, string message)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= CaptureError;
        SessionState.EraseString(Key);
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/" + (mode == "runner" ? "runner" : mode == "visual" ? "visual" : "wave") + "-verification.txt", (success ? "PASS\n" : "FAIL\n") + message);
        Debug.Log(message);
        EditorApplication.Exit(success ? 0 : 1);
    }
}

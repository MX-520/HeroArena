using System;
using System.IO;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static partial class CompleteDemoVerification
{
    const string Key = "HeroArena.FullDemoTest";
    private static IEnumerator routine;
    private static double deadline;
    private static bool errors;
    static CompleteDemoVerification() { EditorApplication.playModeStateChanged += Entered; }
    public static void RunShooting() => Run("shooting");
    private static void Run(string mode)
    {
        SessionState.SetString(Key, mode);
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        EditorApplication.isPlaying = true;
    }
    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || SessionState.GetString(Key, "") == "") return;
        deadline = EditorApplication.timeSinceStartup + 60;
        errors = false;
        Application.logMessageReceived += Log;
        routine = SessionState.GetString(Key, "") == "ranged" ? Ranged() : SessionState.GetString(Key, "") == "waves" ? Waves() : SessionState.GetString(Key, "") == "drops" ? Drops() : Shooting();
        EditorApplication.update += Tick;
    }
    private static void Log(string message, string stack, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors = true; }
    private static void Tick()
    {
        try
        {
            Require(EditorApplication.timeSinceStartup < deadline, "Test timeout");
            if (routine.MoveNext()) return;
            Require(!errors, "Runtime errors logged");
            Finish(true, "PASS " + SessionState.GetString(Key, ""));
        }
        catch (Exception e) { Finish(false, e.ToString()); }
    }
    static IEnumerator Shooting()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerShooting>();
        var target = new GameObject("ProjectileTarget");
        target.layer = LayerMask.NameToLayer("Enemy");
        target.transform.position = player.transform.position + Vector3.right * 2;
        var health = target.AddComponent<Health>();
        var receiver = target.AddComponent<DamageReceiver>();
        target.AddComponent<BoxCollider2D>();
        Physics2D.SyncTransforms();
        foreach (var hit in Physics2D.CircleCastAll(player.transform.position + Vector3.right * 0.55f, 0.08f, Vector2.right, 3f))
            Debug.Log($"Projectile fixture hit: {hit.collider.name}, layer {hit.collider.gameObject.layer}, distance {hit.distance}");
        Require(player.TryShoot(target.transform.position), "Could not shoot");
        Require(!player.TryShoot(target.transform.position), "Cooldown ignored");
        float until = Time.time + 0.4f;
        while (Time.time < until) yield return null;
        Require(health.CurrentHP == 88, "Projectile did not apply exactly 12 damage; HP=" + health.CurrentHP);
        receiver.SetInvincible(true);
        Require(player.TryShoot(target.transform.position), "Second shot blocked");
        until = Time.time + 0.4f;
        while (Time.time < until) yield return null;
        Require(health.CurrentHP == 88, "Invincible target damaged");
        var movement = player.GetComponent<PlayerMovement>();
        movement.EnterHurt();
        Require(!player.TryShoot(target.transform.position), "Fired while hurt");
        movement.ExitHurt();
        Time.timeScale = 0;
        Require(!player.TryShoot(target.transform.position), "Fired while paused");
        Time.timeScale = 1;
        receiver.SetInvincible(false);
        var wall = new GameObject("ThinWall");
        wall.transform.position = player.transform.position + Vector3.right;
        wall.AddComponent<BoxCollider2D>().size = new Vector2(0.03f, 2);
        Physics2D.SyncTransforms();
        Require(player.TryShoot(target.transform.position), "Wall test could not fire");
        until = Time.time + 0.4f;
        while (Time.time < until) yield return null;
        Require(health.CurrentHP == 88, "Projectile passed through thin wall");
        Object.Destroy(wall);
        Object.Destroy(target);
    }
    public static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Finish(bool success, string result)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= Log;
        string mode = SessionState.GetString(Key, "");
        SessionState.EraseString(Key);
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/full-demo-" + mode + ".txt", (success ? "PASS\n" : "FAIL\n") + result);
        Debug.Log(result);
        EditorApplication.Exit(success ? 0 : 1);
    }
}

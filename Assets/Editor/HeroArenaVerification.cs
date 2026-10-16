using System;
using System.IO;
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

    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || SessionState.GetString(Key, "") == "")
            return;
        deadline = EditorApplication.timeSinceStartup + 1;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < deadline)
            return;
        EditorApplication.update -= Tick;
        try
        {
            CheckRunners();
            Finish(true, "Runner checks passed: group alert, isolation, lethal hit, invincibility, disabled/dead members, hurt recovery, late animation event.");
        }
        catch (Exception exception)
        {
            Finish(false, exception.ToString());
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
        SessionState.EraseString(Key);
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/runner-verification.txt", (success ? "PASS\n" : "FAIL\n") + message);
        Debug.Log(message);
        EditorApplication.Exit(success ? 0 : 1);
    }
}

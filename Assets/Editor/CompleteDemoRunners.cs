using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunRunnerRegression() => Run("runners");
    public static void RunRegression() => Run("regression");
    private static IEnumerator Runners()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        CheckRunners();
        yield return null;
    }
    private static IEnumerator Regression()
    {
        foreach (var check in new[] { Shooting(), Ranged(), Drops(), Runners(), Waves() })
        {
            var player = Object.FindObjectOfType<PlayerHealth>();
            player.GetComponent<Health>().RestoreFull();
            player.GetComponent<PlayerEffects>().Clear();
            player.GetComponent<PlayerMovement>().Respawn();
            player.GetComponent<PlayerMovement>().ResetInput();
            var receiver = player.GetComponent<DamageReceiver>();
            receiver.enabled = true; receiver.SetInvincible(false); receiver.SetShield(false);
            player.GetComponentInChildren<Animator>().Play("Idle");
            while (check.MoveNext()) yield return null;
            yield return null;
        }
    }
    private static EnemyController Spawn(string path, Vector3 position)
    {
        return Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), position, Quaternion.identity)
            .GetComponent<EnemyController>();
    }

    private static void CheckRunners()
    {
        var anchor = Object.FindObjectOfType<PlayerHealth>().transform.position + Vector3.right * 4;
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


}

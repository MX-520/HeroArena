using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunFeedbackAll() => Run("feedback-all");
    static IEnumerator FeedbackAll()
    {
        Require(Mathf.Approximately(UnityEngine.AI.NavMesh.GetSettingsByID(-1372625422).agentRadius, 0.1f), "Original navigation bake radius not restored");
        Require(AssetDatabase.GetAssetPath(Object.FindObjectOfType<NavMeshPlus.Components.NavMeshSurface>().navMeshData) == "Assets/Scenes/SampleScene/NavMesh-Navigation.asset", "Scene does not use original navigation data");
        Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/EnemyEnergyBolt.prefab").transform.localScale == Vector3.one * 2.7f, "Enemy projectile was not enlarged");
        // Stone-pillar jitter is deferred; retain probes only as optional diagnostics.
        var checks = new[] { FeedbackEnemies(), Regression(), FeedbackInput() };
        foreach (var check in checks)
        {
            var player = Object.FindObjectOfType<PlayerHealth>();
            player.GetComponent<Health>().RestoreFull();
            player.GetComponent<PlayerEffects>().Clear();
            player.GetComponent<PlayerMovement>().Respawn();
            player.GetComponent<PlayerMovement>().ResetInput();
            var receiver = player.GetComponent<DamageReceiver>(); receiver.enabled = true; receiver.SetInvincible(true);
            player.GetComponentInChildren<Animator>().Play("Idle");
            while (check.MoveNext()) yield return null;
            yield return null;
        }
    }
    static IEnumerator FeedbackEnemies()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/GoblinArcher.prefab");
        var fixture = Object.Instantiate(prefab, player.transform.position + Vector3.right * 4, Quaternion.identity);
        Require(fixture.GetComponentsInChildren<Transform>(true).All(t => t.name != "Crossbow"), "Crossbow image still present");
        Require(!fixture.GetComponentInChildren<Animator>().GetBool("IsRunner") && fixture.GetComponent<NavMeshAgent>().speed < 3f, "Ranged goblin runs");
        var shots = new List<float>();
        int previous = 0;
        float until = Time.time + 4.5f;
        while (Time.time < until && shots.Count < 3)
        {
            int current = fixture.GetComponent<EnemyAttack>().ShotsFired;
            if (current > previous)
            {
                shots.Add(Time.time); previous = current;
                Require(fixture.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Any(c => c.clip.name == "RangedShoot"), "Ranged goblin plays melee animation");
            }
            yield return null;
        }
        Require(shots.Count >= 3, "Ranged goblin did not shoot repeatedly");
        for (int i = 1; i < shots.Count; i++) Require(Mathf.Abs(shots[i] - shots[i - 1] - 1.2f) < 0.12f, "Actual shot interval is not 1.2s: " + (shots[i] - shots[i - 1]));
        var a = fixture.GetComponent<EnemyController>();
        var second = Object.Instantiate(prefab, player.transform.position + Vector3.left * 4, Quaternion.identity).GetComponent<EnemyController>();
        var group = new RunnerGroup(); a.AssignRunnerGroup(group); second.AssignRunnerGroup(group);
        SetField(a, "hasLockedTarget", false); SetField(second, "hasLockedTarget", false);
        a.GetComponent<DamageReceiver>().TakeDamage(1);
        Require(a.HasLockedTarget && second.HasLockedTarget, "Ranged group did not alert");
        SetField(second, "detectionRange", 0.01f);
        a.GetComponent<DamageReceiver>().TakeDamage(1000);
        float pursuitUntil = Time.time + 0.7f;
        while (Time.time < pursuitUntil) yield return null;
        Require(second.HasLockedTarget && Field(second, "currentState").ToString() != "Idle", "Ranged goblin lost target outside detection range");
        second.GetComponent<DamageReceiver>().TakeDamage(1000);
        Object.Destroy(fixture); Object.Destroy(second.gameObject);
        yield return null;
    }
}

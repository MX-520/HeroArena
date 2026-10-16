using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunObstacleApproach() => Run("obstacle-approach");
    static IEnumerator ObstacleApproach()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        var playerBody = player.GetComponent<Collider2D>();
        Vector3 original = player.transform.position;
        Vector3 playerOffset = playerBody.bounds.center - original;
        var obstacle = Object.FindObjectsOfType<DestructibleObstacle>().OrderBy(o => o.GetComponent<Collider2D>().bounds.size.y).First();
        var box = obstacle.GetComponent<Collider2D>();
        var report = new StringBuilder();
        foreach (string kind in new[] { "Gobling", "GoblinRunner" })
        {
            player.transform.position = box.bounds.center + Vector3.up * (box.bounds.extents.y + playerBody.bounds.extents.y + 0.05f) - playerOffset;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/" + kind + ".prefab");
            Vector3 footOffset = prefab.transform.TransformVector(prefab.GetComponent<Collider2D>().offset);
            Vector3 spawn = box.bounds.center + Vector3.down * (box.bounds.extents.y + 0.55f) - footOffset;
            var enemy = Object.Instantiate(prefab, spawn, Quaternion.identity).GetComponent<EnemyController>();
            var agent = enemy.GetComponent<NavMeshAgent>();
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            Require(NavMesh.SamplePosition(spawn, out NavMeshHit sample, 1.5f, filter) && agent.Warp(sample.position), "Approach fixture cannot join navmesh");
            Physics2D.SyncTransforms();
            var attack = enemy.GetComponent<EnemyAttack>();
            bool initiallyBlocked = !attack.HasClearAttackPath(player.transform);
            bool reached = false;
            float until = Time.time + 7f;
            while (Time.time < until)
            {
                bool clear = attack.HasClearAttackPath(player.transform);
                Require(clear || !enemy.CanPerformHit, kind + " attacked while obstacle remained between feet");
                if (enemy.CanPerformHit && clear) { reached = true; break; }
                yield return null;
            }
            report.AppendLine(kind + ": obstacle=" + obstacle.name + ", initiallyBlocked=" + initiallyBlocked + ", reachedClearAttack=" + reached + ", rootDistance=" + Vector2.Distance(enemy.transform.position, player.transform.position).ToString("F2") + ", path=" + agent.pathStatus);
            File.WriteAllText("Logs/obstacle-approach.txt", report.ToString());
            Require(initiallyBlocked && reached, "Enemy did not walk around actual obstacle: " + report);
            Object.Destroy(enemy.gameObject); yield return null;
        }
        player.transform.position = original;
    }
}

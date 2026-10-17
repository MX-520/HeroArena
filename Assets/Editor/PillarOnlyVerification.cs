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
    public static void RunPillarOnlyBaseline() => Run("pillar-only-baseline");
    public static void RunPillarOnlyFixed() => Run("pillar-only-fixed");
    static IEnumerator PillarOnly(bool strict)
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        var body = player.GetComponent<Collider2D>();
        Vector3 original = player.transform.position;
        Vector3 offset = body.bounds.center - original;
        var pillars = GameObject.Find("MapCollision/Peristele").GetComponentsInChildren<BoxCollider2D>().Where(c => c.gameObject.layer == 0).ToArray();
        var report = new StringBuilder();
        string file = "Logs/pillar-only-" + (strict ? "fixed" : "baseline") + ".txt";
        var reference = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab");
        var foot = reference.GetComponent<BoxCollider2D>();
        Vector3 enemyOffset = reference.transform.TransformVector(foot.offset);
        Vector3 extents = Vector3.Scale(foot.size, reference.transform.lossyScale) * 0.5f;
        report.AppendLine($"enemyFoot offset={enemyOffset:F3} extents={extents:F3}, buildRadius={NavMesh.GetSettingsByID(-1372625422).agentRadius}");
        foreach (var pillar in pillars)
        foreach (Vector3 direction in new[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right })
        {
            var proxy = pillar.transform.Find("NavigationObstacle").GetComponent<BoxCollider2D>();
            float edge = Mathf.Abs(direction.x) * (pillar.bounds.extents.x + body.bounds.extents.x) + Mathf.Abs(direction.y) * (pillar.bounds.extents.y + body.bounds.extents.y);
            player.transform.position = pillar.bounds.center + direction * (edge + 0.03f) - offset;
            Physics2D.SyncTransforms();
            var enemies = new EnemyController[2];
            var flips = new int[2]; var facing = new bool[2]; var reached = new bool[2];
            Vector3 tangent = new Vector3(-direction.y, direction.x, 0);
            for (int i = 0; i < 2; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/" + (i == 0 ? "Gobling" : "GoblinRunner") + ".prefab");
                enemies[i] = Object.Instantiate(prefab, pillar.bounds.center - direction * (edge + 1.2f) + tangent * ((i - 0.5f) * 0.8f) - enemyOffset, Quaternion.identity).GetComponent<EnemyController>();
                var agent = enemies[i].GetComponent<NavMeshAgent>();
                var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                Require(NavMesh.SamplePosition(enemies[i].transform.position, out NavMeshHit spawn, 1.5f, filter) && agent.Warp(spawn.position), "Pillar spawn invalid");
                facing[i] = enemies[i].GetComponentInChildren<SpriteRenderer>().flipX;
            }
            float penetration = 0;
            float until = Time.time + 6f;
            while (Time.time < until)
            {
                Physics2D.SyncTransforms();
                for (int i = 0; i < 2; i++)
                {
                    var sprite = enemies[i].GetComponentInChildren<SpriteRenderer>();
                    if (sprite.flipX != facing[i]) flips[i]++;
                    facing[i] = sprite.flipX;
                    reached[i] |= enemies[i].CanPerformHit;
                    var contact = enemies[i].GetComponent<Collider2D>().Distance(pillar);
                    if (contact.isOverlapped) penetration = Mathf.Max(penetration, -contact.distance);
                }
                yield return null;
            }
            NavMesh.SamplePosition(player.transform.position, out NavMeshHit target, 3, new NavMeshQueryFilter { agentTypeID = -1372625422, areaMask = NavMesh.AllAreas });
            for(int i=0;i<2;i++)
            {
                var probe = (PillarNavigation.Approach)Field(enemies[i], "pillarApproach");
                bool found = probe.TryGetDestination(player.transform,enemies[i].GetComponent<EnemyAttack>(),enemies[i].GetComponent<Collider2D>(),enemies[i].GetComponent<NavMeshAgent>(),out Vector3 goal);
                report.AppendLine($"enemy={i} pos={enemies[i].transform.position:F3} player={player.transform.position:F3} playerFeet={body.bounds.center:F3} approach={found} goal={goal:F3} vel={enemies[i].GetComponent<NavMeshAgent>().velocity:F3} canAttack={enemies[i].GetComponent<EnemyAttack>().CanAttackTarget(player.transform)}");
            }
            report.AppendLine($"pillar={pillar.name}, side={direction}, proxyCenter={proxy.bounds.center:F3} size={proxy.bounds.size:F3}, targetProjection={target.distance:F3}, flips=[{string.Join(",",flips)}], reached=[{string.Join(",",reached)}], penetration={penetration:F3}, states=[{string.Join(",",enemies.Select(e=>Field(e,"currentState")))}], paths=[{string.Join(",",enemies.Select(e=>e.GetComponent<NavMeshAgent>().pathStatus))}]");
            File.WriteAllText(file, report.ToString());
            if (strict) Require(reached.All(v=>v) && flips.All(f=>f<15) && penetration<0.025f, "Pillar approach/jitter/penetration failed: " + report);
            foreach (var enemy in enemies) Object.Destroy(enemy.gameObject);
            yield return null;
        }
        player.transform.position = original;
    }
}

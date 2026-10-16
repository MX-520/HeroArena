using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunBlockedAttackBaseline() => Run("blocked-baseline");
    public static void RunBlockedAttackFixed() => Run("blocked-fixed");
    static IEnumerator BlockedAttack(bool fixedVersion)
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        Vector3 original = player.transform.position;
        var receiver = player.GetComponent<DamageReceiver>();
        receiver.SetInvincible(false);
        var enemy = Spawn("Assets/Prefabs/Enemy/Gobling.prefab", original + Vector3.down * 0.9f);
        var wall = new GameObject("AttackOcclusionFixture");
        var box = wall.AddComponent<BoxCollider2D>();
        var from = enemy.GetComponent<Collider2D>().bounds.center;
        var to = player.GetComponent<Collider2D>().bounds.center;
        wall.transform.position = (from + to) * 0.5f;
        box.size = new Vector2(0.7f, 0.08f);
        Physics2D.SyncTransforms();
        Invoke(enemy, "UpdateIdle");
        Invoke(enemy, "UpdateChase");
        string state = Field(enemy, "currentState").ToString();
        File.WriteAllText("Logs/blocked-attack-" + (fixedVersion ? "fixed" : "baseline") + ".txt", "Root distance=0.9; wall between foot colliders; state=" + state);
        Require(state == (fixedVersion ? "Chase" : "Attack"), "Unexpected obstacle-side state: " + state);
        if (fixedVersion)
        {
            Require(!enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped, "Blocked enemy stopped navigating");
            int hp = player.GetComponent<Health>().CurrentHP;
            enemy.GetComponent<EnemyAttack>().DetectHit();
            Require(player.GetComponent<Health>().CurrentHP == hp, "Melee damaged through obstacle");
            Object.DestroyImmediate(wall); Physics2D.SyncTransforms();
            // Vertical root distance alone can still be inside range while attack-point circle misses.
            enemy.transform.position = original + Vector3.up * 0.9f;
            Physics2D.SyncTransforms();
            Require(!enemy.GetComponent<EnemyAttack>().CanAttackTarget(player.transform), "Melee root range incorrectly implies contact");
            enemy.transform.position = original + Vector3.down * 0.5f;
            Physics2D.SyncTransforms();
            Require(enemy.GetComponent<EnemyAttack>().CanAttackTarget(player.transform), "Clear reachable melee target rejected");
            Invoke(enemy, "UpdateChase");
            Require(Field(enemy, "currentState").ToString() == "Attack", "Enemy did not attack after reaching target");
            enemy.GetComponent<EnemyAttack>().DetectHit();
            Require(player.GetComponent<Health>().CurrentHP < hp, "Reachable melee target took no damage");
        }
        Object.Destroy(enemy.gameObject); if (wall != null) Object.Destroy(wall);
        player.transform.position = original;
        yield return null;
    }
}

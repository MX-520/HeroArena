using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class CompleteDemoSetup
{
    public static void SetupThirdWave()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var challenge = Object.FindObjectOfType<WaveController>();
        var player = Object.FindObjectOfType<PlayerHealth>();
        var data = new SerializedObject(challenge);
        var waves = data.FindProperty("waves");
        waves.arraySize = 3;
        waves.GetArrayElementAtIndex(0).FindPropertyRelative("title").stringValue = "初次交锋";
        waves.GetArrayElementAtIndex(1).FindPropertyRelative("title").stringValue = "群体追猎";
        var third = waves.GetArrayElementAtIndex(2);
        third.FindPropertyRelative("title").stringValue = "交叉火力";
        var entries = third.FindPropertyRelative("enemies");
        entries.arraySize = 3;
        var paths = new[] { "Gobling", "GoblinRunner", "GoblinArcher" };
        for (int i = 0; i < 3; i++)
        {
            var batch = entries.GetArrayElementAtIndex(i);
            batch.FindPropertyRelative("prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/" + paths[i] + ".prefab").GetComponent<EnemyController>();
            batch.FindPropertyRelative("count").intValue = 2;
        }
        var points = data.FindProperty("spawnPoints");
        points.arraySize = 6;
        var agent = ((EnemyController)entries.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue).GetComponent<NavMeshAgent>();
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        for (int i = 4; i < 6; i++)
        {
            var offset = i == 4 ? new Vector3(3, -4, 0) : new Vector3(-3, 4, 0);
            if (!NavMesh.SamplePosition(player.transform.position + offset, out NavMeshHit hit, 2f, filter))
                throw new System.Exception("Cannot place third-wave spawn");
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(hit.position, player.transform.position, filter, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new System.Exception("Third-wave spawn unreachable");
            var point = challenge.transform.Find("SpawnPoint" + (i + 1));
            if (point == null) { point = new GameObject("SpawnPoint" + (i + 1)).transform; point.SetParent(challenge.transform); }
            point.position = hit.position;
            points.GetArrayElementAtIndex(i).objectReferenceValue = point;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        CompleteDemoVerification.RunWaves();
    }
}

public static partial class CompleteDemoVerification
{
    public static void RunWaves() => Run("waves");
    static IEnumerator Waves()
    {
        var wave = Object.FindObjectOfType<WaveController>();
        Object.FindObjectOfType<PlayerHealth>().GetComponent<DamageReceiver>().SetInvincible(true);
        Require(wave.TotalWaves == 3, "Expected three waves");
        wave.enabled = true;
        if (wave.State == WaveController.ChallengeState.Idle) wave.RestartChallenge();
        int[] counts = { 3, 4, 6 };
        for (int index = 0; index < 3; index++)
        {
            while (wave.State != WaveController.ChallengeState.Fighting)
            {
                Require(wave.State != WaveController.ChallengeState.Failed, wave.FailureReason);
                yield return null;
            }
            Require(wave.CurrentWave == index + 1 && wave.RemainingEnemies == counts[index], "Wrong wave composition/count");
            var enemies = wave.GetComponentsInChildren<EnemyController>().Where(e => !e.IsDead).ToArray();
            Require(enemies.All(e => e.GetComponent<NavMeshAgent>().isOnNavMesh), "Enemy off NavMesh");
            if (index == 2)
                Require(enemies.Count(e => e.GetComponent<EnemyAttack>().IsRanged) == 2 && enemies.Count(e => e.IsRunner) == 2, "Crossfire types incorrect");
            foreach (var enemy in enemies) enemy.GetComponent<DamageReceiver>().TakeDamage(1000);
            Require(wave.RemainingEnemies == 0, "Death count failed");
            while (wave.State == WaveController.ChallengeState.Fighting) yield return null;
            if (index < 2) Require(wave.State == WaveController.ChallengeState.Intermission, "Missing recovery interval");
        }
        Require(wave.State == WaveController.ChallengeState.Completed && wave.TotalKills == 13, "Challenge did not complete exactly 13 kills");
        wave.RestartChallenge();
        Require(wave.TotalKills == 0, "Restart did not reset kill statistics");
        wave.enabled = false;
        Require(wave.RemainingEnemies == 0, "Stop leaked enemies");
    }
}

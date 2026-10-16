using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveController : MonoBehaviour
{
    [Serializable]
    public class EnemyBatch
    {
        public EnemyController prefab;
        [Min(1)] public int count = 1;
    }

    [Serializable]
    public class Wave
    {
        public string title;
        public EnemyBatch[] enemies;
    }

    public enum ChallengeState { Idle, Preparing, Fighting, Intermission, Completed, Failed }

    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField, Min(0)] private float initialDelay = 1f;
    [SerializeField, Min(0)] private float intermission = 3f;
    [SerializeField, Min(0.1f)] private float sampleRadius = 2f;
    [SerializeField, Min(0.1f)] private float navigationTimeout = 5f;

    private readonly Dictionary<Health, Action> deaths = new Dictionary<Health, Action>();
    private readonly List<EnemyController> spawned = new List<EnemyController>();
    private Transform player;

    public ChallengeState State { get; private set; }
    public int CurrentWave { get; private set; }
    public string CurrentWaveTitle => waves != null && CurrentWave > 0 && CurrentWave <= waves.Length ? waves[CurrentWave - 1].title : "Forest Trial";
    public int TotalKills { get; private set; }
    public int TotalWaves => waves == null ? 0 : waves.Length;
    public int RemainingEnemies => deaths.Count;
    public string FailureReason { get; private set; }
    public event Action Changed;

    private void Start() => RestartChallenge();

    public void RestartChallenge()
    {
        StopAllCoroutines();
        ClearEnemies();
        CurrentWave = 0;
        TotalKills = 0;
        FailureReason = null;
        State = ChallengeState.Idle;
        if (isActiveAndEnabled)
            StartCoroutine(RunChallenge());
    }

    private IEnumerator RunChallenge()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null || waves == null || waves.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            Fail("Challenge needs a player, waves and spawn points.");
            yield break;
        }
        SetState(ChallengeState.Preparing);
        yield return new WaitForSeconds(initialDelay);

        for (int index = 0; index < waves.Length; index++)
        {
            CurrentWave = index + 1;
            SetState(ChallengeState.Preparing);
            if (waves[index] == null)
            {
                Fail("Missing wave configuration.");
                yield break;
            }
            var prefabs = new List<EnemyController>();
            foreach (EnemyBatch batch in waves[index].enemies ?? Array.Empty<EnemyBatch>())
            {
                if (batch == null || batch.prefab == null || batch.count < 1 || batch.prefab.GetComponent<Health>() == null || batch.prefab.GetComponent<NavMeshAgent>() == null)
                {
                    Fail("Invalid enemy batch in wave " + CurrentWave);
                    yield break;
                }
                for (int count = 0; count < batch.count; count++)
                    prefabs.Add(batch.prefab);
            }
            if (prefabs.Count == 0 || prefabs.Count > spawnPoints.Length)
            {
                Fail("Each enemy needs a separate spawn point in wave " + CurrentWave);
                yield break;
            }

            // Wait for navigation data, then validate the entire wave before instantiation.
            var positions = new Vector3[prefabs.Count];
            float until = Time.time + navigationTimeout;
            while (!TryPositions(prefabs, positions))
            {
                if (Time.time >= until)
                {
                    Fail("No separated, reachable NavMesh spawn positions for wave " + CurrentWave);
                    yield break;
                }
                yield return null;
            }

            var runners = new RunnerGroup();
            for (int i = 0; i < prefabs.Count; i++)
            {
                EnemyController enemy = Instantiate(prefabs[i], positions[i], Quaternion.identity, transform);
                spawned.Add(enemy);
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (!agent.isOnNavMesh || !agent.Warp(positions[i]))
                {
                    Fail("Enemy could not join the NavMesh.");
                    yield break;
                }
                enemy.AssignRunnerGroup(runners);
                Health health = enemy.GetComponent<Health>();
                Action callback = () => EnemyDied(health);
                deaths.Add(health, callback);
                health.OnDied += callback;
            }
            SetState(ChallengeState.Fighting);
            while (RemainingEnemies > 0)
            {
                // Unexpected removal is an explicit failure, rather than a wave that never finishes.
                foreach (Health health in deaths.Keys)
                {
                    if (health == null || !health.gameObject.activeInHierarchy)
                    {
                        Fail("A living wave enemy was removed or disabled.");
                        yield break;
                    }
                }
                yield return null;
            }
            if (index + 1 < waves.Length)
            {
                SetState(ChallengeState.Intermission);
                yield return new WaitForSeconds(intermission);
            }
        }
        SetState(ChallengeState.Completed);
    }

    private bool TryPositions(List<EnemyController> prefabs, Vector3[] positions)
    {
        if (player == null)
            return false;
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (spawnPoints[i] == null)
                return false;
            NavMeshAgent agent = prefabs[i].GetComponent<NavMeshAgent>();
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(spawnPoints[i].position, out NavMeshHit hit, sampleRadius, filter) ||
                !NavMesh.SamplePosition(player.position, out NavMeshHit target, sampleRadius, filter))
                return false;
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(hit.position, target.position, filter, path) || path.status != NavMeshPathStatus.PathComplete)
                return false;
            for (int j = 0; j < i; j++)
            {
                float distance = agent.radius + prefabs[j].GetComponent<NavMeshAgent>().radius + 0.2f;
                if (Vector2.Distance(positions[j], hit.position) < distance)
                    return false;
            }
            positions[i] = hit.position;
        }
        return true;
    }

    private void EnemyDied(Health health)
    {
        if (!deaths.TryGetValue(health, out Action callback))
            return;
        health.OnDied -= callback;
        deaths.Remove(health);
        TotalKills++;
        Changed?.Invoke();
    }

    private void SetState(ChallengeState state)
    {
        State = state;
        Changed?.Invoke();
        Debug.Log($"Challenge: {State}, wave {CurrentWave}/{TotalWaves}, remaining {RemainingEnemies}");
    }

    private void Fail(string reason)
    {
        FailureReason = reason;
        ClearEnemies();
        SetState(ChallengeState.Failed);
        Debug.LogWarning(reason, this);
    }

    private void ClearEnemies()
    {
        foreach (var pair in deaths)
        {
            if (pair.Key != null)
                pair.Key.OnDied -= pair.Value;
        }
        deaths.Clear();
        foreach (EnemyController enemy in spawned)
        {
            if (enemy == null)
                continue;
            enemy.AssignRunnerGroup(null);
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }
        spawned.Clear();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        ClearEnemies();
        CurrentWave = 0;
        SetState(ChallengeState.Idle);
    }
}

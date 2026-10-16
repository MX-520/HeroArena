using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

// Explicit editor command; never modifies scenes during import or normal play.
public static class WaveChallengeSetup
{
    public static void Configure()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        if (Object.FindObjectOfType<WaveController>() != null)
            throw new InvalidOperationException("Challenge already configured; edit the existing component instead.");

        EnemyController normal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab").GetComponent<EnemyController>();
        EnemyController runner = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/GoblinRunner.prefab").GetComponent<EnemyController>();
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        NavMeshAgent agent = normal.GetComponent<NavMeshAgent>();
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        var positions = new List<Vector3>();
        var desired = new[] { new Vector3(3.5f, 0), new Vector3(-3.5f, 0), new Vector3(5.5f, 2), new Vector3(-5.5f, -2) };
        foreach (Vector3 offset in desired)
        {
            if (!NavMesh.SamplePosition(player.position + offset, out NavMeshHit hit, 3f, filter))
                throw new InvalidOperationException("Could not place a spawn point on the baked NavMesh.");
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(hit.position, player.position, filter, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("Spawn point cannot reach the player.");
            foreach (Vector3 previous in positions)
                if (Vector2.Distance(previous, hit.position) < 1.5f)
                    throw new InvalidOperationException("Spawn points overlap.");
            positions.Add(hit.position);
        }

        var root = new GameObject("WaveChallenge");
        var challenge = root.AddComponent<WaveController>();
        var data = new SerializedObject(challenge);
        var points = data.FindProperty("spawnPoints");
        points.arraySize = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            var point = new GameObject("SpawnPoint" + (i + 1)).transform;
            point.SetParent(root.transform);
            point.position = positions[i];
            points.GetArrayElementAtIndex(i).objectReferenceValue = point;
        }
        var waves = data.FindProperty("waves");
        waves.arraySize = 2;
        SetWave(waves.GetArrayElementAtIndex(0), new[] { normal }, new[] { 3 });
        SetWave(waves.GetArrayElementAtIndex(1), new[] { normal, runner }, new[] { 2, 2 });
        data.ApplyModifiedPropertiesWithoutUndo();

        var canvasObject = new GameObject("ChallengeHUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var panel = new GameObject("StatusPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(canvasObject.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1);
        panelRect.anchorMax = new Vector2(0.5f, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, -20);
        panelRect.sizeDelta = new Vector2(660, 58);
        panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.03f, 0.06f, 0.08f, 0.88f);
        panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        var label = new GameObject("WaveStatus", typeof(RectTransform), typeof(UnityEngine.UI.Text));
        label.transform.SetParent(panel.transform, false);
        var rect = label.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12, 4);
        rect.offsetMax = new Vector2(-12, -4);
        var text = label.GetComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.97f, 0.89f);
        text.raycastTarget = false;
        text.text = "GET READY  |  Defeat all enemies";
        var hud = canvasObject.AddComponent<WaveHUD>();
        var hudData = new SerializedObject(hud);
        hudData.FindProperty("challenge").objectReferenceValue = challenge;
        hudData.FindProperty("status").objectReferenceValue = text;
        hudData.ApplyModifiedPropertiesWithoutUndo();

        // Replace only the authored enemy instances after all configuration is valid.
        int removed = 0;
        foreach (EnemyController enemy in Object.FindObjectsOfType<EnemyController>())
        {
            Object.DestroyImmediate(enemy.gameObject);
            removed++;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        File.WriteAllText("Logs/wave-setup.txt", $"Configured 2 waves. Replaced {removed} scene enemies. Player: {player.position}. Spawns: {string.Join(", ", positions)}");
        EditorApplication.Exit(0);
    }

    private static void SetWave(SerializedProperty wave, EnemyController[] prefabs, int[] counts)
    {
        var batches = wave.FindPropertyRelative("enemies");
        batches.arraySize = prefabs.Length;
        for (int i = 0; i < prefabs.Length; i++)
        {
            var batch = batches.GetArrayElementAtIndex(i);
            batch.FindPropertyRelative("prefab").objectReferenceValue = prefabs[i];
            batch.FindPropertyRelative("count").intValue = counts[i];
        }
    }
}

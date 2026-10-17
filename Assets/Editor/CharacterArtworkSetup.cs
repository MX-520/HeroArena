using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Explicit character-art preparation commands; never runs on import.</summary>
public static class CharacterArtworkSetup
{
    [Serializable]
    private sealed class ReachReport
    {
        public Vector3 rootScale;
        public Vector3 attackCenterOffset;
        public float normalRadius;
        public float forwardBoundary;
        public float standingBodyHeight;
        public float swordTipInBodyHeights;
        public float giantForwardBoundary;
    }

    public static void Measure()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            WriteMeasurement();
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    public static void MeasureLive()
    {
        try { WriteMeasurement(); }
        catch (Exception exception)
        {
            Directory.CreateDirectory("Logs/Characters");
            File.WriteAllText("Logs/Characters/measurement-error.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    private static void WriteMeasurement()
    {
        var attack = UnityEngine.Object.FindObjectOfType<PlayerAttack>();
        if (attack == null) throw new Exception("Missing player attack");
        var serialized = new SerializedObject(attack);
        var point = (Transform)serialized.FindProperty("attackPoint").objectReferenceValue;
        float radius = serialized.FindProperty("attackRange").floatValue;
        var renderer = attack.GetComponentInChildren<SpriteRenderer>();
        float bodyHeight = renderer.sprite.rect.height / renderer.sprite.pixelsPerUnit * renderer.transform.lossyScale.y;
        Vector3 offset = point.position - attack.transform.position;
        var report = new ReachReport
        {
            rootScale = attack.transform.lossyScale,
            attackCenterOffset = offset,
            normalRadius = radius,
            forwardBoundary = Mathf.Abs(offset.x) + radius,
            standingBodyHeight = bodyHeight,
            swordTipInBodyHeights = (Mathf.Abs(offset.x) + radius) / bodyHeight,
            giantForwardBoundary = Mathf.Abs(offset.x) + radius * 1.5f
        };
        Directory.CreateDirectory("Logs/Characters");
        File.WriteAllText("Logs/Characters/attack-reach-baseline.json", JsonUtility.ToJson(report, true));
        Debug.Log("Character reach: " + JsonUtility.ToJson(report));
    }
}

using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using Object = UnityEngine.Object;

public static partial class CombatFeedbackSetup
{
    public static void AuditNavigation()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var report = new StringBuilder();
        foreach (var collider in Object.FindObjectsOfType<Collider2D>())
        {
            if (collider.isTrigger) continue;
            string path = collider.name;
            for (Transform parent = collider.transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;
            report.AppendLine($"COLLIDER {path} layer={collider.gameObject.layer} center={collider.bounds.center} size={collider.bounds.size}");
        }
        foreach (var surface in Object.FindObjectsOfType<NavMeshSurface>())
        {
            report.AppendLine($"SURFACE {surface.name} type={surface.agentTypeID} layers={surface.layerMask.value} useGeometry={surface.useGeometry} bounds={surface.navMeshData.sourceBounds}");
            var settings = surface.GetBuildSettings();
            report.AppendLine($"BUILD radius={settings.agentRadius} voxel={settings.voxelSize} overrideVoxel={settings.overrideVoxelSize}");
        }
        foreach (string name in new[] { "Gobling", "GoblinRunner", "GoblinArcher" })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/" + name + ".prefab");
            var agent = root.GetComponent<NavMeshAgent>();
            report.AppendLine($"AGENT {name} radius={agent.radius} speed={agent.speed} scale={root.transform.localScale} updatePosition={agent.updatePosition} RB={root.GetComponent<Rigidbody2D>().bodyType}");
        }
        File.WriteAllText("Logs/pillar-navigation-before.txt", report.ToString());
        EditorApplication.Exit(0);
    }
}

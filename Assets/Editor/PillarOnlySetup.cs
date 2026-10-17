using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using Object = UnityEngine.Object;

public static class PillarOnlySetup
{
    public static void Calibrate()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var reference = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab");
        var body = reference.GetComponent<BoxCollider2D>();
        Vector3 offset = reference.transform.TransformVector(body.offset);
        Vector3 extents = Vector3.Scale(body.size, reference.transform.lossyScale) * 0.5f;
        var surface = Object.FindObjectOfType<NavMeshSurface>();
        float bakedRadius = surface.GetBuildSettings().agentRadius;
        var report = new StringBuilder();
        foreach (var pillar in GameObject.Find("MapCollision/Peristele").GetComponentsInChildren<BoxCollider2D>().Where(c=>c.gameObject.layer==0))
        {
            if (pillar.GetComponent<PillarNavigation>() == null) pillar.gameObject.AddComponent<PillarNavigation>();
            var proxy = pillar.transform.Find("NavigationObstacle").GetComponent<BoxCollider2D>();
            Vector3 center = pillar.bounds.center - offset;
            Vector3 padding = new Vector3(Mathf.Max(0,extents.x-bakedRadius)+0.04f,Mathf.Max(0,extents.y-bakedRadius)+0.04f,0);
            Vector3 size = pillar.bounds.size + 2*padding;
            proxy.offset = proxy.transform.InverseTransformPoint(center);
            proxy.size = new Vector2(size.x/Mathf.Abs(proxy.transform.lossyScale.x),size.y/Mathf.Abs(proxy.transform.lossyScale.y));
            EditorUtility.SetDirty(proxy);
            report.AppendLine($"{pillar.name}: physical={pillar.bounds}, rootOffset={offset}, proxy={proxy.bounds}, bakedRadius={bakedRadius}");
        }
        Physics2D.SyncTransforms();
        surface.BuildNavMesh();
        const string path = "Assets/Scenes/SampleScene/NavMesh-Pillars.asset";
        var previous = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if(previous==null) AssetDatabase.CreateAsset(surface.navMeshData,path);
        else { EditorUtility.CopySerialized(surface.navMeshData,previous);surface.RemoveData();surface.navMeshData=previous;surface.AddData(); }
        EditorUtility.SetDirty(surface);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        File.WriteAllText("Logs/pillar-only-calibration.txt",report.ToString());
        CompleteDemoVerification.RunPillarOnlyFixed();
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using Object = UnityEngine.Object;

public static partial class CombatFeedbackSetup
{
    public static void SetupEnemiesAndNavigation()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var clip = new AnimationClip { name = "RangedShoot", frameRate = 20 };
        var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Enemy/Gobling/Idle.anim");
        var spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var frame = AnimationUtility.GetObjectReferenceCurve(idle, spriteBinding)[0].value;
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, new[] { new ObjectReferenceKeyframe { time = 0, value = frame }, new ObjectReferenceKeyframe { time = 0.6f, value = frame } });
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.r"), new AnimationCurve(new Keyframe(0, 0.8f), new Keyframe(0.3f, 1), new Keyframe(0.6f, 0.8f)));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.g"), new AnimationCurve(new Keyframe(0, 0.72f), new Keyframe(0.3f, 1), new Keyframe(0.6f, 0.72f)));
        AnimationUtility.SetAnimationEvents(clip, new[] { new AnimationEvent { time = 0.3f, functionName = "DetectHit" }, new AnimationEvent { time = 0.55f, functionName = "OnAttackFinished" } });
        const string clipPath = "Assets/Animations/Enemy/Gobling/RangedShoot.anim";
        var previousClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (previousClip == null) AssetDatabase.CreateAsset(clip, clipPath);
        else { EditorUtility.CopySerialized(clip, previousClip); Object.DestroyImmediate(clip); clip = previousClip; }
        var shotRoot = new GameObject("EnemyEnergyBolt");
        var renderer = shotRoot.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        renderer.color = new Color(0.8f, 0.5f, 1);
        renderer.sortingOrder = 30;
        shotRoot.transform.localScale = Vector3.one * 1.35f;
        shotRoot.AddComponent<Projectile>();
        var shotPrefab = PrefabUtility.SaveAsPrefabAsset(shotRoot, "Assets/Prefabs/Combat/EnemyEnergyBolt.prefab");
        Object.DestroyImmediate(shotRoot);
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemy/GoblinArcher.prefab");
        CompleteDemoSetup.Number(root.GetComponent<EnemyController>(), "attackCooldown", 1.2f);
        var weapon = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Crossbow");
        if (weapon != null) Object.DestroyImmediate(weapon.gameObject);
        CompleteDemoSetup.Set(root.GetComponent<EnemyAttack>(), "projectilePrefab", shotPrefab.GetComponent<Projectile>());
        var animator = root.GetComponentInChildren<Animator>();
        const string controllerPath = "Assets/Animations/Enemy/Gobling/GoblinArcher.overrideController";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(controllerPath);
        if (controller == null)
        {
            controller = new AnimatorOverrideController(AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Enemy/Gobling/Gobling.controller"));
            AssetDatabase.CreateAsset(controller, controllerPath);
        }
        controller["Attack"] = clip;
        animator.runtimeAnimatorController = controller;
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Enemy/GoblinArcher.prefab");
        PrefabUtility.UnloadPrefabContents(root);

        var baseEnemy = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab");
        var foot = baseEnemy.GetComponent<Collider2D>();
        Vector3 footOffset = baseEnemy.transform.TransformVector(foot.offset);
        foreach (var pillar in GameObject.Find("MapCollision/Peristele").GetComponentsInChildren<BoxCollider2D>().Where(c => c.gameObject.layer == 0))
        {
            var proxy = pillar.transform.Find("NavigationObstacle").GetComponent<BoxCollider2D>();
            Vector3 center = pillar.bounds.center - footOffset;
            Vector3 size = Vector3.Max(pillar.bounds.size + new Vector3(0.1f, 0.1f, 0), proxy.bounds.size);
            proxy.offset = proxy.transform.InverseTransformPoint(center);
            proxy.size = new Vector2(size.x / Mathf.Abs(proxy.transform.lossyScale.x), size.y / Mathf.Abs(proxy.transform.lossyScale.y));
            EditorUtility.SetDirty(proxy);
        }
        var surface = Object.FindObjectOfType<NavMeshSurface>();
        surface.overrideVoxelSize = true; surface.voxelSize = 0.08f;
        Physics2D.SyncTransforms();
        surface.BuildNavMesh();
        const string navPath = "Assets/Scenes/SampleScene/NavMesh-CombatNavigation.asset";
        var oldData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
        if (oldData == null) AssetDatabase.CreateAsset(surface.navMeshData, navPath);
        else
        {
            EditorUtility.CopySerialized(surface.navMeshData, oldData);
            surface.RemoveData(); surface.navMeshData = oldData; surface.AddData();
        }
        EditorUtility.SetDirty(surface);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunFeedbackAll();
    }
    public static void OpenReviewScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Selection.activeGameObject = Object.FindObjectOfType<PlayerMovement>().gameObject;
        if (SceneView.lastActiveSceneView != null) { SceneView.lastActiveSceneView.in2DMode = true; SceneView.lastActiveSceneView.FrameSelected(); }
    }
}

using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoSetup
{
    private static void ConfigureHitboxes()
    {
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(10).stringValue = "ProjectileHitbox";
        tags.ApplyModifiedPropertiesWithoutUndo();
        var baseRoot = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemy/Gobling.prefab");
        AddHitbox(baseRoot);
        PrefabUtility.SaveAsPrefabAsset(baseRoot, "Assets/Prefabs/Enemy/Gobling.prefab");
        PrefabUtility.UnloadPrefabContents(baseRoot);
        AddHitbox(Object.FindObjectOfType<PlayerMovement>().gameObject);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
    private static void AddHitbox(GameObject target)
    {
        if (target.transform.Find("ProjectileHitbox") != null) return;
        var box = new GameObject("ProjectileHitbox");
        box.layer = 10;
        box.transform.SetParent(target.transform, false);
        var collider = box.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.3f, 0.4f);
        collider.offset = new Vector2(0, 0.03f);
    }

    public static void SetupRanged()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        ConfigureHitboxes();
        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = "GoblinArcher";
        var controller = instance.GetComponent<EnemyController>();
        Number(controller, "detectionRange", 10);
        Number(controller, "attackCooldown", 1.2f);
        Number(instance.GetComponent<Health>(), "maxHP", 40);
        Number(instance.GetComponent<UnityEngine.AI.NavMeshAgent>(), "m_Speed", 2.3f);
        var attack = instance.GetComponent<EnemyAttack>();
        var data = new SerializedObject(attack);
        data.FindProperty("isRanged").boolValue = true;
        data.FindProperty("attackRange").floatValue = 6;
        data.FindProperty("damage").intValue = 8;
        data.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/EnemyEnergyBolt.prefab").GetComponent<Projectile>();
        instance.GetComponentInChildren<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Enemy/Gobling/GoblinArcher.overrideController");
        instance.GetComponentInChildren<SpriteRenderer>().color = new Color(0.8f, 0.72f, 1f);
        var lineObject = new GameObject("AimWarning");
        lineObject.transform.SetParent(instance.transform, false);
        var line = lineObject.AddComponent<LineRenderer>();
        var material = new Material(Shader.Find("Sprites/Default"));
        var previousMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Combat/AimWarning.mat");
        if (previousMaterial == null) AssetDatabase.CreateAsset(material, "Assets/Art/Combat/AimWarning.mat");
        else { Object.DestroyImmediate(material); material = previousMaterial; }
        line.sharedMaterial = material;
        line.positionCount = 2;
        line.startWidth = 0.04f;
        line.endWidth = 0.02f;
        line.startColor = new Color(1, 0.35f, 0.65f, 0.8f);
        line.endColor = new Color(1, 0.7f, 0.2f, 0.5f);
        line.sortingOrder = 20;
        line.enabled = false;
        data.FindProperty("aimLine").objectReferenceValue = line;
        data.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(instance, "Assets/Prefabs/Enemy/GoblinArcher.prefab");
        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunRanged();
    }
}

public static partial class CompleteDemoVerification
{
    public static void RunRanged() => Run("ranged");
    static IEnumerator Ranged()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        var health = player.GetComponent<Health>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/GoblinArcher.prefab");
        var enemy = Object.Instantiate(prefab, player.transform.position + Vector3.right * 4f, Quaternion.identity);
        var attack = enemy.GetComponent<EnemyAttack>();
        float until = Time.time + 2f;
        while (!enemy.GetComponentInChildren<LineRenderer>(true).enabled && Time.time < until) yield return null;
        Require(enemy.GetComponentInChildren<LineRenderer>(true).enabled, "Archer has no aim telegraph");
        until = Time.time + 2f;
        while (health.CurrentHP == health.MaxHP && Time.time < until) yield return null;
        Require(health.CurrentHP == 92 && attack.ShotsFired >= 1, "Archer projectile did not hit for 8 damage; HP=" + health.CurrentHP);
        int shots = attack.ShotsFired;
        enemy.GetComponent<DamageReceiver>().TakeDamage(1);
        enemy.GetComponentInChildren<EnemyAnimationEventReceiver>().DetectHit();
        Require(attack.ShotsFired == shots && !enemy.GetComponentInChildren<LineRenderer>(true).enabled, "Hurt did not cancel windup");
        enemy.GetComponent<DamageReceiver>().TakeDamage(1000);
        enemy.GetComponentInChildren<EnemyAnimationEventReceiver>().DetectHit();
        Require(attack.ShotsFired == shots, "Dead archer fired a late shot");
        Object.Destroy(enemy);
    }
}

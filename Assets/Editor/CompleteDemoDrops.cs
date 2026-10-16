using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoSetup
{
    public static void SetupDrops()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Pickups");
        var prefabs = new Pickup[4];
        string[] names = { "Heal", "Barrage", "Giant", "Shield" };
        for (int i = 0; i < names.Length; i++)
        {
            var root = new GameObject(names[i] + "Pickup");
            root.transform.localScale = Vector3.one * 0.75f;
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = Icon(names[i]);
            renderer.sortingOrder = 22;
            root.AddComponent<CircleCollider2D>().isTrigger = true;
            root.GetComponent<CircleCollider2D>().radius = 0.5f;
            var pickup = root.AddComponent<Pickup>();
            Number(pickup, "kind", i);
            var saved = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Pickups/" + names[i] + ".prefab");
            prefabs[i] = saved.GetComponent<Pickup>();
            Object.DestroyImmediate(root);
        }
        var enemy = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemy/Gobling.prefab");
        ConfigureDrop(enemy, prefabs, 0.35f);
        PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Prefabs/Enemy/Gobling.prefab");
        PrefabUtility.UnloadPrefabContents(enemy);
        foreach (var obstacle in Object.FindObjectsOfType<DestructibleObstacle>()) ConfigureDrop(obstacle.gameObject, prefabs, 0.7f);
        var player = Object.FindObjectOfType<PlayerMovement>();
        var effects = player.GetComponent<PlayerEffects>() ?? player.gameObject.AddComponent<PlayerEffects>();
        Set(effects, "shieldSprite", Icon("Shield"));
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        CompleteDemoVerification.RunDrops();
    }
    private static void ConfigureDrop(GameObject root, Pickup[] prefabs, float chance)
    {
        var drop = root.GetComponent<DropOnDeath>() ?? root.AddComponent<DropOnDeath>();
        var data = new SerializedObject(drop);
        var array = data.FindProperty("prefabs");
        array.arraySize = 4;
        for (int i = 0; i < 4; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        data.FindProperty("chance").floatValue = chance;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
}

public static partial class CompleteDemoVerification
{
    public static void RunDrops() => Run("drops");
    static IEnumerator Drops()
    {
        var wave = Object.FindObjectOfType<WaveController>();
        wave.enabled = false;
        var names = new[] { "Heal", "Barrage", "Giant", "Shield" };
        for (int i = 0; i < names.Length; i++)
            Require(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/" + names[i] + ".prefab").GetComponent<Pickup>().Kind == (PickupKind)i, "Pickup prefab kind mismatch: " + names[i]);
        var effects = Object.FindObjectOfType<PlayerEffects>();
        var health = effects.GetComponent<Health>();
        var receiver = effects.GetComponent<DamageReceiver>();
        Require(!effects.TryApply(PickupKind.Heal), "Full health consumed a potion");
        receiver.TakeDamage(30);
        Require(effects.TryApply(PickupKind.Heal) && health.CurrentHP == 95, "Heal did not restore 25");
        Require(effects.TryApply(PickupKind.Heal) && health.CurrentHP == 100, "Healing exceeded maximum");
        var original = new SerializedObject(effects);
        float giantDuration = original.FindProperty("giantDuration").floatValue;
        float shieldDuration = original.FindProperty("shieldDuration").floatValue;
        float barrageDuration = original.FindProperty("barrageDuration").floatValue;
        CompleteDemoSetup.Number(effects, "giantDuration", 0.25f);
        CompleteDemoSetup.Number(effects, "shieldDuration", 0.25f);
        CompleteDemoSetup.Number(effects, "barrageDuration", 0.25f);
        effects.TryApply(PickupKind.Giant);
        effects.TryApply(PickupKind.Barrage);
        effects.TryApply(PickupKind.Shield);
        effects.GetComponent<PlayerMovement>().ExitHurt();
        effects.GetComponentInChildren<Animator>().Play("Idle");
        int shotsBefore = effects.GetComponent<PlayerShooting>().ShotsFired;
        Require(effects.GetComponent<PlayerShooting>().TryShoot(effects.transform.position + Vector3.right * 6), "Barrage could not fire");
        Require(effects.GetComponent<PlayerShooting>().ShotsFired == shotsBefore + 3, "Barrage did not create three projectiles");
        Require(Object.FindObjectsOfType<Projectile>().Any(p => p.Owner == effects.transform && p.Damage == 22), "Giant multiplier did not affect projectile damage");
        Require(effects.GiantActive && effects.GetComponent<PlayerShooting>().Barrage && effects.GetComponent<PlayerMovement>().SpeedMultiplier == 1.25f, "Buffs not applied");
        receiver.SetInvincible(true);
        receiver.SetInvincible(false);
        receiver.TakeDamage(10);
        Require(health.CurrentHP == 100, "Ending roll invincibility erased the shield");
        float until = Time.time + 0.15f;
        while (Time.time < until) yield return null;
        effects.TryApply(PickupKind.Giant);
        Require(effects.Remaining(PickupKind.Giant) > 0.2f, "Repeated buff did not refresh duration");
        until = Time.time + 0.35f;
        while (Time.time < until) yield return null;
        Require(!effects.GiantActive && !effects.GetComponent<PlayerShooting>().Barrage && effects.GetComponent<PlayerMovement>().SpeedMultiplier == 1, "Expired buffs did not restore base values");
        receiver.TakeDamage(10);
        Require(health.CurrentHP == 90, "Expired shield still blocked damage");

        var enemy = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/GoblinRunner.prefab"), effects.transform.position + Vector3.right * 4, Quaternion.identity);
        enemy.GetComponent<DropOnDeath>().GuaranteeHeal = true;
        int before = Object.FindObjectsOfType<Pickup>().Length;
        enemy.GetComponent<DamageReceiver>().TakeDamage(1000);
        enemy.GetComponent<Health>().TakeDamage(1000);
        Require(Object.FindObjectsOfType<Pickup>().Length == before + 1, "Death did not drop exactly one pickup");
        var potion = Object.FindObjectsOfType<Pickup>().Last(p => p.Kind == PickupKind.Heal);
        Require(potion.TryCollect(effects) && health.CurrentHP == 100 && !potion.TryCollect(effects), "Pickup did not heal/clamp/consume once");
        var obstacle = Object.FindObjectsOfType<DestructibleObstacle>().First();
        CompleteDemoSetup.Number(obstacle.GetComponent<DropOnDeath>(), "chance", 1);
        before = Object.FindObjectsOfType<Pickup>().Length;
        obstacle.GetComponent<DamageReceiver>().TakeDamage(1000);
        Require(Object.FindObjectsOfType<Pickup>().Length == before + 1, "Obstacle drop missing");
        effects.TryApply(PickupKind.Giant);
        health.TakeDamage(1000);
        Require(!effects.GiantActive && effects.GetComponent<PlayerMovement>().SpeedMultiplier == 1, "Death leaked buffs");
        yield return null;
        CompleteDemoSetup.Number(effects, "giantDuration", giantDuration);
        CompleteDemoSetup.Number(effects, "shieldDuration", shieldDuration);
        CompleteDemoSetup.Number(effects, "barrageDuration", barrageDuration);
        wave.enabled = true;
        wave.RestartChallenge();
        yield return null;
        Require(Object.FindObjectsOfType<Pickup>().Length == 0 && Object.FindObjectsOfType<Projectile>().Length == 0, "Restart leaked pickups/projectiles");
        wave.enabled = false;
    }
}

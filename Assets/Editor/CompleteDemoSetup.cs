using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoSetup
{
    public const string AtlasPath = "Assets/Art/Combat/CombatAtlas.png";
    public static Sprite Icon(string name) => AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>().First(s => s.name == name);
    public static void Set(Object target, string field, Object value)
    {
        var data = new SerializedObject(target);
        data.FindProperty(field).objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Number(Object target, string field, float value)
    {
        var data = new SerializedObject(target);
        var property = data.FindProperty(field);
        if (property.propertyType == SerializedPropertyType.Integer) property.intValue = (int)value;
        else property.floatValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetupShooting()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 512;
        importer.maxTextureSize = 2048;
        var names = new[] { "Heal", "Barrage", "Giant", "Shield", "Bolt", "Crossbow" };
        importer.spritesheet = names.Select((name, i) => new SpriteMetaData {
            name = name, rect = new Rect((i % 3) * 512, (1 - i / 3) * 512, 512, 512), pivot = Vector2.one * 0.5f, alignment = 0
        }).ToArray();
        importer.SaveAndReimport();
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Combat");
        var shot = new GameObject("PlayerBolt");
        var renderer = shot.AddComponent<SpriteRenderer>();
        renderer.sprite = Icon("Bolt");
        renderer.sortingOrder = 30;
        shot.transform.localScale = Vector3.one * 0.65f;
        shot.AddComponent<Projectile>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(shot, "Assets/Prefabs/Combat/PlayerBolt.prefab");
        Object.DestroyImmediate(shot);
        var player = Object.FindObjectOfType<PlayerMovement>();
        var shooting = player.GetComponent<PlayerShooting>() ?? player.gameObject.AddComponent<PlayerShooting>();
        Set(shooting, "projectilePrefab", prefab.GetComponent<Projectile>());
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        EditorApplication.Exit(0);
    }
}

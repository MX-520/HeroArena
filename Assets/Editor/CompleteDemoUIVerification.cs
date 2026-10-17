using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    public static void RunUI() => Run("ui");
    public static void RunVisualUI() => Run("visual");
    private static UnityEngine.UI.Button FindButton(string name) => Object.FindObjectsOfType<UnityEngine.UI.Button>(true).First(b => b.name == name);
    static IEnumerator UI()
    {
        yield return null;
        var flow = Object.FindObjectOfType<GameFlow>();
        int obstacleCount = Object.FindObjectsOfType<DestructibleObstacle>().Length;
        Require(flow.State == GameFlow.ScreenState.Menu && Time.timeScale == 0, "Menu did not stop the game");
        Require(Object.FindObjectsOfType<EventSystem>().Length == 1 && Object.FindObjectOfType<InputSystemUIInputModule>() != null, "UI input is not configured");
        Canvas.ForceUpdateCanvases();
        var button = FindButton("StartButton");
        var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position) };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count>0 && hits[0].gameObject==button.gameObject, "Start button is obscured from pointer input");
        if (SessionState.GetString(Key, "") == "visual") { CaptureDemo("menu"); CaptureDemo("menu-wide",2048,940); }
        var hover=VerifyHover(button,"menu-hover");
        while(hover.MoveNext()) yield return hover.Current;
        button.onClick.Invoke();
        Require(flow.State == GameFlow.ScreenState.Playing && Time.timeScale == 1, "Start button not wired");
        while (flow.Waves.State != WaveController.ChallengeState.Fighting) yield return null;
        var player = Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        var shooting = player.GetComponent<PlayerShooting>();
        var enemy = flow.Waves.GetComponentsInChildren<EnemyController>().First();
        int hp = enemy.GetComponent<Health>().CurrentHP;
        Require(shooting.TryShoot(enemy.transform.position), "Could not fire at a real goblin");
        float until = Time.time + 0.5f;
        while (Time.time < until) yield return null;
        Require(enemy.GetComponent<Health>().CurrentHP == hp - 12, "Body hitbox did not receive player shot");
        var effects = player.GetComponent<PlayerEffects>();
        effects.TryApply(PickupKind.Giant); effects.TryApply(PickupKind.Shield); effects.TryApply(PickupKind.Barrage);
        yield return null;
        if (SessionState.GetString(Key, "") == "visual") { CaptureDemo("hud"); CaptureDemo("hud-wide", 2048, 940); }
        FindButton("PauseButton").onClick.Invoke();
        Require(flow.State == GameFlow.ScreenState.Paused && Time.timeScale == 0, "Pause button failed");
        float buffTime = effects.Remaining(PickupKind.Barrage);
        Vector3 playerPosition = player.transform.position;
        double wait = EditorApplication.timeSinceStartup + 0.2;
        while (EditorApplication.timeSinceStartup < wait) yield return null;
        Require(Mathf.Abs(buffTime - effects.Remaining(PickupKind.Barrage)) < 0.01f && player.transform.position == playerPosition, "Pause did not freeze buffs/movement");
        if (SessionState.GetString(Key, "") == "visual") { CaptureDemo("pause"); CaptureDemo("pause-wide",2048,940); }
        hover=VerifyHover(FindButton("RestartButton"),"pause-hover");
        while(hover.MoveNext()) yield return hover.Current;
        FindButton("ResumeButton").onClick.Invoke();
        Require(flow.State == GameFlow.ScreenState.Playing && Time.timeScale == 1, "Resume button failed");
        Object.FindObjectOfType<DestructibleObstacle>().GetComponent<Health>().TakeDamage(1000);
        while (flow.State == GameFlow.ScreenState.Playing)
        {
            if (flow.Waves.State == WaveController.ChallengeState.Fighting)
            {
                if (flow.Waves.CurrentWave == 3 && SessionState.GetString(Key, "") == "visual") CaptureDemo("wave3");
                foreach (var target in flow.Waves.GetComponentsInChildren<EnemyController>().Where(e => !e.IsDead))
                    target.GetComponent<DamageReceiver>().TakeDamage(1000);
            }
            yield return null;
        }
        Require(flow.State == GameFlow.ScreenState.Victory && flow.Kills == 13 && Time.timeScale == 0, "Three waves did not show victory");
        if (SessionState.GetString(Key, "") == "visual") { CaptureDemo("victory"); CaptureDemo("victory-wide",2048,940); }
        FindButton("RestartButton").onClick.Invoke();
        yield return null;
        flow = Object.FindObjectOfType<GameFlow>();
        while (flow == null || flow.State != GameFlow.ScreenState.Playing) { yield return null; flow = Object.FindObjectOfType<GameFlow>(); }
        Require(Object.FindObjectsOfType<DestructibleObstacle>().Length == obstacleCount, "Restart did not restore obstacles");
        Require(Vector2.Distance(Object.FindObjectOfType<PlayerHealth>().GetComponent<Collider2D>().bounds.center,CenteredSpawnSetup.ArenaCenter())<.01f,"Restart did not restore centered spawn");
        player = Object.FindObjectOfType<PlayerHealth>();
        Require(player.GetComponent<Health>().CurrentHP == 100 && !player.GetComponent<PlayerEffects>().GiantActive, "Restart leaked health or buffs");
        player.GetComponent<Health>().TakeDamage(1000);
        Require(flow.State == GameFlow.ScreenState.Defeat && Time.timeScale == 0, "Player death did not show defeat");
        if (SessionState.GetString(Key, "") == "visual") CaptureDemo("defeat");
        FindButton("MenuButton").onClick.Invoke();
        yield return null;
        flow = Object.FindObjectOfType<GameFlow>();
        while (flow == null || flow.State != GameFlow.ScreenState.Menu) { yield return null; flow = Object.FindObjectOfType<GameFlow>(); }
        Require(Time.timeScale == 0 && Object.FindObjectsOfType<EnemyController>().Length == 0, "Return to menu did not reset world");
    }

    private static IEnumerator VerifyHover(UnityEngine.UI.Button button,string screenshot)
    {
        var fill=button.GetComponent<UnityEngine.UI.Image>();
        Color resting=fill.color;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Require(hits.Count>0 && hits[0].gameObject==button.gameObject,"Button decoration blocks hover: "+button.name);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerEnterHandler);
        double until=EditorApplication.timeSinceStartup+0.22;
        while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(Vector4.Distance(fill.color,resting)>0.16f && button.transform.localScale.x>1.015f,"Hover did not visibly change fill and scale: "+button.name);
        if(SessionState.GetString(Key,"")=="visual") CaptureDemo(screenshot);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerExitHandler);
        EventSystem.current.SetSelectedGameObject(null);
        until=EditorApplication.timeSinceStartup+0.22;
        while(EditorApplication.timeSinceStartup<until) yield return null;
        Require(Vector4.Distance(fill.color,resting)<0.03f && Mathf.Abs(button.transform.localScale.x-1)<0.002f,"Hover did not restore rest style: "+button.name);
    }

    private static void CaptureDemo(string name, int width = 1280, int height = 720)
    {
        VerifyChinesePresentation();
        Camera camera = Camera.main;
        Canvas canvas = Object.FindObjectOfType<GameUI>().GetComponent<Canvas>();
        var target = RenderTexture.GetTemporary(width, height, 24);
        var previous = RenderTexture.active;
        var oldTarget = camera.targetTexture;
        var oldMode = canvas.renderMode;
        var oldCamera = canvas.worldCamera;
        float oldDistance = canvas.planeDistance;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 1;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            pixels.Apply();
            File.WriteAllBytes("Logs/demo-" + name + ".png", pixels.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = oldMode; canvas.worldCamera = oldCamera; canvas.planeDistance = oldDistance;
            camera.targetTexture = oldTarget; RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target); Object.Destroy(pixels);
        }
    }
}

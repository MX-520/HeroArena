using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

public static partial class CombatFeedbackSetup
{
    public static void SetupInput()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        CompleteDemoSetup.Number(Object.FindObjectOfType<PlayerEffects>(), "barrageDuration", 10);
        foreach (var text in Object.FindObjectsOfType<TMPro.TMP_Text>(true))
        {
            text.text = text.text.Replace("RMB/J melee", "J melee").Replace("J melee", "RMB/J melee").Replace("BARRAGE", "MULTISHOT");
            EditorUtility.SetDirty(text);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        CompleteDemoVerification.RunFeedbackInput();
    }
}
public static partial class CompleteDemoVerification
{
    public static void RunFeedbackInput() => Run("feedback-input");
    public static void RunNavigationBaseline() => Run("nav-baseline");
    public static void RunNavigationFixed() => Run("nav-fixed");
    static IEnumerator FeedbackInput()
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        Object.FindObjectOfType<GameUI>().gameObject.SetActive(false);
        var effects = Object.FindObjectOfType<PlayerEffects>();
        var shooting = effects.GetComponent<PlayerShooting>();
        var input = effects.GetComponent<PlayerInput>();
        Require(input.actions.FindAction("Attack").bindings.Any(b => b.path == "<Mouse>/rightButton"), "Right mouse melee not bound");
        var oldBackground = InputSystem.settings.backgroundBehavior;
        var oldEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var mouse = InputSystem.AddDevice<Mouse>();
        InputSystem.EnableDevice(mouse);
        var state = new MouseState { position = Camera.main.WorldToScreenPoint(effects.transform.position + Vector3.right * 5) };
        InputSystem.QueueStateEvent(mouse, state);
        effects.TryApply(PickupKind.Barrage);
        Require(effects.Remaining(PickupKind.Barrage) > 9.9f, "Multishot duration is not 10 seconds");
        int shots = shooting.ShotsFired;
        float until = Time.time + 0.4f;
        while (Time.time < until) yield return null;
        Require(shooting.ShotsFired == shots, "Multishot fired without left mouse");
        Require(shooting.TryShoot(effects.transform.position + Vector3.right * 5), "Multishot could not fire");
        Require(shooting.ShotsFired == shots + 3, "Multishot did not create three trajectories");
        until = Time.time + 0.15f;
        while (Time.time < until) yield return null;
        Require(!shooting.TryShoot(effects.transform.position + Vector3.right * 5), "Multishot changed normal cooldown");
        input.ActivateInput();
        UnityEngine.InputSystem.Users.InputUser.PerformPairingWithDevice(mouse, input.user);
        input.actions.devices = null;
        input.actions.FindActionMap("Player").Enable();
        InputSystem.QueueStateEvent(mouse, state.WithButton(MouseButton.Right));
        typeof(InputSystem).GetMethod("Update", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(InputUpdateType) }, null).Invoke(null, new object[] { InputUpdateType.Dynamic });
        Debug.Log($"Right click diagnostic: raw={mouse.rightButton.isPressed}, action={input.actions.FindAction("Attack").IsPressed()}, active={input.inputIsActive}, enabled={input.enabled}, timeScale={Time.timeScale}, rolling={effects.GetComponent<PlayerMovement>().IsRolling}, hurt={effects.GetComponent<PlayerMovement>().IsHurt}");
        Require(effects.GetComponent<PlayerAttack>().IsAttacking, "Right click did not actually start melee");
        InputSystem.QueueStateEvent(mouse, state);
        until = Time.time + 10.1f;
        while (Time.time < until) yield return null;
        Require(!shooting.Barrage, "Multishot did not expire");
        InputSystem.RemoveDevice(mouse);
        InputSystem.settings.backgroundBehavior = oldBackground;
        InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorBehavior;
    }
    static IEnumerator NavigationProbe(bool strict)
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        var body = player.GetComponent<Collider2D>();
        Vector3 originalPosition = player.transform.position;
        Vector3 bodyOffset = body.bounds.center - player.transform.position;
        var pillars = GameObject.Find("MapCollision/Peristele").GetComponentsInChildren<BoxCollider2D>().Where(c => c.gameObject.layer == 0).ToArray();
        var report = new StringBuilder();
        foreach (var pillar in pillars)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 center = pillar.bounds.center;
                Vector3 contact = center + Vector3.right * (side * (pillar.bounds.extents.x + body.bounds.extents.x + 0.03f));
                player.transform.position = contact - bodyOffset;
                Physics2D.SyncTransforms();
                var fixture = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/Gobling.prefab"), center - Vector3.right * (side * 2.7f) + Vector3.up * 0.44f, Quaternion.identity);
                var agent = fixture.GetComponent<NavMeshAgent>();
                var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                Require(NavMesh.SamplePosition(fixture.transform.position, out NavMeshHit spawn, 2, filter) && agent.Warp(spawn.position), "Probe could not join NavMesh");
                int flips = 0;
                var sprite = fixture.GetComponentInChildren<SpriteRenderer>();
                bool previousFacing = sprite.flipX;
                float nearest = float.MaxValue;
                float until = Time.time + 2.5f;
                while (Time.time < until)
                {
                    if (sprite.flipX != previousFacing) flips++;
                    previousFacing = sprite.flipX;
                    nearest = Mathf.Min(nearest, Vector2.Distance(fixture.transform.position, player.transform.position));
                    yield return null;
                }
                report.AppendLine($"Pillar={pillar.name}, side={side}, flips={flips}, closest={nearest:0.00}, path={agent.pathStatus}, remaining={agent.remainingDistance:0.00}");
                if (strict) Require(flips < 6 && nearest < 1.7f, "Pillar approach oscillated or failed: " + report);
                Object.Destroy(fixture); yield return null;
            }
        }
        File.WriteAllText(strict ? "Logs/pillar-probe-fixed.txt" : "Logs/pillar-probe-baseline.txt", report.ToString());
        player.transform.position = originalPosition;
        Physics2D.SyncTransforms();
    }
    public static void RunCrowdBaseline() => Run("crowd-baseline");
    public static void RunCrowdFixed() => Run("crowd-fixed");
    static IEnumerator CrowdProbe(bool strict)
    {
        Object.FindObjectOfType<WaveController>().enabled = false;
        var player = Object.FindObjectOfType<PlayerHealth>();
        player.GetComponent<DamageReceiver>().SetInvincible(true);
        var playerBody = player.GetComponent<Collider2D>();
        var originalPosition = player.transform.position;
        var offset = playerBody.bounds.center - player.transform.position;
        var pillars = GameObject.Find("MapCollision/Peristele").GetComponentsInChildren<BoxCollider2D>().Where(c => c.gameObject.layer == 0).ToArray();
        var report = new StringBuilder();
        foreach (var pillar in pillars)
        {
            player.transform.position = pillar.bounds.center + Vector3.left * (pillar.bounds.extents.x + playerBody.bounds.extents.x + 0.03f) - offset;
            Physics2D.SyncTransforms();
            string[] names = { "Gobling", "GoblinRunner", "GoblinArcher", "GoblinArcher" };
            var fixtures = new GameObject[4]; var facing = new bool[4]; var flips = new int[4];
            for (int i = 0; i < fixtures.Length; i++)
            {
                fixtures[i] = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/" + names[i] + ".prefab"), pillar.bounds.center + new Vector3(2 + i * 0.3f, (i - 1.5f) * 0.8f, 0), Quaternion.identity);
                var agent = fixtures[i].GetComponent<NavMeshAgent>();
                var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                Require(NavMesh.SamplePosition(fixtures[i].transform.position, out NavMeshHit hit, 2f, filter) && agent.Warp(hit.position), "Crowd spawn failed");
                facing[i] = fixtures[i].GetComponentInChildren<SpriteRenderer>().flipX;
            }
            float until = Time.time + 3.5f;
            float closest = float.MaxValue;
            while (Time.time < until)
            {
                for (int i = 0; i < fixtures.Length; i++)
                {
                    bool current = fixtures[i].GetComponentInChildren<SpriteRenderer>().flipX;
                    if (current != facing[i]) flips[i]++;
                    facing[i] = current;
                    if (i < 2) closest = Mathf.Min(closest, Vector2.Distance(fixtures[i].transform.position, player.transform.position));
                }
                yield return null;
            }
            report.AppendLine($"Pillar={pillar.name}, facingFlips=[{string.Join(",", flips)}], closestMelee={closest:0.00}");
            File.WriteAllText(strict ? "Logs/pillar-crowd-fixed.txt" : "Logs/pillar-crowd-baseline.txt", report.ToString());
            if (strict) Require(flips.All(f => f < 12) && closest < 1.8f, "Crowd navigation still oscillates/stalls: " + report);
            foreach (var fixture in fixtures) Object.Destroy(fixture);
            yield return null;
        }
        player.transform.position = originalPosition;
        Physics2D.SyncTransforms();
    }

}

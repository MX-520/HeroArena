using UnityEngine;

/// <summary>Playback controls used only by the isolated character preview scene.</summary>
public sealed class CharacterIdlePreviewControls : MonoBehaviour
{
    public Animator[] animators;
    public Camera previewCamera;
    private bool paused;
    private float speed = 1f;

    private void LateUpdate()
    {
        if (previewCamera != null)
            previewCamera.orthographicSize = Mathf.Max(2.7f, 4.6f / previewCamera.aspect);
    }

    private void OnGUI()
    {
        float scale = Mathf.Max(0.65f, Screen.width / 1280f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
        var style = new GUIStyle(GUI.skin.label) { fontSize = 19, normal = { textColor = new Color(0.94f, 0.95f, 0.87f) } };
        GUI.Box(new Rect(15, 15, 1000, 105), GUIContent.none);
        GUI.Label(new Rect(30, 24, 900, 28), "White-haired hero / four-direction idle trial", style);
        if (GUI.Button(new Rect(30, 65, 100, 35), paused ? "Play" : "Pause"))
        {
            paused = !paused;
            ApplySpeed();
        }
        if (GUI.Button(new Rect(145, 65, 120, 35), "Next frame"))
        {
            paused = true;
            float phase = animators[0].GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            int step = (Mathf.FloorToInt(phase * 8) + 1) % 8;
            ApplySpeed();
            foreach (var animator in animators)
            {
                animator.Play("Idle", 0, step / 8f + 0.015f);
                animator.Update(0);
            }
        }
        GUI.Label(new Rect(290, 68, 110, 30), "Speed " + speed.ToString("0.00") + "x");
        float next = GUI.HorizontalSlider(new Rect(410, 78, 220, 25), speed, 0.25f, 2f);
        if (!Mathf.Approximately(next, speed)) { speed = next; ApplySpeed(); }
        GUI.Label(new Rect(660, 68, 330, 35), "Extracted trial frames / not final sprites");
        GUI.matrix = previous;
    }

    private void ApplySpeed()
    {
        foreach (var animator in animators)
            if (animator != null) animator.speed = paused ? 0 : speed;
    }
}

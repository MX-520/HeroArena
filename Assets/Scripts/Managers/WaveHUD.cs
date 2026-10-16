using UnityEngine;
using UnityEngine.InputSystem;

public class WaveHUD : MonoBehaviour
{
    [SerializeField] private WaveController challenge;
    [SerializeField] private UnityEngine.UI.Text status;

    private void OnEnable()
    {
        if (challenge == null || status == null)
            return;
        challenge.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (challenge != null)
            challenge.Changed -= Refresh;
    }

    private void Update()
    {
        if (challenge != null &&
            (challenge.State == WaveController.ChallengeState.Completed || challenge.State == WaveController.ChallengeState.Failed) &&
            Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            challenge.RestartChallenge();
    }

    private void Refresh()
    {
        switch (challenge.State)
        {
            case WaveController.ChallengeState.Fighting:
                status.text = $"WAVE {challenge.CurrentWave} / {challenge.TotalWaves}   |   ENEMIES {challenge.RemainingEnemies}";
                break;
            case WaveController.ChallengeState.Intermission:
                status.text = "WAVE CLEARED  |  Next wave in 3 seconds";
                break;
            case WaveController.ChallengeState.Completed:
                status.text = "CHALLENGE COMPLETE  |  R: Play waves again";
                break;
            case WaveController.ChallengeState.Failed:
                status.text = "CHALLENGE STOPPED  |  R: Retry";
                break;
            default:
                status.text = "GET READY  |  Defeat all enemies";
                break;
        }
    }
}

using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameFlow flow;
    [SerializeField] private Health health;
    [SerializeField] private PlayerEffects effects;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject modal;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private TMP_Text heading;
    [SerializeField] private TMP_Text detail;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text hint;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text timer;
    [SerializeField] private UnityEngine.UI.Image healthFill;
    [SerializeField] private TMP_Text[] buffTimes;
    private void OnEnable()
    {
        flow.Changed += Refresh;
        health.OnHealthChanged += HealthChanged;
        HealthChanged(health.CurrentHP, health.MaxHP);
        Refresh();
    }
    private void OnDisable() { flow.Changed -= Refresh; health.OnHealthChanged -= HealthChanged; }
    private void Update()
    {
        timer.text = $"{Mathf.FloorToInt(flow.RunSeconds / 60):00}:{Mathf.FloorToInt(flow.RunSeconds % 60):00}  /  {flow.Waves.TotalKills} KILLS";
        for (int i = 0; i < buffTimes.Length; i++)
        {
            float left = effects.Remaining((PickupKind)(i + 1));
            buffTimes[i].text = left > 0 ? $"{left:0.0}s" : "--";
            buffTimes[i].color = left > 0 ? new Color(1, 0.86f, 0.45f) : new Color(0.5f, 0.59f, 0.6f);
        }
    }
    private void HealthChanged(int current, int max)
    {
        healthText.text = $"HEALTH  {current} / {max}";
        healthFill.fillAmount = max > 0 ? (float)current / max : 0;
    }
    private void Refresh()
    {
        var state = flow.State;
        menu.SetActive(state == GameFlow.ScreenState.Menu);
        hud.SetActive(state != GameFlow.ScreenState.Menu);
        modal.SetActive(state == GameFlow.ScreenState.Paused || state == GameFlow.ScreenState.Victory || state == GameFlow.ScreenState.Defeat);
        resumeButton.SetActive(state == GameFlow.ScreenState.Paused);
        if (state == GameFlow.ScreenState.Paused)
        {
            heading.text = "TAKE A BREATH";
            detail.text = "The forest can wait.\nPress Esc to return to the fight.";
        }
        else if (state == GameFlow.ScreenState.Victory)
        {
            heading.text = "FOREST CLEARED";
            detail.text = $"All three waves defeated.\n{flow.Kills} enemies  /  {flow.RunSeconds:0.0} seconds";
        }
        else if (state == GameFlow.ScreenState.Defeat)
        {
            heading.text = "THE FOREST PREVAILS";
            detail.text = flow.Waves.State == WaveController.ChallengeState.Failed
                ? "Challenge interrupted. Please retry."
                : $"{flow.Kills} enemies defeated.\nRoll through danger. Dodge the marked shots.";
        }
        var waves = flow.Waves;
        waveText.text = $"WAVE {Mathf.Max(1, waves.CurrentWave)} / {waves.TotalWaves}  |  {waves.RemainingEnemies} LEFT";
        hint.text = waves.State == WaveController.ChallengeState.Intermission ? "WAVE CLEARED  /  Catch your breath" :
            waves.State == WaveController.ChallengeState.Preparing ? "GET READY" : waves.CurrentWaveTitle;
    }
}

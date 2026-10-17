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
    [SerializeField] private GameObject pauseButton;
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
        timer.text = $"{Mathf.FloorToInt(flow.RunSeconds / 60):00}:{Mathf.FloorToInt(flow.RunSeconds % 60):00}  /  击败 {flow.Waves.TotalKills}";
        for (int i = 0; i < buffTimes.Length; i++)
        {
            float left = effects.Remaining((PickupKind)(i + 1));
            buffTimes[i].text = left > 0 ? $"{left:0.0}秒" : "—";
            buffTimes[i].color = left > 0 ? new Color(1, 0.86f, 0.45f) : new Color(0.5f, 0.59f, 0.6f);
        }
    }
    private void HealthChanged(int current, int max)
    {
        healthText.text = $"生命　{current} / {max}";
        healthFill.fillAmount = max > 0 ? (float)current / max : 0;
    }
    private void Refresh()
    {
        var state = flow.State;
        menu.SetActive(state == GameFlow.ScreenState.Menu);
        hud.SetActive(state != GameFlow.ScreenState.Menu);
        modal.SetActive(state == GameFlow.ScreenState.Paused || state == GameFlow.ScreenState.Victory || state == GameFlow.ScreenState.Defeat);
        resumeButton.SetActive(state == GameFlow.ScreenState.Paused);
        pauseButton.SetActive(state == GameFlow.ScreenState.Playing);
        if (state == GameFlow.ScreenState.Paused)
        {
            heading.text = "已暂停";
            detail.text = "按 Esc 继续战斗。";
        }
        else if (state == GameFlow.ScreenState.Victory)
        {
            heading.text = "挑战成功";
            detail.text = $"已击退全部三波敌人。\n击败 {flow.Kills} 名敌人　用时 {flow.RunSeconds:0.0} 秒";
        }
        else if (state == GameFlow.ScreenState.Defeat)
        {
            heading.text = "挑战失败";
            detail.text = flow.Waves.State == WaveController.ChallengeState.Failed
                ? "挑战中断，请重新尝试。"
                : $"已击败 {flow.Kills} 名敌人。\n善用翻滚，避开预警弹道。";
        }
        var waves = flow.Waves;
        waveText.text = $"第 {Mathf.Max(1, waves.CurrentWave)} / {waves.TotalWaves} 波　剩余 {waves.RemainingEnemies}";
        hint.text = waves.State == WaveController.ChallengeState.Intermission ? "本波结束，稍作休整" :
            waves.State == WaveController.ChallengeState.Preparing ? "准备战斗" : waves.CurrentWaveTitle;
    }
}

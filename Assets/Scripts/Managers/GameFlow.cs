using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public enum ScreenState { Menu, Playing, Paused, Victory, Defeat }
    [SerializeField] private WaveController waves;
    [SerializeField] private PlayerHealth player;
    private static bool startAfterLoad;
    private UnityEngine.InputSystem.PlayerInput input;
    private PlayerMovement movement;
    private Health health;
    public ScreenState State { get; private set; }
    public float RunSeconds { get; private set; }
    public int Kills { get; private set; }
    public WaveController Waves => waves;
    public event Action Changed;

    private void Awake()
    {
        input = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        movement = player.GetComponent<PlayerMovement>();
        health = player.GetComponent<Health>();
    }
    private void OnEnable() { health.OnDied += Died; waves.Changed += WaveChanged; }
    private void OnDisable() { health.OnDied -= Died; waves.Changed -= WaveChanged; Time.timeScale = 1; }
    private void Start()
    {
        if (startAfterLoad) { startAfterLoad = false; BeginRun(); }
        else SetState(ScreenState.Menu);
    }
    private void Update()
    {
        if (State == ScreenState.Playing) RunSeconds += Time.deltaTime;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (State == ScreenState.Playing) Pause();
            else if (State == ScreenState.Paused) Resume();
        }
    }
    public void BeginRun()
    {
        if (State != ScreenState.Menu) return;
        RunSeconds = 0;
        Kills = 0;
        player.GetComponent<PlayerEffects>().Clear();
        health.RestoreFull();
        SetState(ScreenState.Playing);
        waves.RestartChallenge();
    }
    public void Pause() { if (State == ScreenState.Playing) SetState(ScreenState.Paused); }
    public void Resume() { if (State == ScreenState.Paused) SetState(ScreenState.Playing); }
    public void Restart()
    {
        startAfterLoad = true;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void MainMenu()
    {
        startAfterLoad = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void Died()
    {
        if (State != ScreenState.Playing) return;
        Kills = waves.TotalKills;
        waves.StopChallenge();
        SetState(ScreenState.Defeat);
    }
    private void WaveChanged()
    {
        if (State != ScreenState.Playing) return;
        Kills = waves.TotalKills;
        if (waves.State == WaveController.ChallengeState.Completed) SetState(ScreenState.Victory);
        else if (waves.State == WaveController.ChallengeState.Failed) SetState(ScreenState.Defeat);
        Changed?.Invoke();
    }
    private void SetState(ScreenState state)
    {
        State = state;
        bool playing = state == ScreenState.Playing;
        Time.timeScale = playing ? 1 : 0;
        movement.ResetInput();
        if (playing) input.ActivateInput(); else input.DeactivateInput();
        Changed?.Invoke();
    }
}

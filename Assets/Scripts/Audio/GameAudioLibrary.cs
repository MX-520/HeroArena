using System;
using UnityEngine;

public enum GameSound
{
    UIHover, UIClick, UIBack, WaveStart, PlayerShot, PlayerSwing, EnemySwing, Roll,
    Footstep, PlayerHurt, EnemyHurt, EnemyDeath, PlayerDeath, WoodBreak, StoneBreak,
    Pickup, Heal, MagicCast, MagicImpact, Victory, Defeat, MeleeImpact, Count
}

[CreateAssetMenu(menuName="HeroArena/Audio Library")]
public sealed class GameAudioLibrary : ScriptableObject
{
    [Serializable] public class Cue
    {
        public GameSound sound;
        public AudioClip[] clips;
        [Range(0,1)] public float gain=0.4f;
        public Vector2 pitch=new Vector2(0.96f,1.04f);
        public float cooldown=0.04f;
        public int voices=3;
        public bool unpaused;
        public int priority;
    }
    public bool musicEnabled=true;
    [Range(0,1)] public float musicGain=1;
    public AudioClip menuMusic,battleMusic;
    public Cue[] cues;
}

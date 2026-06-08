using UnityEngine;

public enum SoundID {
    Footstep,
    WoodCrack,
    PickUp,
    CoinPickUp,
    HealthPickUp,
    PlayerAttack,
    PlayerShoot,
    PlayerHit,
    PlayerDeath,
    PlayerDash,
    EnemySmash,
    EnemyShoot,
    EnemyHit,
    EnemyDeath,
    UIConfirm,
    UICancel,
    UIHover,
    Interact,
}

[System.Serializable]
public struct SoundEffect {
    public SoundID soundID;
    public AudioClip[] clips;
}

public class SoundLibrary : MonoBehaviour {
    public SoundEffect[] soundEffects;
    public AudioClip GetClip(SoundID soundID) {
        foreach (var soundEffect in soundEffects) {
            if (soundEffect.soundID == soundID) {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        return null;
    }
}

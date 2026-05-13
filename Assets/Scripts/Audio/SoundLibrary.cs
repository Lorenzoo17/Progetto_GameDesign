using UnityEngine;

public enum SoundID {
    Footstep,
    PlayerAttack,
    PlayerHit,
    EnemyHit,
    EnemySmash,
    EnemyShoot,
    UIConfirm,
    UICancel,
    WoodCrack,
    PickUp
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

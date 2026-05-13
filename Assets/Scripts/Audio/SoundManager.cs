using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager Instance;

    [SerializeField] private SoundLibrary sfxLibrary;
    [SerializeField] private AudioSource sfx2DSource;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos) {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public void PlaySound3D(SoundID soundID, Vector3 pos) {
        AudioClip clip = sfxLibrary.GetClip(soundID);
        PlaySound3D(clip, pos);
    }

    public void PlaySound2D(SoundID soundID, float volume = 1f) {
        if (sfx2DSource == null) return;

        AudioClip clip = sfxLibrary.GetClip(soundID);

        if (clip == null) return;

        sfx2DSource.PlayOneShot(clip, volume);
    }
}
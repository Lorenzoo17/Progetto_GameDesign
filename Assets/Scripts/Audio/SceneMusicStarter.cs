using UnityEngine;

public class SceneMusicStarter : MonoBehaviour {
    [SerializeField] private MusicID sceneMusic;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float volume = 1f;

    private void Start() {
        if (MusicManager.Instance == null) return;

        MusicManager.Instance.PlayMusic(sceneMusic, fadeDuration, volume);
    }
}

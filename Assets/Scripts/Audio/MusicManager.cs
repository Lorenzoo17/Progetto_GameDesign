using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    private Coroutine crossfadeCoroutine;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(MusicID musicID, float fadeDuration = 0.5f, float targetVolume = 1f) {
        if (musicLibrary == null || musicSource == null) return;

        AudioClip nextTrack = musicLibrary.GetClip(musicID);

        if (nextTrack == null) return;

        if (musicSource.clip == nextTrack && musicSource.isPlaying) {
            musicSource.volume = targetVolume;
            return;
        }

        if (crossfadeCoroutine != null) {
            StopCoroutine(crossfadeCoroutine);
        }

        crossfadeCoroutine = StartCoroutine(
            AnimateMusicCrossfade(nextTrack, fadeDuration, targetVolume)
        );
    }

    private IEnumerator AnimateMusicCrossfade( AudioClip nextTrack, float fadeDuration = 0.5f, float targetVolume = 1f) {
        float startVolume = musicSource.volume;

        float percent = 0f;

        while (percent < 1f) {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0f;

        while (percent < 1f) {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, percent);
            yield return null;
        }

        musicSource.volume = targetVolume;
        crossfadeCoroutine = null;
    }
}
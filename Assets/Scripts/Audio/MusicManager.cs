using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    [Header("Scene Music")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string hubSceneName = "HUB";
    [SerializeField] private string dungeonSceneName = "Basement1_generation"; // metti qui il nome reale della scena
    [Header("Volume Settings")]
    [SerializeField] private float mainMenuVolume = 0.25f;
    [SerializeField] private float hubVolume = 1.5f;
    [SerializeField] private float dungeonVolume = 0.15f;

    private Coroutine crossfadeCoroutine;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start() {
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == mainMenuSceneName) {
            PlayMusic(MusicID.MainMenu, fadeDuration: 0.5f, targetVolume: mainMenuVolume);
            return;
        }

        if (scene.name == hubSceneName) {
            PlayMusic(MusicID.Hub, fadeDuration: 0.8f, targetVolume: hubVolume);
            return;
        }

        if (scene.name == dungeonSceneName) {
            PlayMusic(MusicID.SewerDungeon, fadeDuration: 0.8f, targetVolume: dungeonVolume);
            return;
        }

        // Default music (sewer dungeon) for any other scene
        PlayMusic(MusicID.SewerDungeon, fadeDuration: 0.8f, targetVolume: dungeonVolume);
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

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f, float targetVolume = 1f) {
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
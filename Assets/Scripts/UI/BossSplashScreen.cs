using FirstGearGames.SmoothCameraShaker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossSplashScreen : MonoBehaviour {
    public static BossSplashScreen Instance { get; private set; }

    [Header("Splash Settings")]
    [SerializeField] private float splashScreenDuration = 2f;
    [SerializeField] private GameObject splashScreenLayout;
    [SerializeField] private TextMeshProUGUI bossNameTMPro;
    [SerializeField] private Image bossImage;

    [Header("Splash Arts")]
    [SerializeField] private GameObject bossSplash;
    [SerializeField] private GameObject playerSplash;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.5f;

    [SerializeField] private Vector2 bossStartPosition;
    [SerializeField] private Vector2 bossEndPosition;

    [SerializeField] private Vector2 playerStartPosition;
    [SerializeField] private Vector2 playerEndPosition;

    private RectTransform bossSplashRect;
    private RectTransform playerSplashRect;

    private Coroutine splashCoroutine;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (bossSplash != null)
            bossSplashRect = bossSplash.GetComponent<RectTransform>();

        if (playerSplash != null)
            playerSplashRect = playerSplash.GetComponent<RectTransform>();
    }

    // richiamato in BossRoom.cs
    public void SetBossSplashScreen(string bossName, Sprite bossSprite, BossFreezable bossFreezable) {
        splashScreenLayout.SetActive(true);

        bossNameTMPro.text = bossName;
        bossImage.sprite = bossSprite;

        if (splashCoroutine != null)
            StopCoroutine(splashCoroutine);

        splashCoroutine = StartCoroutine(SplashScreenRoutine(bossFreezable));
    }

    private System.Collections.IEnumerator SplashScreenRoutine(BossFreezable bossFreezable) {
        Player.Instance.playerMovement.StopPlayer();

        if (bossFreezable != null)
            bossFreezable.FreezeBoss();

        // splash art resettate in posizione iniziale
        if (bossSplashRect != null)
            bossSplashRect.anchoredPosition = bossStartPosition;

        if (playerSplashRect != null)
            playerSplashRect.anchoredPosition = playerStartPosition;

        // animazione di entrata
        yield return MoveSplashArtsRoutine();

        // resto della durata dello splash
        float remainingTime = splashScreenDuration - moveDuration;

        if (remainingTime > 0f)
            yield return new WaitForSecondsRealtime(remainingTime);

        Player.Instance.playerMovement.ResumePlayer();

        if (bossFreezable != null)
            bossFreezable.ResumeBoss();

        splashScreenLayout.SetActive(false);
        splashCoroutine = null;
    }

    private System.Collections.IEnumerator MoveSplashArtsRoutine() {
        float timer = 0f;

        while (timer < moveDuration) {
            timer += Time.unscaledDeltaTime;

            float t = timer / moveDuration;
            t = Mathf.Clamp01(t);

            // rende il movimento più morbido
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (bossSplashRect != null) {
                bossSplashRect.anchoredPosition = Vector2.Lerp(
                    bossStartPosition,
                    bossEndPosition,
                    smoothT
                );
            }

            if (playerSplashRect != null) {
                playerSplashRect.anchoredPosition = Vector2.Lerp(
                    playerStartPosition,
                    playerEndPosition,
                    smoothT
                );
            }

            yield return null;
        }

        // per sicurezza imposto esattamente le posizioni finali
        if (bossSplashRect != null)
            bossSplashRect.anchoredPosition = bossEndPosition;

        if (playerSplashRect != null)
            playerSplashRect.anchoredPosition = playerEndPosition;

        if(EffectManager.Instance != null) {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.UI));
        }
    }
}
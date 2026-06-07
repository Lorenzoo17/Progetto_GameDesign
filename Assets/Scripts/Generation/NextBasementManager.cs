using UnityEngine;

public class NextBasementManager : MonoBehaviour, IInteractable {
    [SerializeField] private string nextBasementSceneName;

    [Header("Prompt")]
    [SerializeField] private GameObject promptInterface;
    [SerializeField] private float promptAnimationDuration = 0.15f;
    [SerializeField] private float promptStartScale = 0.6f;
    [SerializeField] private float promptEndScale = 1f;
    private Coroutine promptCoroutine;
    private SpriteRenderer promptSpriteRenderer;
    private Vector3 promptOriginalScale;

    private void Awake() {
        if (promptInterface != null) {
            promptOriginalScale = promptInterface.transform.localScale;
            promptSpriteRenderer = promptInterface.GetComponent<SpriteRenderer>();
            promptInterface.SetActive(false);
        }
    }

    public void ShowPrompt() {
        if (promptInterface == null) return;
        if (!isActiveAndEnabled) return;
        if (promptCoroutine != null) {
            StopCoroutine(promptCoroutine);
        }

        promptCoroutine = StartCoroutine(AnimateShowPrompt());
    }
    public void HidePrompt() {
        if (promptInterface == null) return;
        
        if (promptCoroutine != null) {
            StopCoroutine(promptCoroutine);
        }

        if (!gameObject.activeInHierarchy || !enabled) {
            HidePromptImmediate();
            return;
        }

        promptCoroutine = StartCoroutine(AnimateHidePrompt());
    }

    private System.Collections.IEnumerator AnimateShowPrompt() {
        promptInterface.SetActive(true);

        float elapsed = 0f;

        Vector3 startScale = promptOriginalScale * promptStartScale;
        Vector3 endScale = promptOriginalScale * promptEndScale;

        promptInterface.transform.localScale = startScale;
        SetPromptAlpha(0f);

        while (elapsed < promptAnimationDuration) {
            elapsed += Time.deltaTime;

            float t = elapsed / promptAnimationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            promptInterface.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            SetPromptAlpha(t);

            yield return null;
        }

        promptInterface.transform.localScale = endScale;
        SetPromptAlpha(1f);
    }

    private System.Collections.IEnumerator AnimateHidePrompt() {
        float elapsed = 0f;

        Vector3 startScale = promptInterface.transform.localScale;
        Vector3 endScale = promptOriginalScale * promptStartScale;

        while (elapsed < promptAnimationDuration) {
            elapsed += Time.deltaTime;

            float t = elapsed / promptAnimationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            promptInterface.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            SetPromptAlpha(1f - t);

            yield return null;
        }

        promptInterface.transform.localScale = promptOriginalScale;
        SetPromptAlpha(1f);
        promptInterface.SetActive(false);
    }

    private void SetPromptAlpha(float alpha) {
        if (promptSpriteRenderer == null) return;

        Color color = promptSpriteRenderer.color;
        color.a = alpha;
        promptSpriteRenderer.color = color;
    }

    public void Interact() {
        HidePromptImmediate();
        if (LevelLoader.Instance != null) {
            LevelLoader.Instance.LoadNextScene(nextBasementSceneName);
        }
    }
    private void HidePromptImmediate() {
        if (promptInterface == null) return;

        promptInterface.SetActive(false);
        promptInterface.transform.localScale = promptOriginalScale;
        SetPromptAlpha(1f);
    }

    private void OnDisable() {
        if (promptCoroutine != null) {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }

        HidePromptImmediate();
    }
}

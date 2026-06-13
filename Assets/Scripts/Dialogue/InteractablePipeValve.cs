using System.Collections;
using UnityEngine;

public class InteractablePipeValve : MonoBehaviour, IInteractable
{
    [Header("Impostazioni Sabotaggio")]
    [Tooltip("0 = Alto, 1 = Destra, 2 = Sinistra")]
    [SerializeField] private int pipeGroupId = 0;

    [Header("Prompt Interface")]
    [SerializeField] private GameObject promptInterface;
    [SerializeField] private float promptAnimationDuration = 0.15f;
    [SerializeField] private float promptStartScale = 0.6f;
    [SerializeField] private float promptEndScale = 1f;

    private Coroutine promptCoroutine;
    private SpriteRenderer promptSpriteRenderer;
    private Vector3 promptOriginalScale;
    private bool isAlreadyUsed = false;


    private void Awake()
    {
        if (promptInterface != null)
        {
            promptOriginalScale = promptInterface.transform.localScale;
            promptSpriteRenderer = promptInterface.GetComponent<SpriteRenderer>();
            promptInterface.SetActive(false);
        }
    }

    // ==========================================
    // LOGICA DI INTERAZIONE (Quando premi F)
    // ==========================================
    public void Interact()
    {
        if (isAlreadyUsed) return;

        isAlreadyUsed = true;
        HidePrompt();

        
        if (PipeManager.Instance != null)
        {
            PipeManager.Instance.LockRandomPipeInGroup(pipeGroupId);
        }
        else
        {
            Debug.LogWarning("PipeManager non trovato nella scena!");
        }
    }

    // ==========================================
    // GESTIONE VISIVA DEL PROMPT ('F')
    // ==========================================
    public void ShowPrompt()
    {
        if (promptInterface == null || isAlreadyUsed) return;

        if (promptCoroutine != null) StopCoroutine(promptCoroutine);
        promptCoroutine = StartCoroutine(AnimateShowPrompt());
    }

    public void HidePrompt()
    {
        if (promptInterface == null) return;

        if (promptCoroutine != null) StopCoroutine(promptCoroutine);
        promptCoroutine = StartCoroutine(AnimateHidePrompt());
    }

    private IEnumerator AnimateShowPrompt()
    {
        promptInterface.SetActive(true);
        float elapsed = 0f;

        Vector3 startScale = promptOriginalScale * promptStartScale;
        Vector3 endScale = promptOriginalScale * promptEndScale;

        promptInterface.transform.localScale = startScale;
        SetPromptAlpha(0f);

        while (elapsed < promptAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / promptAnimationDuration);

            promptInterface.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            SetPromptAlpha(t);
            yield return null;
        }

        promptInterface.transform.localScale = endScale;
        SetPromptAlpha(1f);
    }

    private IEnumerator AnimateHidePrompt()
    {
        float elapsed = 0f;
        Vector3 startScale = promptInterface.transform.localScale;
        Vector3 endScale = promptOriginalScale * promptStartScale;

        while (elapsed < promptAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / promptAnimationDuration);

            promptInterface.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            SetPromptAlpha(1f - t);
            yield return null;
        }

        promptInterface.transform.localScale = promptOriginalScale;
        SetPromptAlpha(1f);
        promptInterface.SetActive(false);
    }

    private void SetPromptAlpha(float alpha)
    {
        if (promptSpriteRenderer == null) return;

        Color color = promptSpriteRenderer.color;
        color.a = alpha;
        promptSpriteRenderer.color = color;
    }
}
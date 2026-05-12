using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// informazioni sul personaggio da mostrare
[System.Serializable]
public class DialogueCharacter {
    public string name;
    public Sprite icon;
}

// linea di dialogo del personaggio
[System.Serializable]
public class DialogueLine {
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}

// lista delle linee di dialogo
[System.Serializable]
public class Dialogue {
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour, IInteractable {
    public Dialogue dialogue;

    [Header("Prompt")]
    [SerializeField] private GameObject promptInterface;
    [SerializeField] private float promptAnimationDuration = 0.15f;
    [SerializeField] private float promptStartScale = 0.6f;
    [SerializeField] private float promptEndScale = 1f;

    [Header("Look at player")]
    [SerializeField] private bool lookAtPlayer = false;
    [SerializeField] private bool invertFlipDirection;


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


    private void Update() {
        FlipBasedOnPlayer();
    }

    public void TriggerDialogue() {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    public void Interact() { // richiamato in playerInteract
        if (DialogueManager.Instance == null) {
            Debug.LogWarning("Dialogue managaer non presente nella scena!");
            return;
        }
        if (DialogueManager.Instance.isDialogueActive) return; // se c'e' gia' un dialogo in corso non permetto l'interazione
        TriggerDialogue();
        HidePrompt(); // nascondo prompt
    }

    public void ShowPrompt() {
        if (promptInterface == null) return;
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

        promptCoroutine = StartCoroutine(AnimateHidePrompt());
    }

    private IEnumerator AnimateShowPrompt() {
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

    private IEnumerator AnimateHidePrompt() {
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

    private void FlipBasedOnPlayer() {
        if (Player.Instance == null) return;

        bool flipDirection = invertFlipDirection ? true : false;

        if (Player.Instance.transform.position.x > transform.position.x) {
            this.GetComponent<SpriteRenderer>().flipX = flipDirection;
        }
        else {
            this.GetComponent<SpriteRenderer>().flipX = !flipDirection;
        }
    }

}
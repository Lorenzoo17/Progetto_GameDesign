using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance;

    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    public Animator animator;
    private int dialogueStartedFrame = -1; // visto che interagisco e vado avanti con il dialogo con lo stesso tasto
    private float canStartDialogueAgainTime = 0f;

    [SerializeField] private float restartDialogueDelay = 0.2f;

    private void Awake() {
        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();
    }

    private void Start() {
        StartCoroutine(SubscribeToInputManager());
    }

    private IEnumerator SubscribeToInputManager() {
        while (InputManager.Instance == null) {
            yield return null;
        }

        InputManager.Instance.OnInteractEvent -= Instance_OnInteractEvent;
        InputManager.Instance.OnInteractEvent += Instance_OnInteractEvent;
    }

    private void OnDestroy() {
        if (InputManager.Instance != null) {
            InputManager.Instance.OnInteractEvent -= Instance_OnInteractEvent;
        }
    }

    private void Instance_OnInteractEvent(object sender, System.EventArgs e) {
        if (!isDialogueActive) return;

        if (Time.frameCount == dialogueStartedFrame)
            return;

        DisplayNextDialogueLine();
    }

    public void StartDialogue(Dialogue dialogue) {
        if (isDialogueActive) return;

        if (Time.time < canStartDialogueAgainTime) // per evitare di far ripartire subito il dialogo se premo di continuo F
            return;

        isDialogueActive = true;
        dialogueStartedFrame = Time.frameCount;

        if (Player.Instance != null) {
            Player.Instance.playerMovement.StopPlayer();
            Player.Instance.playerAttack.BlockAttack();
        }

        animator.Play("show"); // mostro il box di dialogo

        lines.Clear(); // pulisco le linee di testo precedenti

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines) {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine(); // mostro prima linea di dialogo
    }

    public void DisplayNextDialogueLine() {
        if (lines.Count == 0) {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue(); // prendo la linea di dialogo successiva

        // aggiorno icona e nome 
        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;

        StopAllCoroutines();

        StartCoroutine(TypeSentence(currentLine)); // animazione per mostare il testo della linea di dialogo
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine) {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray()) {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void EndDialogue() {
        isDialogueActive = false;

        canStartDialogueAgainTime = Time.time + restartDialogueDelay;

        if (Player.Instance != null) {
            Player.Instance.playerMovement.ResumePlayer();
            Player.Instance.playerAttack.UnlockAttack();
        }

        animator.Play("hide"); // nascondo box di dialogo
    }
}

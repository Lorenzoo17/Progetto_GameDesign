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

    private void Awake() {
        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();
    }

    public void StartDialogue(Dialogue dialogue) {
        isDialogueActive = true;

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
        animator.Play("hide"); // nascondo il box di dialogo
    }
}

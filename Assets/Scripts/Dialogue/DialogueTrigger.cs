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

public class DialogueTrigger : MonoBehaviour {
    public Dialogue dialogue;

    public void TriggerDialogue() {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<Player>() != null) {
            TriggerDialogue();
        }
    }
}
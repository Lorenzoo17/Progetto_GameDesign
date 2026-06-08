using UnityEngine;

public class PlayerInteract : MonoBehaviour {
    public IInteractable currentInteractableEntity;

    private void Start() {
        if(InputManager.Instance != null) {
            InputManager.Instance.OnInteractEvent += Instance_OnInteractEvent;
        }
    }

    private void Instance_OnInteractEvent(object sender, System.EventArgs e) {
        if (currentInteractableEntity != null) {

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D(SoundID.Interact, .25f);
            }

            currentInteractableEntity.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<IInteractable>() != null) {
            currentInteractableEntity = other.GetComponent<IInteractable>();
            currentInteractableEntity.ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.TryGetComponent<IInteractable>(out IInteractable interactableEntity)) {
            if (interactableEntity == currentInteractableEntity) {
                if (currentInteractableEntity is MonoBehaviour monoBehaviour && monoBehaviour.isActiveAndEnabled) {
                    currentInteractableEntity.HidePrompt();
                }
                currentInteractableEntity.HidePrompt();
                currentInteractableEntity = null;
            }
        }
    }
}

using TMPro;
using UnityEngine;

public class SellingSlot : MonoBehaviour, IInteractable {
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject promptInterface;
    public Transform itemPositioningTransform;

    private GameObject sellingItem;
    private bool sold;

    public void Interact() { // richiamato in playerInteract
        // qui bisognera' aggiornare MetaProgressionManager
        sold = true;
        Destroy(sellingItem); // per ora solo cosi
        HidePrompt();
    }

    public void SetUpSellingSlot(GameObject item, int itemPrice) { // poi id dell'oggetto o scriptableObject
        sellingItem = item;
        if(sellingItem.TryGetComponent<Collider2D>(out Collider2D c)) {
            c.enabled = false; // disabilito collider (dipende da che prefab si usa, in questo caso quello
                               // dell'arma effettiva, quindi cosi evito interazioni.
                               // in alternativa si puo' mettere altro prefabb fatto apposta o crearne uno con
                               // solo lo sprite renderer dell'oggetto. Appena si fatto gli SO)
        }
        sold = false;
        priceText.text = itemPrice.ToString();
    }

    public void ShowPrompt() {
        if (promptInterface == null || sold) return;
        promptInterface.SetActive(true);
    }
    public void HidePrompt() {
        if (promptInterface == null) return;
        promptInterface.SetActive(false);
    }
}

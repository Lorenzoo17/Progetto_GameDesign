using TMPro;
using UnityEngine;

public class SellingSlot : MonoBehaviour, IInteractable {
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject promptInterface;
    public Transform itemPositioningTransform;

    public SellingItemData sellingItem;
    private bool sold;

    public void Interact() { // richiamato in playerInteract
        if (sold) return;
        if (sellingItem == null) return;

        if (MetaProgressionManager.Instance == null) {
            Debug.LogWarning("MetaProgressionManager non presente");
            return;
        }

        bool canBuy = false;
        canBuy = MetaProgressionManager.Instance.SpendMutagenCoin(sellingItem.price);

        if (!canBuy) {
            Debug.Log("Monete insufficienti");
            return;
        }

        // si aggiorna MetaProgressionManager!
        UpdateMetaProgressionManager();

        sold = true;

        if (sellingItem.itemObject != null) {
            Destroy(sellingItem.itemObject);// si distrugge istanza
        }

        HidePrompt();
        Collider2D slotCollider = GetComponent<Collider2D>();
        if (slotCollider != null && slotCollider.isTrigger) {
            slotCollider.enabled = false;
        }

        if (priceText != null) {
            priceText.text = "";
        }
    }

    private void UpdateMetaProgressionManager() {
        switch (sellingItem.type) {
            case SellingItemType.Weapon:
                MetaProgressionManager.Instance.UnlockNewWeapon(sellingItem.id);
                break;
            case SellingItemType.Mutagen:
                MetaProgressionManager.Instance.UnlockNewMutagen(sellingItem.id);
                break;
            case SellingItemType.Perk:
                MetaProgressionManager.Instance.UnlockNewPerk(sellingItem.id);
                break;
        }
    }

    public void SetUpSellingSlot(GameObject itemObject, int itemPrice, string itemId, SellingItemType type) { // poi id dell'oggetto o scriptableObject
        sellingItem = new SellingItemData(itemId, itemObject, itemPrice, type);

        if(sellingItem.itemObject.TryGetComponent<Collider2D>(out Collider2D c)) {
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

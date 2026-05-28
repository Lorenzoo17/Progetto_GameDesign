using TMPro;
using UnityEngine;

public class SellingSlot : MonoBehaviour, IInteractable {
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject promptInterface;

    public Transform itemPositioningTransform;

    public SellingItemData sellingItem;

    private GameObject currentItemInstance;
    private bool sold;

    public void Interact() {
        if (sold) return;
        if (sellingItem == null) return;

        if (MetaProgressionManager.Instance == null) {
            Debug.LogWarning("MetaProgressionManager non presente");
            return;
        }

        bool canBuy;

        switch (sellingItem.purchaseMode) {
            case SellingPurchaseMode.UnlockMetaProgression:
                // per hub, spendo monete mutagene
                canBuy = MetaProgressionManager.Instance.SpendMutagenCoin(sellingItem.price);

                if (!canBuy) {
                    Debug.Log("Monete insufficienti");
                    return;
                }
                BuyAsMetaUnlock();
                break;

            case SellingPurchaseMode.ReleasePickup:
                // per dugeon, si spendono monete dungeon
                canBuy = MetaProgressionManager.Instance.SpendDungeonCoin(sellingItem.price);

                if (!canBuy) {
                    Debug.Log("Monete insufficienti");
                    return;
                }
                BuyAsDungeonPickup();
                break;
        }

        sold = true;

        HidePrompt();
        DisableSlotInteraction();

        if (priceText != null) {
            priceText.text = "";
        }
    }

    private void BuyAsMetaUnlock() {
        UpdateMetaProgressionManager();

        if (currentItemInstance != null) {
            Destroy(currentItemInstance);
        }
    }

    private void BuyAsDungeonPickup() {
        if (currentItemInstance == null)
            return;

        currentItemInstance.transform.SetParent(null);

        SetItemCollidersEnabled(currentItemInstance, true);

        Debug.Log($"Oggetto acquistato e ora raccoglibile: {sellingItem.id}");
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

    public void SetUpSellingSlot(GameObject itemInstance, SellingItemData itemData) {
        currentItemInstance = itemInstance;
        sellingItem = itemData;

        if (currentItemInstance != null) {
            SetItemCollidersEnabled(currentItemInstance, false);
            // se e' una moneta, evito che vada verso il player in automatico
            if(currentItemInstance.GetComponent<Coins>() != null) {
                currentItemInstance.GetComponent<Coins>().towardsPlayer = false;
            }
        }

        sold = false;

        if (priceText != null) {
            priceText.text = itemData.price.ToString();
        }
    }

    private void SetItemCollidersEnabled(GameObject item, bool enabled) {
        Collider2D[] colliders = item.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in colliders) {
            collider.enabled = enabled;
        }
    }

    private void DisableSlotInteraction() {
        Collider2D slotCollider = GetComponent<Collider2D>();

        if (slotCollider != null) {
            slotCollider.enabled = false;
        }
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
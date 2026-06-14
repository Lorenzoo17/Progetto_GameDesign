using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PerkPickup : MonoBehaviour, IInteractable {
    [Header("Visuals")]
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private GameObject lockedVisual;

    [Header("Prompt")]
    [SerializeField] private GameObject promptInterface;

    // ── state ──────────────────────────────────────────────
    private PerkPair[] _perkPairs = new PerkPair[3];
    private bool _used = false;
    private bool _locked = false;
    private PerkBase _grantedNegative = null;
    private RoomBehaviour _room;
    private System.Random random = new System.Random();

    public void SetUp(PerkPair[] perkPairs, RoomBehaviour room) {
        if (perkPairs == null || perkPairs.Length != 3) {
            Debug.LogError("PerkPickup.SetUp: deve ricevere esattamente 3 PerkPair");
            return;
        }

        _perkPairs = perkPairs;
        _room = room;

        activeVisual?.SetActive(true);
        lockedVisual?.SetActive(false);

        _room.OnRoomExit += HandleRoomExit;
        _room.OnRoomCleared += HandleRoomCleared;
    }

    private void OnDestroy() {
        if (_room == null) return;
        _room.OnRoomExit -= HandleRoomExit;
        _room.OnRoomCleared -= HandleRoomCleared;
    }

    public void Interact() {
        // Evita di riaprire il pannello mentre è già aperto
        if (PerkChoiceUIManager.Instance != null && PerkChoiceUIManager.Instance.IsShowing) {
            Debug.LogWarning("⚠️ PerkChoiceUI già aperta, ignoro Interact");
            return;
        }

        Debug.Log($"🔑 PerkPickup.Interact() - _used: {_used}, _locked: {_locked}, _perkPairs: {(_perkPairs != null ? "OK" : "NULL")}");

        if (_used || _locked || _perkPairs == null) {
            Debug.LogWarning($"⚠️ Interact bloccato: _used={_used}, _locked={_locked}, _perkPairs={(_perkPairs != null)}");
            return;
        }

        Debug.Log("✅ Calling PerkChoiceUIManager.ShowPerkChoices");
        PerkChoiceUIManager.Instance?.ShowPerkChoices(_perkPairs, this);
    }

    public void SelectPerk(PerkPair selectedPair) {
        if (selectedPair == null) return;

        _used = true;

        bool positive = random.NextDouble() < 0.35;
        PerkBase chosen = positive ? selectedPair.positive : selectedPair.negative;

        PerkController controller = FindFirstObjectByType<PerkController>();
        if (controller != null) {
            if (positive) {
                controller.AddPerk(selectedPair.positive);
            }
            else {
                controller.AddNegativePerk(selectedPair.negative, selectedPair.positive);
            }

            NotificationUI.Instance?.ShowMessage(
                $"You obtained {chosen.name}"
            );

            if (!positive) _grantedNegative = selectedPair.negative;
        }

        Lock();
    }

    public void ShowPrompt() {
        if (_used || _locked || promptInterface == null) return;
        promptInterface.SetActive(true);
    }

    public void HidePrompt() {
        if (promptInterface == null) return;
        promptInterface.SetActive(false);
    }

    private void HandleRoomExit(object sender, System.EventArgs e) {
        if (_used || _locked) return;

        NotificationUI.Instance?.ShowMessage(
            "You lost your chance"
        );

        Lock();
    }

    private void HandleRoomCleared(object sender, System.EventArgs e) {
        if (_grantedNegative == null) return;

        PerkController controller = FindFirstObjectByType<PerkController>();
        controller?.RemovePerk(_grantedNegative);
        _grantedNegative = null;
    }

    private void Lock() {
        _locked = true;
        HidePrompt();

        activeVisual.SetActive(false);
        lockedVisual.SetActive(true);

        GetComponent<Collider2D>().enabled = false;
    }
}
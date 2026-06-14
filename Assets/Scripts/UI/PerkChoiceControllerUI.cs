using UnityEngine;

public class PerkChoiceUIController : MonoBehaviour {
    [Header("UI Root")]
    [SerializeField] private GameObject uiRoot;

    [Header("Slots")]
    [SerializeField] private PerkChoiceSlotUI[] slots = new PerkChoiceSlotUI[3];

    private PerkPickup _currentPickup;
    private PerkPair[] _perkPairs;
    private float previousTimeScale = 1f;

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    private void Start() {

    }

    public void ShowChoices(PerkPair[] perkPairs, PerkPickup pickup) {
        // Evita che premendo F più volte venga sovrascritto previousTimeScale con 0
        if (_isOpen) {
            Debug.LogWarning("⚠️ PerkChoiceUI già aperta, ignoro ShowChoices");
            return;
        }

        Debug.Log($"📊 ShowChoices: ricevute {(perkPairs != null ? perkPairs.Length : 0)} perk pairs");

        if (perkPairs == null || perkPairs.Length != 3) {
            Debug.LogError("❌ Deve ricevere esattamente 3 PerkPair");
            return;
        }

        _isOpen = true;

        _perkPairs = perkPairs;
        _currentPickup = pickup;


        // Configura i 3 slot
        for (int i = 0; i < 3; i++) {
            Debug.Log($"  - Slot {i}: {perkPairs[i].name}");
            slots[i].SetUp(perkPairs[i], i, OnPerkSelected);
            Debug.Log($"  ✅ Slot {i} configurato");
        }

        // Debug: Controlla lo stato del Canvas
        var canvasComponent = uiRoot.GetComponentInParent<Canvas>();

        // Assicurati che il Canvas root sia attivo
        Canvas canvasRoot = uiRoot.GetComponent<Canvas>();

        // Mostra l'UI
        uiRoot.SetActive(true);

        // Mettiamo in pausa il gioco
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Disabilitiamo gli input del player
        if (InputManager.Instance != null) {
            InputManager.Instance.inputEnabled = false;
        }
    }

    private void OnPerkSelected(int slotIndex) {
        if (!_isOpen) return;

        Debug.Log($"🎯 Perk selezionato: slot {slotIndex}");

        if (_currentPickup != null && _perkPairs != null) {
            _currentPickup.SelectPerk(_perkPairs[slotIndex]);
        }

        HideChoices();
    }

    public void HideChoices() {
        if (!_isOpen)
            return;

        Debug.Log("❌ Nascondendo UI...");

        _isOpen = false;

        // Riprendi il gioco
        Time.timeScale = previousTimeScale;

        // Riabilita gli input del player
        if (InputManager.Instance != null) {
            InputManager.Instance.inputEnabled = true;
        }

        if (uiRoot != null)
            uiRoot.SetActive(false);

        _currentPickup = null;
        _perkPairs = null;
    }
}
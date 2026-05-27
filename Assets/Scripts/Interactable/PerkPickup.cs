using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PerkPickup : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    [SerializeField] private GameObject activeVisual;  // green pool sprite
    [SerializeField] private GameObject lockedVisual;  // caged sprite

    [Header("Prompt")]
    [SerializeField] private GameObject promptInterface;

    // ── state ──────────────────────────────────────────────
    private PerkPair _perkPair;
    private bool _used = false;
    private bool _locked = false;
    private PerkBase _grantedNegative = null;
    private RoomBehaviour _room;

    // ── Setup (called by the spawner) ──────────────────────

    public void SetUp(PerkPair pair, RoomBehaviour room)
    {
        _perkPair = pair;
        _room = room;

        activeVisual?.SetActive(true);
        lockedVisual?.SetActive(false);

        _room.OnRoomExit += HandleRoomExit;
        _room.OnRoomCleared += HandleRoomCleared;
    }

    private void OnDestroy()
    {
        if (_room == null) return;
        _room.OnRoomExit -= HandleRoomExit;
        _room.OnRoomCleared -= HandleRoomCleared;
    }

    // ── IInteractable ──────────────────────────────────────

    public void Interact()
    {
        if (_used || _locked || _perkPair == null) return;

        _used = true;

        bool positive = Random.value <= 0.6f;
        PerkBase chosen = positive ? _perkPair.positive : _perkPair.negative;

        PerkController controller = FindFirstObjectByType<PerkController>();
        if (controller != null)
        {
            controller.AddPerk(chosen);
            if (!positive) _grantedNegative = chosen;
        }

        Lock();
    }

    public void ShowPrompt()
    {
        if (_used || _locked || promptInterface == null) return;
        promptInterface.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptInterface == null) return;
        promptInterface.SetActive(false);
    }

    // ── Room event handlers ────────────────────────────────

    private void HandleRoomExit(object sender, System.EventArgs e)
    {
        if (_used) return;
        Lock();
    }

    private void HandleRoomCleared(object sender, System.EventArgs e)
    {
        if (_grantedNegative == null) return;

        PerkController controller = FindFirstObjectByType<PerkController>();
        controller?.RemovePerk(_grantedNegative);
        _grantedNegative = null;
    }

    // ── Helpers ────────────────────────────────────────────

    private void Lock()
    {
        _locked = true;
        HidePrompt();

        activeVisual.SetActive(false);
        lockedVisual.SetActive(true);

        GetComponent<Collider2D>().enabled = false;
    }
}

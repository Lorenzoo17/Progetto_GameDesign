using System;
using System.Collections.Generic;
using UnityEngine;

public class MutagenController : MonoBehaviour
{
    public event Action OnMutagenStateChanged;
    [Header("Equipped Mutagens")]
    [SerializeField] private MutagenSO equipped1;
    [SerializeField] private MutagenSO equipped2;

    [Header("Active Mutagens")]
    [SerializeField] private MutagenInstance active1;
    [SerializeField] private MutagenInstance active2;

    private Player player;
    private PlayerMana playerMana;
    private MutagenSO pendingMutagen;
    private MutagenItem pendingMutagenItem;
    public event Action<MutagenSO> OnRequestSlotSelection;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerMana = GetComponent<PlayerMana>();
    }

    public void RegisterInput()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMutagenPressed -= UseMutagenSlot;
            InputManager.Instance.OnMutagenPressed += UseMutagenSlot;
        }
    }

    public void UnregisterInput()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMutagenPressed -= UseMutagenSlot;
        }
    }

    private void HandleInteract(object sender, EventArgs e)
    {
        // Se c'è un mutagen in attesa di essere equipaggiato, mostra il menu di scelta
        if (pendingMutagen != null)
        {
            OnRequestSlotSelection?.Invoke(pendingMutagen);
        }
    }

    private void Update()
    {
        UpdateMutagens(Time.deltaTime);
    }

    // ======================================================
    // INPUT
    // ======================================================

    private void UseMutagenSlot(int slotIndex)
    {
        TryUseMutagen(slotIndex);
    }

    // ======================================================
    // EQUIPPED
    // ======================================================

    public void TryUseMutagen(int slotIndex)
    {
        MutagenSO mutagen = GetEquippedMutagen(slotIndex);

        if (mutagen == null)
        {
            Debug.Log("No mutagen equipped in this slot.");
            return;
        }

        TryActivateMutagen(mutagen);
    }

    public void EquipMutagen(MutagenSO mutagen, int slotIndex)
    {
        if (mutagen == null)
            return;

        switch (slotIndex)
        {
            case 0:
                equipped1 = mutagen;
                break;

            case 1:
                equipped2 = mutagen;
                break;
        }

        NotifyUI();
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        return slotIndex switch
        {
            0 => equipped1 == null,
            1 => equipped2 == null,
            _ => true
        };
    }

    public void UnequipMutagen(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0:
                equipped1 = null;
                break;

            case 1:
                equipped2 = null;
                break;
        }
    }

    private MutagenSO GetEquippedMutagen(int slotIndex)
    {
        return slotIndex switch
        {
            0 => equipped1,
            1 => equipped2,
            _ => null
        };
    }

    // ======================================================
    // ACTIVATION
    // ======================================================

    public bool TryActivateMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return false;

        // 1. Controllo se è già attivo
        if (IsMutagenActive(mutagen))
        {
            Debug.Log($"Mutagen already active: {mutagen.mutagenName}");
            return false;
        }

        // 2. Controllo il mana
        if (!playerMana.HasEnoughMana(mutagen.manaCost))
        {
            Debug.Log("Not enough mana.");
            return false;
        }

        // 3. Trova lo slot di equipaggiamento
        int slotIndex = GetMutagenSlotIndex(mutagen);
        if (slotIndex == -1)
        {
            Debug.Log("Mutagen is not equipped in any slot.");
            return false;
        }

        // 4. Crea l'istanza unica a runtime
        MutagenInstance instance = new MutagenInstance(mutagen);

        // 5. Attiva il mutagen UNA SOLA VOLTA
        bool hasActivated = mutagen.Activate(player, instance);

        if (!hasActivated)
            return false;

        // 6. Se l'attivazione è andata a buon fine, consuma il mana e assegna lo slot attivo
        playerMana.UseMana(mutagen.manaCost);
        SetActiveMutagen(slotIndex, instance);

        Debug.Log($"Activated mutagen: {mutagen.mutagenName}");
        NotifyUI();

        return true;
    }

    public void DeactivateMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return;

        int slotIndex = GetMutagenSlotIndex(mutagen);
        if (slotIndex == -1)
            return;

        MutagenInstance instance =
            GetActiveMutagen(slotIndex);

        if (instance == null)
            return;

        mutagen.Deactivate(player, instance);

        SetActiveMutagen(slotIndex, null);

        NotifyUI();

        Debug.Log($"Deactivated mutagen: {mutagen.mutagenName}");
    }

    private int GetMutagenSlotIndex(MutagenSO mutagen)
    {
        if (equipped1 == mutagen) return 0;
        if (equipped2 == mutagen) return 1;
        return -1;
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void UpdateMutagens(float deltaTime)
    {
        UpdateMutagen(active1, 0, deltaTime);
        UpdateMutagen(active2, 1, deltaTime);
    }

    private void UpdateMutagen(
        MutagenInstance mutagen,
        int slotIndex,
        float deltaTime)
    {
        if (mutagen == null || mutagen.source == null)
            return;

        // tick
        mutagen.source.Tick(player, mutagen, deltaTime);

        // duration
        mutagen.UpdateTime(deltaTime);

        // expiration
        if (mutagen.IsExpired)
        {
            mutagen.source.Deactivate(player, mutagen);

            Debug.Log($"Expired mutagen: {mutagen.source.mutagenName}");

            SetActiveMutagen(slotIndex, null);

            NotifyUI();
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private MutagenInstance GetActiveMutagen(int slotIndex)
    {
        return slotIndex switch
        {
            0 => active1,
            1 => active2,
            _ => null
        };
    }

    private void SetActiveMutagen(
        int slotIndex,
        MutagenInstance instance)
    {
        switch (slotIndex)
        {
            case 0:
                active1 = instance;
                break;

            case 1:
                active2 = instance;
                break;
        }
    }

    public bool IsMutagenActive(MutagenSO mutagen)
    {
        if (mutagen == null)
            return false;

        int slotIndex = GetMutagenSlotIndex(mutagen);
        if (slotIndex == -1)
            return false;

        MutagenInstance active =
            GetActiveMutagen(slotIndex);

        return active != null && active.source == mutagen;
    }

    public MutagenSO GetEquippedMutagenBySlot(int slotIndex)
    {
        return slotIndex switch
        {
            0 => equipped1,
            1 => equipped2,
            _ => null
        };
    }

    public HashSet<string> GetEquippedMutagenIds()
    {
        HashSet<string> equippedMutagenIds = new();

        AddEquippedMutagenId(equipped1, equippedMutagenIds);
        AddEquippedMutagenId(equipped2, equippedMutagenIds);

        return equippedMutagenIds;
    }

    public void RequestSlotSelection(MutagenSO mutagen)
    {
        // Temporaneamente salva il mutagen in attesa di scelta
        pendingMutagen = mutagen;
        OnRequestSlotSelection?.Invoke(mutagen);
    }

    private void AddEquippedMutagenId(MutagenSO mutagen, HashSet<string> ids)
    {
        if (mutagen == null)
            return;

        if (mutagen.mutagenLootData == null)
        {
            Debug.LogWarning($"Mutagene {mutagen.mutagenName} non ha MutagenLootData assegnato");
            return;
        }

        ids.Add(mutagen.mutagenLootData.id);
    }

    //Eventi
    private void NotifyUI()
    {
        OnMutagenStateChanged?.Invoke();
    }
}

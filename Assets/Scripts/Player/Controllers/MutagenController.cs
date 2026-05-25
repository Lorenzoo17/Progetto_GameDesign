using System;
using System.Collections.Generic;
using UnityEngine;

public class MutagenController : MonoBehaviour
{
    public event Action OnMutagenStateChanged;
    [Header("Equipped Mutagens")]
    [SerializeField] private MutagenSO equippedHead;
    [SerializeField] private MutagenSO equippedBody;
    [SerializeField] private MutagenSO equippedPaws;

    [Header("Active Mutagens")]
    [SerializeField] private MutagenInstance activeHead;
    [SerializeField] private MutagenInstance activeBody;
    [SerializeField] private MutagenInstance activePaws;

    private Player player;
    private PlayerMana playerMana;

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

    public void EquipMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return;

        switch (mutagen.bodyPart)
        {
            case MutagenBodyPart.Head:
                equippedHead = mutagen;
                break;

            case MutagenBodyPart.Body:
                equippedBody = mutagen;
                break;

            case MutagenBodyPart.Paws:
                equippedPaws = mutagen;
                break;
        }

        NotifyUI();
    }

    public void UnequipMutagen(MutagenBodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case MutagenBodyPart.Head:
                equippedHead = null;
                break;

            case MutagenBodyPart.Body:
                equippedBody = null;
                break;

            case MutagenBodyPart.Paws:
                equippedPaws = null;
                break;
        }
    }

    private MutagenSO GetEquippedMutagen(int slotIndex)
    {
        return slotIndex switch
        {
            0 => equippedHead,
            1 => equippedBody,
            2 => equippedPaws,
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

        // already active check
        if (IsMutagenActive(mutagen))
        {
            Debug.Log($"Mutagen already active: {mutagen.mutagenName}");
            return false;
        }

        // mana check
        if (!playerMana.HasEnoughMana(mutagen.manaCost))
        {
            Debug.Log("Not enough mana.");
            return false;
        }

        // activation validation
        MutagenInstance activationCheck = new MutagenInstance(mutagen);

        bool hasActivated =
            mutagen.Activate(player, activationCheck);

        if (!hasActivated)
            return false;

        // consume mana
        playerMana.UseMana(mutagen.manaCost);

        // runtime instance
        MutagenInstance instance =
            new MutagenInstance(mutagen);

        SetActiveMutagen(mutagen.bodyPart, instance);

        // activate
        mutagen.Activate(player, instance);

        Debug.Log($"Activated mutagen: {mutagen.mutagenName}");

        NotifyUI();

        return true;
    }

    public void DeactivateMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return;

        MutagenInstance instance =
            GetActiveMutagen(mutagen.bodyPart);

        if (instance == null)
            return;

        mutagen.Deactivate(player, instance);

        SetActiveMutagen(mutagen.bodyPart, null);

        NotifyUI();

        Debug.Log($"Deactivated mutagen: {mutagen.mutagenName}");
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void UpdateMutagens(float deltaTime)
    {
        UpdateMutagen(activeHead, MutagenBodyPart.Head, deltaTime);
        UpdateMutagen(activeBody, MutagenBodyPart.Body, deltaTime);
        UpdateMutagen(activePaws, MutagenBodyPart.Paws, deltaTime);
    }

    private void UpdateMutagen(
        MutagenInstance mutagen,
        MutagenBodyPart bodyPart,
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

            SetActiveMutagen(bodyPart, null);

            NotifyUI();
        }
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private MutagenInstance GetActiveMutagen(MutagenBodyPart bodyPart)
    {
        return bodyPart switch
        {
            MutagenBodyPart.Head => activeHead,
            MutagenBodyPart.Body => activeBody,
            MutagenBodyPart.Paws => activePaws,
            _ => null
        };
    }

    private void SetActiveMutagen(
        MutagenBodyPart bodyPart,
        MutagenInstance instance)
    {
        switch (bodyPart)
        {
            case MutagenBodyPart.Head:
                activeHead = instance;
                break;

            case MutagenBodyPart.Body:
                activeBody = instance;
                break;

            case MutagenBodyPart.Paws:
                activePaws = instance;
                break;
        }
    }

    public bool IsMutagenActive(MutagenSO mutagen)
    {
        if (mutagen == null)
            return false;

        MutagenInstance active =
            GetActiveMutagen(mutagen.bodyPart);

        return active != null && active.source == mutagen;
    }

    public MutagenSO GetEquippedMutagenByPart(MutagenBodyPart bodyPart)
    {
        return bodyPart switch
        {
            MutagenBodyPart.Head => equippedHead,

            MutagenBodyPart.Body => equippedBody,

            MutagenBodyPart.Paws => equippedPaws,

            _ => null
        };
    }

    public HashSet<string> GetEquippedMutagenIds() {
        HashSet<string> equippedMutagenIds = new();

        AddEquippedMutagenId(equippedHead, equippedMutagenIds);
        AddEquippedMutagenId(equippedBody, equippedMutagenIds);
        AddEquippedMutagenId(equippedPaws, equippedMutagenIds);

        return equippedMutagenIds;
    }

    private void AddEquippedMutagenId(MutagenSO mutagen, HashSet<string> ids) {
        if (mutagen == null)
            return;

        if (mutagen.mutagenLootData == null) {
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
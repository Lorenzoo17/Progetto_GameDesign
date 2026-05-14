using System.Collections.Generic;
using UnityEngine;

public class MutagenController : MonoBehaviour
{
    [Header("Equipped Mutagens")]
    [SerializeField]
    private List<MutagenSO> equippedMutagens = new();

    [Header("Active Mutagens")]
    [SerializeField]
    private List<MutagenInstance> activeMutagens = new();

    [Header("Settings")]
    [SerializeField]
    private int maxActiveMutagens = 3;

    private Player player;
    private PlayerMana playerMana;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerMana = GetComponent<PlayerMana>();
    }

    private void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMutagenPressed += UseMutagenSlot;
        }
    }

    private void OnDestroy()
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
    // EQUIPPED MUTAGENS
    // ======================================================

    public void TryUseMutagen(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedMutagens.Count)
        {
            Debug.Log("Invalid mutagen slot.");
            return;
        }

        MutagenSO mutagen = equippedMutagens[slotIndex];

        if (mutagen == null)
        {
            Debug.Log("No mutagen equipped in this slot.");
            return;
        }

        TryActivateMutagen(mutagen);
    }

    public void EquipMutagen(MutagenSO mutagen)
    {
        if (equippedMutagens.Count >= 3)
        {
            Debug.Log("Maximum equipped mutagens reached.");
            return;
        }

        equippedMutagens.Add(mutagen);
    }

    public void UnequipMutagen(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedMutagens.Count)
            return;

        equippedMutagens.RemoveAt(slotIndex);
    }

    public List<MutagenSO> GetEquippedMutagens()
    {
        return equippedMutagens;
    }

    // ======================================================
    // ACTIVATION
    // ======================================================

    public bool TryActivateMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return false;

        // max active mutagens
        if (activeMutagens.Count >= maxActiveMutagens)
        {
            Debug.Log("Maximum active mutagens reached.");
            return false;
        }

        // already active check
        foreach (var active in activeMutagens)
        {
            if (active.source == mutagen)
            {
                Debug.Log($"Mutagen already active: {mutagen.mutagenName}");
                return false;
            }
        }
        


        // mana check
        if (!playerMana.HasEnoughMana(mutagen.manaCost))
        {
            Debug.Log("Not enough mana.");
            return false;
        }

        // activation check
        MutagenInstance activationCheck = new MutagenInstance(mutagen);
        bool hasActivated = mutagen.Activate(player, activationCheck);

        if (!hasActivated)
            return false;

        // consume mana
        playerMana.UseMana(mutagen.manaCost);

        // create runtime instance
        MutagenInstance instance = new MutagenInstance(mutagen);

        activeMutagens.Add(instance);

        // activate logic
        mutagen.Activate(player, instance);

        Debug.Log($"Activated mutagen: {mutagen.mutagenName}");

        return true;
    }

    public void DeactivateMutagen(MutagenSO mutagen)
    {
        if (mutagen == null)
            return;

        MutagenInstance instance =
            activeMutagens.Find(m => m.source == mutagen);

        if (instance == null)
            return;

        mutagen.Deactivate(player, instance);

        activeMutagens.Remove(instance);

        Debug.Log($"Deactivated mutagen: {mutagen.mutagenName}");
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void UpdateMutagens(float deltaTime)
    {
        for (int i = activeMutagens.Count - 1; i >= 0; i--)
        {
            MutagenInstance mutagen = activeMutagens[i];

            if (mutagen == null || mutagen.source == null)
                continue;

            // tick update
            mutagen.source.Tick(player, mutagen, deltaTime);

            // duration update
            mutagen.UpdateTime(deltaTime);

            // expiration
            if (mutagen.IsExpired)
            {
                mutagen.source.Deactivate(player, mutagen);

                Debug.Log($"Expired mutagen: {mutagen.source.mutagenName}");

                activeMutagens.RemoveAt(i);
            }
        }
    }

    // ======================================================
    // GETTERS
    // ======================================================

    public List<MutagenInstance> GetActiveMutagens()
    {
        return activeMutagens;
    }

    public bool IsMutagenActive(MutagenSO mutagen)
    {
        foreach (var active in activeMutagens)
        {
            if (active.source == mutagen)
            {
                return true;
            }
        }

        return false;
    }
}

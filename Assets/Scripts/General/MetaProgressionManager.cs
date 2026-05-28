using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaProgressionManager : MonoBehaviour {
    // script usato per la gestione delle risore che devono rimanere persistenti tra le scene (e salvate in locale)
    // Queste risorse sono:
    // monete mutagene
    // mutageni equipaggiati e numero di mutageni che e' possibile equipaggiare
    // perk sbloccati (che entrano nel pool di quelli che e' possibile trovare)
    // armi sbloccate (che entrano nel pool di quelle che e' possibile trovare)

    // MutagenCoin
    public int MutagenCoin;// monete mutagene
    public int DungeonCoin; // monete dungeon -> resettare quando si ritorna in hub o quando si entra in dungeon (fatto in GameOverManager)
    [SerializeField] private LootDatabase lootDatabase; // database di tutte le armi e mutageni e perk
    public event EventHandler OnMutagenCoinChanged;
    public event EventHandler OnDungeonCoinChanged;

    private int maxEquippedMutagens = 1; // numero massimo di mutageni che e' possibile equipaggiare (cambiano in base a 
    // numero di boss sconfitti o comprati o altro da vedere) di default per ora 1

    // Perk, armi, mutageni
    private HashSet<string> unlockedPerks = new(); // lista dei perk sbloccati
    private HashSet<string> unlockedWeapons = new(); // lista delle armi sbloccate
    private HashSet<string> unlockedMutagens = new(); // lista mutageni sbloccati
    private HashSet<string> equippedMutagens = new(); // lista mutageni equipaggiati dal player nell'hub

    private HashSet<string> defaultWeapons = new();

    public event EventHandler OnMetaProgressionChanged;

    public static MetaProgressionManager Instance {  get; private set; }

    [Header("DEBUG - Runtime State")]
    [SerializeField] private List<string> debugUnlockedWeapons = new();
    [SerializeField] private List<string> debugUnlockedPerks = new();
    [SerializeField] private List<string> debugUnlockedMutagens = new();
    [SerializeField] private List<string> debugEquippedMutagens = new();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UnlockDefaultItems();
    }

    public void AddDungeonCoin(int amount) {
        DungeonCoin += amount;

        OnDungeonCoinChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool SpendDungeonCoin(int amount) {
        if (DungeonCoin < amount) return false;
        DungeonCoin -= amount;

        OnDungeonCoinChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    // richiamato quando ad esempio si sconfigge un boss (si raccolgono mutagenCoin)
    public void AddMutagenCoin(int amount) {
        MutagenCoin += amount;

        OnMutagenCoinChanged?.Invoke(this, EventArgs.Empty); // evento quando cambiano le mutagenCoin
    }

    // richiamato dal vendor dell'hub ad esempio
    public bool SpendMutagenCoin(int amount) {
        if(MutagenCoin < amount) return false;
        MutagenCoin -= amount;

        OnMutagenCoinChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    // quando si sblocca un nuovo perk (si compra dal vendor) bisogna richiamare questo metodo per
    // far si che si possa poi trovare nel dungeon (in quanto viene messo in questa lista)
    public void UnlockNewPerk(string perkNameId) {
        if (unlockedPerks.Add(perkNameId)) {
            RefreshDebugInspector();
            OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
        }
        else {
            Debug.LogWarning($"Perk {perkNameId} già sbloccato");
        }
    }
    public void UnlockNewWeapon(string weaponNameId) {
        if (unlockedWeapons.Add(weaponNameId)) {
            RefreshDebugInspector();
            OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
        }
        else {
            Debug.LogWarning($"Arma {weaponNameId} già sbloccata");
        }
    }

    public void UnlockNewMutagen(string mutagenId) {
        if (unlockedMutagens.Add(mutagenId)) {
            RefreshDebugInspector();
            OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
        }
        else {
            Debug.LogWarning($"Mutagene {mutagenId} già sbloccato");
        }
    }

    public bool EquipMutagen(string mutagenNameId) {
        if (!unlockedMutagens.Contains(mutagenNameId)) {
            Debug.LogWarning($"Mutagene {mutagenNameId} non ancora sbloccato!");
            return false;
        }

        if (equippedMutagens.Count >= maxEquippedMutagens || equippedMutagens.Contains(mutagenNameId)) {
            Debug.Log("Non puoi equipaggiare altri mutageni o il mutagene e' gia' equipaggiato!");
            // tipo di ritorno specifico poi per dare allarme anche a livello di UI
            return false;
        }
        equippedMutagens.Add(mutagenNameId);
        OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }
    public void UnequipMutagen(string id) {
        if (equippedMutagens.Remove(id)) {
            OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int GetMaxMutagenSlots() => maxEquippedMutagens;

    public void SetMaxMutagenSlots(int amount) {
        maxEquippedMutagens = amount;
        OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsWeaponUnlocked(string id) => unlockedWeapons.Contains(id);
    public bool IsPerkUnlocked(string id) => unlockedPerks.Contains(id);
    public bool IsMutagenEquipped(string id) => equippedMutagens.Contains(id);
    public bool IsMutagenUnlocked(string id) => unlockedMutagens.Contains(id);
    public List<string> GetUnlockedWeapons() => new(unlockedWeapons);
    public List<string> GetUnlockedPerks() => new(unlockedPerks);
    public List<string> GetUnlockedMutagens() => new(unlockedMutagens);
    public List<string> GetEquippedMutagens() {
        return new List<string>(equippedMutagens);
    }

    public List<string> GetDefaultWeapons() => new(defaultWeapons);

    // richiamato in awake e resetAll per sbloccare di default le armi iniziali
    private void UnlockDefaultItems() {
        if(lootDatabase == null) {
            Debug.LogWarning("LootDatabase non assegnato");
            return;
        }

        foreach (WeaponLootData weapon in lootDatabase.weapons) {
            if (weapon.unlockedByDefault) {
                unlockedWeapons.Add(weapon.id);
                defaultWeapons.Add(weapon.id); // aggiungo anche alla lista delle armi default (questa non si aggiornera')
            }
        }

        foreach (PerkLootData perk in lootDatabase.perks) {
            if (perk.unlockedByDefault)
                unlockedPerks.Add(perk.id);
        }

        foreach (MutagenLootData mutagen in lootDatabase.mutagens) {
            if (mutagen.unlockedByDefault)
                unlockedMutagens.Add(mutagen.id);
        }

        RefreshDebugInspector();
    }

    private void RefreshDebugInspector() {
        debugUnlockedWeapons = new List<string>(unlockedWeapons);
        debugUnlockedPerks = new List<string>(unlockedPerks);
        debugUnlockedMutagens = new List<string>(unlockedMutagens);
        debugEquippedMutagens = new List<string>(equippedMutagens);
    }

    public void ResetAll() {
        MutagenCoin = 0;

        unlockedWeapons.Clear();
        unlockedPerks.Clear();
        unlockedMutagens.Clear();
        equippedMutagens.Clear();
        defaultWeapons.Clear();

        maxEquippedMutagens = 1;

        UnlockDefaultItems();
        RefreshDebugInspector();

        OnMutagenCoinChanged?.Invoke(this, EventArgs.Empty);
        OnMetaProgressionChanged?.Invoke(this, EventArgs.Empty);
    }
}

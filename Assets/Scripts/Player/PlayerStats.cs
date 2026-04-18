using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

    // statistiche da impostare dall'editor o tramite ScriptableObject
    [SerializeField] private CharacterStats baseStats;
    public CharacterStats playerCurrentStats;

    private List<StatPerkSO> activePerks = new List<StatPerkSO>(); // perks attualmente attivi

    private void Awake() {
        // Inizializzo le statistiche
        playerCurrentStats = new CharacterStats(baseStats);
    }

    public List<StatPerkSO> GetActivePerks() {
        return activePerks;
    }

    public void AddPerk(StatPerkSO perk) {
        activePerks.Add(perk);

        UpdateStats(); // Aggiornamento statistiche
    }

    private void UpdateStats() {
        // reset
        playerCurrentStats = new CharacterStats(baseStats);

        foreach (var perk in activePerks) {

            switch (perk.statType) {

                case StatType.DodgeCoolDown:
                    ApplyModifier(ref playerCurrentStats, perk,
                        s => s.AddDodgeCooldown(perk.value),
                        s => s.MultiplyDodgeCooldown(1 + perk.value));
                    break;

                case StatType.Attack:
                    ApplyModifier(ref playerCurrentStats, perk,
                        s => s.AddAttack(perk.value),
                        s => s.MultiplyAttack(1 + perk.value));
                    break;

                case StatType.Speed:
                    ApplyModifier(ref playerCurrentStats, perk,
                        s => s.AddMoveSpeed(perk.value),
                        s => s.MultiplyMoveSpeed(1 + perk.value));
                    break;

                case StatType.AttackRate:
                    ApplyModifier(ref playerCurrentStats, perk,
                        s => s.AddAttackRate(perk.value),
                        s => s.MultiplyAttackRate(1 + perk.value));
                    break;
            }
        }
    }

    private void ApplyModifier(
        ref CharacterStats stats,
        StatPerkSO perk,
        Action<CharacterStats> flatAction,
        Action<CharacterStats> percentAction) {
            if (perk.modifierType == ModifierType.Flat)
                flatAction(stats);
            else
                percentAction(stats);
    }
}

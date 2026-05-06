using System.Collections.Generic;
using UnityEngine;

public class PerkGestors : MonoBehaviour
{
    [SerializeField] private List<StatPerkSO> initialPerks = new List<StatPerkSO>(); // Perk iniziali da Inspector
    private List<string> activePerks = new List<string>();

    private void Start()
    {
        // Aggiungi perk iniziali dall'Inspector
        foreach (var perk in initialPerks)
        {
            activePerks.Add(perk.perkName);
        }

        // Aggiungi perk attivi dal PlayerStats (raccolti durante il gioco)
        foreach (var perk in Player.Instance.playerStats.GetActivePerks())
        {
            if (!activePerks.Contains(perk.perkName))
            {
                activePerks.Add(perk.perkName);
            }
        }
    }

    public void AddPerk(string perkName)
    {
        if (!activePerks.Contains(perkName))
        {
            activePerks.Add(perkName);
        }
    }

    public void ApplyAttackEffects(GameObject target)
    {
        if (activePerks.Contains("Poison"))
        {
           Player.PlayerStats.updateStatsEvent.Invoke();
        }
    }
}

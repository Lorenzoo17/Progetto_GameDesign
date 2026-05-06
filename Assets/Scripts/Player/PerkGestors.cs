using System.Collections.Generic;
using UnityEngine;

public class PerkGestors : MonoBehaviour
{
    [SerializeField] private List<StatPerkSO> initialPerks = new List<StatPerkSO>(); // Perk iniziali da Inspector
    private HashSet<string> activePerks = new HashSet<string>();

    private void Start()
    {
        // Applico i perk iniziali del player e li aggiungo alle statistiche
        foreach (var perk in initialPerks)
        {
            if (perk == null) continue;

            if (!activePerks.Contains(perk.perkName))
            {
                Player.Instance.playerStats.AddPerk(perk);
            }
        }

        // Aggiungi eventuali perk raccolti durante il gioco
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
        activePerks.Add(perkName);
    }

    public void ApplyAttackEffects(GameObject target)
    {
        foreach (var perkName in activePerks)
        {
            switch (perkName)
            {
                case "Poison":
                    if (target.CompareTag("Enemy") && target.GetComponent<PoisonEffect>() == null)
                    {
                        target.AddComponent<PoisonEffect>();
                    }
                    break;
                // Aggiungi qui altri perk con i loro effetti
                // case "AltroPerk":
                //     // effetto
                //     break;
            }
        }
    }
}

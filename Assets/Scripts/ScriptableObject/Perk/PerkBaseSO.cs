using UnityEngine;
[CreateAssetMenu(menuName = "Perks")]
public abstract class PerkBase : ScriptableObject {

    public string perkName;

    [Header("Optional: link to a pair")]
    public PerkPair pair;

    public virtual void OnApply(Player player)
    {
        
    }
    public virtual void OnRemove(Player player) {}
}
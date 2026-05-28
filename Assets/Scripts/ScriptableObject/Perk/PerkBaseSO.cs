using UnityEngine;
public abstract class PerkBase : ScriptableObject
{

    public string perkName;

    [Header("Optional: link to a pair")]
    public PerkPair pair;

    public PerkLootData perkLootData; // usato in treasureRoomSpawner, per capire se il player ha gia'
    // il mutagene corrispondente equipaggiato

    public virtual void OnApply(Player player) { }
    public virtual void OnRemove(Player player) { }
}

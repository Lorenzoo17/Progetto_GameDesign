using UnityEngine;
[CreateAssetMenu(fileName = "new perk",menuName = "ScriptableObject/BasePerk")]
public  class PerkBase : ScriptableObject {

    public string perkName;

    [Header("Optional: link to a pair")]
    public PerkPair pair;

    public  void OnApply(Player player){}
    public  void OnRemove(Player player) {}
}

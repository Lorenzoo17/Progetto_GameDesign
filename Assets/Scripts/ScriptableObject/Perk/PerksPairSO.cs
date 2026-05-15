using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Perk Pair")]
public class PerkPair : ScriptableObject {

    public PerkBase negative;
    public PerkBase positive;
}
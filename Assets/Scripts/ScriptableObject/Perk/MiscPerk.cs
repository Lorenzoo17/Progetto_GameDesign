using UnityEngine;

[CreateAssetMenu(fileName = "new misc perk", menuName = "ScriptableObject/MiscPerk")]
public class MiscPerkSO : PerkBase
{
    public StatusType statusType;
    public DamageType damageType;
    public float value;

    public override void OnApply(Player player)
    {
        Debug.Log("Misc perk applied");
    }

    public void onUse(Player player) {
        // Implementa l'effetto del perk quando viene usato
        Debug.Log("Misc perk used");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log("Misc perk removed");
    }
}

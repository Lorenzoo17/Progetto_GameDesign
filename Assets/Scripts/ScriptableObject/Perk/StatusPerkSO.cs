using UnityEngine;

[CreateAssetMenu(fileName = "new status perk", menuName = "ScriptableObject/StatusPerk")]
public class StatusPerkSO : PerkBase
{
    // perk per gestire effetti vari che danno status (in generale sicuramente tutti i debuff)
    public StatusSO statusToApply;


    public override void OnApply(Player player)
    {
        Debug.Log("Status perk applied");
    }

    public void onUse(Player player)
    {
        // Implementa l'effetto del perk quando viene usato
        Debug.Log("Status perk used");
    }

    public override void OnRemove(Player player)
    {
        Debug.Log("Status perk removed");
    }

    public override string Description()
    {
        return $"Applies status: {statusToApply.statusName}";
    }
}

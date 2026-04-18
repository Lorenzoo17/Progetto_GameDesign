using UnityEngine;

[CreateAssetMenu(fileName = "new perk", menuName = "ScriptableObject/Perk")]
public class StatPerkSO : ScriptableObject {

    public string perkName;
    public StatType statType; // tipo di statistica da modificare
    public ModifierType modifierType; // tipo di modifica alla statistica (addittiva o percentuale)
    public float value; // valore relativo alla modifica
}

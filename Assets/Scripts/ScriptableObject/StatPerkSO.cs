using UnityEngine;

[CreateAssetMenu(fileName = "new perk", menuName = "ScriptableObject/StatPerk")]
public class StatPerkSO : ScriptableObject {

    public string perkName; // nome o id, che deve essere univoco! (vedi MetaProgressionManager.cs)
    public StatType statType; // tipo di statistica da modificare
    public ModifierType modifierType; // tipo di modifica alla statistica (addittiva o percentuale)
    public float value; // valore relativo alla modifica

    
}

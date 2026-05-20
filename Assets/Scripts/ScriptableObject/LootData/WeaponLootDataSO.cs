using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Loot/Weapon Loot Data")]
public class WeaponLootData : ScriptableObject {
    public string id;
    public int price = 100;
    public GameObject prefab;
    public bool unlockedByDefault;
}
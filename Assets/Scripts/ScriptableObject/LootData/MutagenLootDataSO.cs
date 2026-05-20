using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Loot/Mutagen Loot Data")]
public class MutagenLootData : ScriptableObject {
    public string id;
    public int price = 100;
    public GameObject prefab;
    public bool unlockedByDefault;
}


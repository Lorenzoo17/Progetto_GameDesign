using System.Collections.Generic;
using UnityEngine;

public class SpawnItems : MonoBehaviour {
    public static SpawnItems Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnItem(Vector2 position, GameObject source = null) {
        DropTable dropTable = GetDropTableFromSource(source);
        if (dropTable == null)
            return;

        SpawnFromDropTable(position, dropTable);
    }

    private DropTable GetDropTableFromSource(GameObject source) {
        if (source == null)
            return null;

        DropTable dropTable = source.GetComponent<DropTable>();
        if (dropTable != null)
            return dropTable;

        dropTable = source.GetComponentInChildren<DropTable>(true);
        if (dropTable != null)
            return dropTable;

        return source.GetComponentInParent<DropTable>();
    }

    private void SpawnFromDropTable(Vector2 position, DropTable dropTable) {
        if (dropTable == null)
            return;

        float chance = Mathf.Clamp01(dropTable.DropChance);
        if (!ShouldSpawnDrop(chance))
            return;

        List<DropEntry> entries = dropTable.DropEntries;
        if (entries == null || entries.Count == 0)
            return;

        GameObject selectedPrefab = GetRandomItem(entries);
        if (selectedPrefab == null)
            return;

        Instantiate(selectedPrefab, position, Quaternion.identity);
    }

    private bool ShouldSpawnDrop(float chance) {
        if (chance <= 0f)
            return false;

        if (chance >= 1f)
            return true;

        return Random.value <= chance;
    }

    private GameObject GetRandomItem(List<DropEntry> pool) {
        List<DropEntry> validEntries = new List<DropEntry>();
        float totalWeight = 0f;

        foreach (DropEntry item in pool) {
            if (item != null && item.prefab != null && item.weight > 0f) {
                validEntries.Add(item);
                totalWeight += item.weight;
            }
        }

        if (validEntries.Count == 0 || totalWeight <= 0f)
            return null;

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < validEntries.Count; i++) {
            DropEntry item = validEntries[i];
            currentWeight += item.weight;

            if (randomValue < currentWeight) {
                return item.prefab;
            }
        }

        return validEntries[validEntries.Count - 1].prefab;
    }
}
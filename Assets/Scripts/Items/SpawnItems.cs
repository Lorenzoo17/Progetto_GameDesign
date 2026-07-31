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

    public void SpawnItemBoss(Vector2 position, GameObject source = null) {
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
        float totalWeight = 0f;

        foreach (DropEntry item in pool) {
            if (item != null && item.prefab != null) {
                totalWeight += item.weight;
            }
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (DropEntry item in pool) {
            if (item == null || item.prefab == null)
                continue;

            currentWeight += item.weight;

            if (randomValue <= currentWeight) {
                return item.prefab;
            }
        }

        return null;
    }
}
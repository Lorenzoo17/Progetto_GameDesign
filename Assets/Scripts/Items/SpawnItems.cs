using UnityEngine;

public class SpawnItems : MonoBehaviour {
    public static SpawnItems Instance;

    [SerializeField] private GameObject[] itemsToSpawnPrefabs; // per ora prefabs
    // e tutti stessa probabilita'
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void SpawnItem(Vector2 position) {
        int chance = Random.Range(0, 1);

        if(chance == 0) {
            if (itemsToSpawnPrefabs != null && itemsToSpawnPrefabs.Length > 0) {
                int index = Random.Range(0, itemsToSpawnPrefabs.Length);
                GameObject item = Instantiate(itemsToSpawnPrefabs[index], position, Quaternion.identity);
            }
        }
    }
}

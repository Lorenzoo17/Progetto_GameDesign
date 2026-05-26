using UnityEngine;

public class SpawnItems : MonoBehaviour {
    public static SpawnItems Instance { get; private set; }

    [System.Serializable]
    private class DropItem {
        public string itemName;
        public GameObject prefab;

        [Min(0f)]
        public float weight; // peso che rappresenta probabilita' che l'oggetto spawni
    }

    [Header("Drop Settings")]
    [SerializeField, Range(0f, 1f)] private float globalDropChance = 0.4f; // probabilita' generale che un 
    // nemico, o altro, droppi un oggetto

    [SerializeField] private DropItem[] dropItems;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SpawnItem(Vector2 position, float localDropChance = -1f) {
        if (dropItems == null || dropItems.Length == 0)
            return;

        float chance = localDropChance < 0 ? globalDropChance : localDropChance;

        // possibilità generale che esca QUALCOSA
        if (Random.value > chance)
            return;

        GameObject selectedPrefab = GetRandomItem(); // si seleziona oggetto da spawnare in base ai pesi

        if (selectedPrefab == null)
            return;

        Instantiate(selectedPrefab, position, Quaternion.identity);
    }

    private GameObject GetRandomItem() {
        // si calcola totale dei pesi
        float totalWeight = 0f;

        foreach (DropItem item in dropItems) {
            if (item.prefab != null) {
                totalWeight += item.weight;
            }
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight); // si calcola valore random da 0 a total weight

        float currentWeight = 0f;

        // per ogni oggetto
        foreach (DropItem item in dropItems) {
            if (item.prefab == null)
                continue;

            currentWeight += item.weight; // si somma il suo peso

            // se il valore random e' minore del peso dell'oggetto, allora
            // si sceglie quell'oggetto da spawnare
            // (quindi oggetti con peso minore, hanno probabilita' minore di spawnare)
            if (randomValue <= currentWeight) {
                return item.prefab;
            }
        }

        return null;
    }
}
using UnityEngine;

public class PerkPickupSpawner : MonoBehaviour
{
    [Header("Pool of possible perk pairs")]
    [SerializeField] private PerkPair[] perkPairPool;

    [Header("Prefab")]
    [SerializeField] private GameObject perkPickupPrefab;
    private System.Random random = new System.Random();

    private void Start()
    {
        SpawnPickup();
    }

    private void SpawnPickup()
    {
        if (perkPairPool == null || perkPairPool.Length < 3)
        {
            Debug.LogWarning("PerkPickupSpawner: serve un pool di almeno 3 PerkPairs.");
            return;
        }

        if (perkPickupPrefab == null)
        {
            Debug.LogWarning("PerkPickupSpawner: no prefab assigned.");
            return;
        }

        // Seleziona 3 perkpair casuali senza duplicati
        PerkPair[] chosenPairs = SelectRandomPairs(3);

        // Instantiate and wire up
        GameObject go = Instantiate(perkPickupPrefab, transform.position, Quaternion.identity);

        if (go.TryGetComponent(out PerkPickup pickup))
        {
            RoomBehaviour room = GetComponentInParent<RoomBehaviour>();
            if (room == null)
                Debug.LogWarning("PerkPickupSpawner: no RoomBehaviour found in parent.");

            pickup.SetUp(chosenPairs, room);
        }
    }

    // Seleziona N perkpair casuali dal pool senza duplicati
    private PerkPair[] SelectRandomPairs(int count)
    {
        PerkPair[] result = new PerkPair[count];

        for (int i = 0; i < count; i++)
        {
            bool foundUnique = false;
            PerkPair candidate = null;

            while (!foundUnique)
            {
                candidate = perkPairPool[random.Next(0, perkPairPool.Length)];

                // Controlla che non sia già stata selezionata
                foundUnique = true;
                for (int j = 0; j < i; j++)
                {
                    if (result[j] == candidate)
                    {
                        foundUnique = false;
                        break;
                    }
                }
            }

            result[i] = candidate;
        }

        return result;
    }
}

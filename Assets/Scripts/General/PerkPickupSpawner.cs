using UnityEngine;

public class PerkPickupSpawner : MonoBehaviour
{
    [Header("Pool of possible perk pairs")]
    [SerializeField] private PerkPair[] perkPairPool;

    [Header("Prefab")]
    [SerializeField] private GameObject perkPickupPrefab;

    private void Start()
    {
        SpawnPickup();
    }

    private void SpawnPickup()
    {
        if (perkPairPool == null || perkPairPool.Length == 0)
        {
            Debug.LogWarning("PerkPickupSpawner: no PerkPairs assigned.");
            return;
        }

        if (perkPickupPrefab == null)
        {
            Debug.LogWarning("PerkPickupSpawner: no prefab assigned.");
            return;
        }

        // Pick a random pair from the pool
        PerkPair chosen = perkPairPool[Random.Range(0, perkPairPool.Length)];

        // Instantiate and wire up
        GameObject go = Instantiate(perkPickupPrefab, transform.position, Quaternion.identity);

        if (go.TryGetComponent(out PerkPickup pickup))
        {
            RoomBehaviour room = GetComponentInParent<RoomBehaviour>();
            if (room == null)
                Debug.LogWarning("PerkPickupSpawner: no RoomBehaviour found in parent.");

            pickup.SetUp(chosen, room);
        }
    }
}

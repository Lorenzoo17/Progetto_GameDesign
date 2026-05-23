using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static Transform currentSpawn;

    private void Awake()
    {
        currentSpawn = transform;
    }
}
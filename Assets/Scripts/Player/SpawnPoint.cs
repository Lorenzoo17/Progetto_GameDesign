using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static Transform hubSpawn;
    
    private void Awake()
    {
        hubSpawn = transform;
    }
}
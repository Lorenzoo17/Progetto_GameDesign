using UnityEngine;

public class EntitySpawner : MonoBehaviour, IInteractable
{
    [Header("Enemy")]
    [SerializeField] public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 2f;

    [Header("Prompt")]
    [SerializeField] private GameObject promptInterface;

    private bool hasSpawned = false;

    public void Interact()
    {
        if (hasSpawned)
            return;

        SpawnEnemies();

        HidePrompt();

        hasSpawned = true;
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle * spawnRadius;

            Vector3 spawnPosition =
                transform.position + (Vector3)randomOffset;

            Instantiate(
                enemyPrefabs[i],
                spawnPosition,
                Quaternion.identity
            );
        }
    }

    public void ShowPrompt()
    {
        if (promptInterface != null)
        {
            promptInterface.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptInterface != null)
        {
            promptInterface.SetActive(false);
        }
    }
}

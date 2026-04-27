using UnityEngine;

public class EnemySpawner : MonoBehaviour{

    public static EnemySpawner Instance {  get; private set; }

    [SerializeField] private GameObject[] enemiesToSpawnPrefabs; // prefab dei nemici da spawnare
    private Transform[] spawnPoints;
    private int minEnemiesToSpawn;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetSpawner(Transform[] spawnPoints, int minEnemiesToSpawn) {
        this.spawnPoints = spawnPoints;
        this.minEnemiesToSpawn = minEnemiesToSpawn;
    }

    public void SpawnEnemies() {
        if (minEnemiesToSpawn > spawnPoints.Length) {
            minEnemiesToSpawn = spawnPoints.Length;
        }

        int enemiesToSpawnNumber = Random.Range(minEnemiesToSpawn, spawnPoints.Length);

        for (int i = 0; i < enemiesToSpawnNumber; i++) {
            int enemyToSpawnIndex = Random.Range(0, enemiesToSpawnPrefabs.Length);
            // Animazione di spawn
            // Effetto di spawn in corrispondenza dello spawnpoint i-esimo

            // si spawna nemico indicato dall'indice nello spawn point i-esimo
            GameObject newEnemy = Instantiate(enemiesToSpawnPrefabs[enemyToSpawnIndex], spawnPoints[i].position, Quaternion.identity);
        }

        // Screen shake
    }
}

using UnityEngine;

public class EnemySpawner : MonoBehaviour{
    // TODO : da spostare nella room manager ? 
    public static EnemySpawner Instance {  get; private set; }

    [SerializeField] private GameObject[] enemiesToSpawnPrefabs; // prefab dei nemici da spawnare
    private Transform[] spawnPoints;
    private int minEnemiesToSpawn;

    private int currentEnemies;

    private RoomBehaviour currentRoom; // stanza corrente in cui il player e' entrato

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetCurrentRoom(RoomBehaviour room) {
        currentRoom = room;
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
        currentEnemies = enemiesToSpawnNumber;

        for (int i = 0; i < enemiesToSpawnNumber; i++) {
            int enemyToSpawnIndex = Random.Range(0, enemiesToSpawnPrefabs.Length);
            // Animazione di spawn
            // Effetto di spawn in corrispondenza dello spawnpoint i-esimo

            // si spawna nemico indicato dall'indice nello spawn point i-esimo
            GameObject newEnemy = Instantiate(enemiesToSpawnPrefabs[enemyToSpawnIndex], spawnPoints[i].position, Quaternion.identity);
            if(newEnemy.TryGetComponent<Enemy>(out Enemy enemy)) {
                enemy.SetEnemySpawner(this);
            }
        }

        // Screen shake
    }

    public void OnEnemyDeath() { // richiamato in Enemy.cs
        currentEnemies--;
        if(currentEnemies <= 0) {
            if(currentRoom != null) {
                currentRoom.RoomCleared();
            }
            else {
                Debug.LogWarning("Stanza corrente non assegnata!");
            }
        }
    }
}

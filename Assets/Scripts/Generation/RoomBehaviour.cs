using UnityEngine;

public class RoomBehaviour : MonoBehaviour {

    // 0 - Up, 1 - Down, 2 - Right, 3 - Left
    [SerializeField] private GameObject[] blocks; // muri
    [SerializeField] private GameObject[] doors;

    [SerializeField] private Transform cameraPoint; // punto centrale della stanza nella quale la camera si deve settare

    private bool[] doorExists = new bool[4]; // per indicare quale porta esiste effettivamente (settato durantte DungeonGenerator)

    private bool isVisited = false;
    private bool isCleared = false;

    public int enemiesAlive = 0;

    // Room generation parameters
    [SerializeField] private Transform[] enemiesSpawnPoints;
    [SerializeField] private Transform[] decorationSpawnPoints;
    [SerializeField] private int minEnemiesToSpawn = 3;

    [SerializeField] private bool isStartRoom = false;

    private void Awake() {
        // sicurezza
        // trova automaticamente tutte le porte nei figli

        Door[] foundDoors = GetComponentsInChildren<Door>(true);
    }

    private void Update() {
        // test
        if(enemiesAlive == 0) {
            RoomCleared();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            enemiesAlive = 0;
        }
    }

    // chiamato dal DungeonGenerator
    public void UpdateRoom(bool[] status) {

        for (int i = 0; i < 4; i++) {

            bool hasDoor = status[i];
            doorExists[i] = hasDoor;

            // muri
            blocks[i].SetActive(!hasDoor);

            // attiva solo porte esistenti
            doors[i].gameObject.SetActive(hasDoor);

            // tutte le porte sono settate come aperte all'inizio
            if (hasDoor) {
                doors[i].GetComponent<Door>().SetClosed(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.GetComponent<Player>()) return;

        Debug.Log($"{other.gameObject.name} triggered");

        // camera
        Camera.main.GetComponent<CameraDungeonBehaviour>().MoveToRoom(cameraPoint);

        if (!isVisited) {
            isVisited = true;

            if (!isStartRoom) {
                StartRoom();
            }
        }
    }

    // prima entrata
    private void StartRoom() {
        if (isStartRoom) return;

        CloseDoors();
        SpawnEnemies();
    }

    // chiudi SOLO porte esistenti
    private void CloseDoors() {
        for (int i = 0; i < 4; i++) {
            if (doorExists[i]) {
                doors[i].GetComponent<Door>().SetClosed(true);
            }
        }
    }

    // Si attivano solo porte effettivamente esistenti
    private void OpenDoors() {
        for (int i = 0; i < 4; i++) {
            if (doorExists[i]) {
                doors[i].GetComponent<Door>().SetClosed(false);
            }
        }
    }

    // spawn nemici (placeholder)
    private void SpawnEnemies() {
        // Sostituire con spawn effettivo
        enemiesAlive = 3;

        if (EnemySpawner.Instance == null) {
            Debug.Log("Enemy spawner non trovato");
            return;
        }

        EnemySpawner.Instance.SetSpawner(enemiesSpawnPoints, minEnemiesToSpawn);
        EnemySpawner.Instance.SpawnEnemies();
    }

    // chiamato dai nemici
    public void OnEnemyDeath() {
        enemiesAlive--;

        if (enemiesAlive <= 0 && !isCleared) {
            RoomCleared();
        }
    }

    // stanza completata
    private void RoomCleared() {
        isCleared = true;
        OpenDoors();
    }
}
using NavMeshPlus.Components;
using System;
using UnityEngine;

public enum RoomType
{
    StartRoom,
    EnemiesRoom,
    TrapRoom,
    TreasureRoom,
    VendorRoom,
    BossRoom
}
public class RoomBehaviour : MonoBehaviour
{

    [SerializeField] private RoomType roomType;

    // 0 - Up, 1 - Down, 2 - Right, 3 - Left
    [SerializeField] private GameObject[] blocks; // muri
    [SerializeField] private GameObject[] doors;

    // [SerializeField] private Transform cameraPoint; // punto centrale della stanza nella quale la camera si deve settare

    private bool[] doorExists = new bool[4]; // per indicare quale porta esiste effettivamente (settato durantte DungeonGenerator)

    private bool isVisited = false;
    private bool isCleared = false;

    // Room generation parameters
    [SerializeField] private Transform[] enemiesSpawnPoints;
    [SerializeField] private Transform[] decorationSpawnPoints;
    [SerializeField] private int minEnemiesToSpawn = 3;

    [SerializeField] private BoxCollider2D roomBounds;
    public Transform roomCentre;

    [SerializeField] private NavMeshSurface navSurface;

    // utilizzati da trapShooter ad esempio
    public event EventHandler OnRoomEnter;
    public event EventHandler OnRoomExit;
    public event EventHandler OnRoomCleared;
    private void Awake()
    {
        // sicurezza
        // trova automaticamente tutte le porte nei figli

        Door[] foundDoors = GetComponentsInChildren<Door>(true);
        roomBounds = roomBounds == null ? GetComponent<BoxCollider2D>() : roomBounds;
    }

    private void Start()
    {
        if (roomCentre == null)
        {
            roomCentre = transform;
        }

        BakeRoomNavMesh();
    }

    public void MarkAsVisited()
    {
        isVisited = true;
    }

    // chiamato dal DungeonGenerator
    public void UpdateRoom(bool[] status)
    {

        for (int i = 0; i < 4; i++)
        {

            bool hasDoor = status[i];
            doorExists[i] = hasDoor;

            // muri
            blocks[i].SetActive(!hasDoor);

            // attiva solo porte esistenti
            doors[i].gameObject.SetActive(hasDoor);

            // tutte le porte sono settate come aperte all'inizio
            if (hasDoor)
            {
                doors[i].GetComponent<Door>().SetClosed(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<Player>()) return;

        OnRoomEnter?.Invoke(this, EventArgs.Empty);

        if (DungeonGenerator.Instance != null && !DungeonGenerator.Instance.IsDungeonReady)
        {
            return;
        }

        if (Camera.main.TryGetComponent<CameraDungeonBehaviour>(out CameraDungeonBehaviour cdb))
        {
            cdb.SetRoomBounds(roomBounds);
        }

        if (!isVisited)
        {
            isVisited = true;

            if (roomType != RoomType.StartRoom)
            {
                StartRoom();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<Player>()) return;

        OnRoomExit?.Invoke(this, EventArgs.Empty);
    }

    // prima entrata
    private void StartRoom() {
        // per queste camere per ora non si chiudono semplicemente le porte
        if (roomType == RoomType.TrapRoom || roomType == RoomType.VendorRoom || roomType == RoomType.TreasureRoom) return;
        // startRoom anche rimane chiusa, in quanto si aspetta che il player raccolga l'arma iniziale

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SetCurrentRoom(this);
        }

        // per le altre stanze, chiudo le porte
        CloseDoors();
        // se necessario, spawno i nemici
        SpawnEnemies();
    }

    // chiudi SOLO porte esistenti
    public void CloseDoors()
    {
        for (int i = 0; i < 4; i++)
        {
            if (doorExists[i])
            {
                doors[i].GetComponent<Door>().SetClosed(true);
            }
        }
    }

    // Si attivano solo porte effettivamente esistenti
    public void OpenDoors()
    {
        for (int i = 0; i < 4; i++)
        {
            if (doorExists[i])
            {
                doors[i].GetComponent<Door>().SetClosed(false);
            }
        }
    }

    // spawn nemici (placeholder)
    private void SpawnEnemies()
    {
        if (EnemySpawner.Instance == null)
        {
            Debug.Log("Enemy spawner non trovato");
            return;
        }

        EnemySpawner.Instance.SetSpawner(enemiesSpawnPoints, minEnemiesToSpawn);
        EnemySpawner.Instance.SpawnEnemies();
    }

    // stanza completata
    public void RoomCleared()
    { // richiamato in enemyspawner (se la stanza ha nemici)
        isCleared = true;
        OpenDoors();
        OnRoomCleared?.Invoke(this, EventArgs.Empty);
    }

    public void BakeRoomNavMesh()
    {
        if (navSurface == null)
        {
            navSurface = GetComponentInChildren<NavMeshSurface>();
        }

        if (navSurface != null)
        {
            navSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning($"NavMeshSurface non trovata nella stanza {name}");
        }
    }
}

using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;

// 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

public class DungeonGenerator : MonoBehaviour {

    public class Cell {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    [System.Serializable]
    public class Rule {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public int ProbabilityOfSpawning(int x, int y) {

            if (x >= minPosition.x && x <= maxPosition.x &&
                y >= minPosition.y && y <= maxPosition.y) {
                return 1;
            }

            return 0;
        }
    }

    [Header("Dungeon Size")]
    public Vector2Int size;
    public Vector2 offset;

    [Header("Rooms")]
    public GameObject bossRoom;
    public GameObject powerUpRoom;
    public GameObject vendorRoom;
    public GameObject startRoom;

    private int startCell;

    public Rule[] normalRooms;

    private List<Cell> board;

    public static DungeonGenerator Instance { get; private set; }
    public bool IsDungeonReady { get; private set; }
    private RoomBehaviour startRoomBehaviour;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        IsDungeonReady = false;

        if (bossRoom == null || powerUpRoom == null || vendorRoom == null || startRoom == null || normalRooms.Length == 0) {
            Debug.Log("Stanze non assegnate correttamente!");
            return;
        }

        MazeGenerator();

        MovePlayerToStartRoom();

        if (startRoomBehaviour != null) {
            startRoomBehaviour.MarkAsVisited();
        }

        IsDungeonReady = true;
    }

    // GENERAZIONE DUNGEON
    void GenerateDungeon() {

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        // Trova celle valide
        List<int> validCells = new List<int>();

        for (int i = 0; i < board.Count; i++) {
            if (board[i].visited) {
                validCells.Add(i);
            }
        }

        // first room
        validCells.Remove(startCell);

        // Boss nella stanza più lontana
        int bossCell = GetFarthestCell();

        validCells.Remove(bossCell);

        // PowerUp e Vendor random
        int powerUpCell = validCells[Random.Range(0, validCells.Count)];
        validCells.Remove(powerUpCell);

        int vendorCell = validCells[Random.Range(0, validCells.Count)];
        validCells.Remove(vendorCell);

        // SPAWN STANZE
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {

                int index = i + j * size.x;
                Cell currentCell = board[index];

                if (!currentCell.visited) continue;

                GameObject roomPrefab;

                if(index == startCell) {
                    roomPrefab = startRoom;
                }
                else if (index == bossCell) {
                    roomPrefab = bossRoom;
                }
                else if (index == powerUpCell) {
                    roomPrefab = powerUpRoom;
                }
                else if (index == vendorCell) {
                    roomPrefab = vendorRoom;
                }
                else {
                    roomPrefab = GetRandomNormalRoom(i, j);
                }

                var newRoom = Instantiate(
                    roomPrefab,
                    new Vector2((i - offsetX) * offset.x, -(j - offsetY) * offset.y),
                    Quaternion.identity,
                    transform
                ).GetComponent<RoomBehaviour>();

                newRoom.SetGridPosition(new Vector2Int(i, j));
                newRoom.UpdateRoom(currentCell.status);

                newRoom.name += $" {i}-{j}";

                if (index == startCell) {
                    startRoomBehaviour = newRoom;
                }
            }
        }
    }

    // ===============================
    // STANZE NORMALI
    // ===============================
    GameObject GetRandomNormalRoom(int i, int j) {

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        List<GameObject> availableRooms = new List<GameObject>();

        for (int k = 0; k < normalRooms.Length; k++) {

            int x = i - offsetX;
            int y = j - offsetY;

            int p = normalRooms[k].ProbabilityOfSpawning(x, y);

            if (p == 1) {
                availableRooms.Add(normalRooms[k].room);
            }
        }

        if (availableRooms.Count > 0) {
            return availableRooms[Random.Range(0, availableRooms.Count)];
        }

        return normalRooms[0].room;
    }

    // TROVA STANZA PIÙ LONTANA (BOSS)
    int GetFarthestCell() {

        int bestIndex = 0;
        float maxDist = 0;

        int centerX = size.x / 2;
        int centerY = size.y / 2;

        for (int i = 0; i < board.Count; i++) {

            if (!board[i].visited) continue;

            int x = i % size.x;
            int y = i / size.x;

            float dist = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);

            if (dist > maxDist) {
                maxDist = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    // MAZE GENERATION
    void MazeGenerator() {

        board = new List<Cell>();

        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                board.Add(new Cell());
            }
        }

        int currentCell = (size.x / 2) + (size.y / 2) * size.x;
        startCell = currentCell;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while (k < 1000) {
            k++;

            board[currentCell].visited = true;

            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0) {
                if (path.Count == 0) break;
                currentCell = path.Pop();
            }
            else {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell) {

                    if (newCell - 1 == currentCell) {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else {
                    if (newCell + 1 == currentCell) {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }
            }
        }

        GenerateDungeon();
    }

    // CHECK NEIGHBORS
    List<int> CheckNeighbors(int cell) {

        List<int> neighbors = new List<int>();

        if (cell - size.x >= 0 && !board[cell - size.x].visited)
            neighbors.Add(cell - size.x);

        if (cell + size.x < board.Count && !board[cell + size.x].visited)
            neighbors.Add(cell + size.x);

        if ((cell + 1) % size.x != 0 && !board[cell + 1].visited)
            neighbors.Add(cell + 1);

        if (cell % size.x != 0 && !board[cell - 1].visited)
            neighbors.Add(cell - 1);

        return neighbors;
    }

    private void MovePlayerToStartRoom() {
        if (Player.Instance == null) {
            Debug.LogWarning("Player non trovato");
            return;
        }

        if (startRoomBehaviour == null) {
            Debug.LogWarning("Start room non trovata");
            return;
        }

        Transform spawnPoint = startRoomBehaviour.roomCentre;

        if (spawnPoint == null) {
            spawnPoint = startRoomBehaviour.transform;
        }

        Collider2D playerCollider = Player.Instance.GetComponent<Collider2D>();

        if (playerCollider != null) {
            playerCollider.enabled = false;
        }

        Player.Instance.transform.position = spawnPoint.position;

        if (playerCollider != null) {
            playerCollider.enabled = true;
        }
    }

    public bool[] GetCellStatus(Vector2Int gridPosition) {
        if (board == null) return null;

        int index = gridPosition.x + gridPosition.y * size.x;

        if (index < 0 || index >= board.Count) return null;

        return board[index].status;
    }
}

using System.Collections.Generic;
using UnityEngine;

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

        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y) {
            // 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

            if (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y) {
                return obligatory ? 2 : 1;
            }

            return 0;
        }

    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public Vector2 offset;

    List<Cell> board;

    // Start is called before the first frame update
    void Start() {
        MazeGenerator();
    }

    void GenerateDungeon() {

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                Cell currentCell = board[(i + j * size.x)];
                if (currentCell.visited) {
                    int randomRoom = -1;
                    List<int> availableRooms = new List<int>();

                    for (int k = 0; k < rooms.Length; k++) {
                        int x = i - offsetX;
                        int y = j - offsetY;

                        int p = rooms[k].ProbabilityOfSpawning(x, y);

                        if (p == 2) {
                            randomRoom = k;
                            break;
                        }
                        else if (p == 1) {
                            availableRooms.Add(k);
                        }
                    }

                    if (randomRoom == -1) {
                        if (availableRooms.Count > 0) {
                            randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                        }
                        else {
                            randomRoom = 0;
                        }
                    }

                    var newRoom = Instantiate(rooms[randomRoom].room,
                        new Vector2((i - offsetX) * offset.x, -(j - offsetY) * offset.y),
                        Quaternion.identity, 
                        transform).GetComponent<RoomBehaviour>();
                    newRoom.UpdateRoom(currentCell.status);
                    newRoom.name += " " + i + "-" + j;

                }
            }
        }

    }

    void MazeGenerator() {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {
                board.Add(new Cell());
            }
        }

        // invece di currentCell = startPos, cosi parte dal centro effettivamente
        int currentCell = (size.x / 2) + (size.y / 2) * size.x;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while (k < 1000) {
            k++;

            board[currentCell].visited = true;

            if (currentCell == board.Count - 1) {
                break;
            }

            //Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0) {
                if (path.Count == 0) {
                    break;
                }
                else {
                    currentCell = path.Pop();
                }
            }
            else {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell) {
                    //down or right
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
                    //up or left
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

    List<int> CheckNeighbors(int cell) {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell - size.x)].visited) {
            neighbors.Add((cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited) {
            neighbors.Add((cell + size.x));
        }

        //check right neighbor
        if ((cell + 1) % size.x != 0 && !board[(cell + 1)].visited) {
            neighbors.Add((cell + 1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited) {
            neighbors.Add((cell - 1));
        }

        return neighbors;
    }
}

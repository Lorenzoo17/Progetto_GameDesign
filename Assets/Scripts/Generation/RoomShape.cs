using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility condivise per la griglia del dungeon.
/// Convenzione direzioni (identica a quella gia' usata da RoomBehaviour/DungeonGenerator):
/// 0 = Up, 1 = Down, 2 = Right, 3 = Left.
/// Convenzione griglia: x cresce verso destra, y cresce verso il BASSO
/// (nel mondo la y viene negata dal DungeonGenerator).
/// </summary>
public static class RoomShape {

    public const int UP = 0;
    public const int DOWN = 1;
    public const int RIGHT = 2;
    public const int LEFT = 3;

    /// <summary>Offset di griglia per ognuna delle 4 direzioni.</summary>
    public static readonly Vector2Int[] Directions = {
        new Vector2Int(0, -1), // Up
        new Vector2Int(0,  1), // Down
        new Vector2Int(1,  0), // Right
        new Vector2Int(-1, 0)  // Left
    };

    public static int Opposite(int direction) {
        switch (direction) {
            case UP: return DOWN;
            case DOWN: return UP;
            case RIGHT: return LEFT;
            default: return RIGHT;
        }
    }

    /// <summary>Forma di default: una singola cella in (0,0).</summary>
    public static readonly Vector2Int[] Single = { Vector2Int.zero };

    /// <summary>
    /// Converte un offset di cella (in celle di griglia) nella posizione LOCALE
    /// che quella cella deve avere dentro il prefab, dato l'offset del dungeon.
    /// </summary>
    public static Vector3 OffsetToLocalPosition(Vector2Int cellOffset, Vector2 dungeonOffset) {
        return new Vector3(cellOffset.x * dungeonOffset.x, -cellOffset.y * dungeonOffset.y, 0f);
    }

    /// <summary>
    /// True se l'insieme di celle passato e' connesso considerando solo l'adiacenza
    /// ortogonale (indipendentemente dalle porte del maze).
    /// </summary>
    public static bool IsContiguous(IList<Vector2Int> cells) {
        if (cells == null || cells.Count == 0) return false;
        if (cells.Count == 1) return true;

        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(cells);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Vector2Int start = cells[0];
        queue.Enqueue(start);
        remaining.Remove(start);

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();

            for (int d = 0; d < 4; d++) {
                Vector2Int neighbour = current + Directions[d];

                if (remaining.Remove(neighbour)) {
                    queue.Enqueue(neighbour);
                }
            }
        }

        return remaining.Count == 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMinimapUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] private RectTransform container;
    [SerializeField] private Image roomIconPrefab;
    [SerializeField] private Image corridorIconPrefab;

    [Header("Layout")]
    [SerializeField] private float spacing = 28f;
    [SerializeField] private float roomSize = 16f;
    [SerializeField] private float corridorThickness = 4f;

    [Header("Visibility")]
    [SerializeField] private bool showOnlyVisitedRooms = true;
    [SerializeField] private bool showCorridorsToUnknownRooms = false;

    [Header("Colors")]
    [SerializeField] private Color normalRoomColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color currentRoomColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color startRoomColor = new Color(0.3f, 0.8f, 1f, 1f);
    [SerializeField] private Color bossRoomColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color treasureRoomColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color vendorRoomColor = new Color(0.4f, 1f, 0.4f, 1f);
    [SerializeField] private Color trapRoomColor = new Color(0.8f, 0.3f, 1f, 1f);
    [SerializeField] private Color corridorColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private DungeonGenerator dungeonGenerator;

    // una stanza puo' occupare piu' celle -> piu' di un'immagine
    // (un'icona per ogni rettangolo massimale + eventuali raccordi tra i rettangoli)
    private readonly Dictionary<RoomBehaviour, List<Image>> roomIcons = new Dictionary<RoomBehaviour, List<Image>>();
    private readonly Dictionary<Vector2Int, RoomBehaviour> roomByGridPosition = new Dictionary<Vector2Int, RoomBehaviour>();
    private readonly List<CorridorData> corridors = new List<CorridorData>();

    private RoomBehaviour currentRoom;

    private class CorridorData {
        public Image image;
        public Vector2Int from;
        public Vector2Int to;

        public CorridorData(Image image, Vector2Int from, Vector2Int to) {
            this.image = image;
            this.from = from;
            this.to = to;
        }
    }

    private void OnEnable() {
        RoomBehaviour.OnAnyRoomEntered += HandleRoomEntered;
        RoomBehaviour.OnAnyRoomVisited += HandleRoomVisited;
    }

    private void OnDisable() {
        RoomBehaviour.OnAnyRoomEntered -= HandleRoomEntered;
        RoomBehaviour.OnAnyRoomVisited -= HandleRoomVisited;
    }

    private IEnumerator Start() {
        yield return new WaitUntil(() =>
            DungeonGenerator.Instance != null &&
            DungeonGenerator.Instance.IsDungeonReady
        );

        dungeonGenerator = DungeonGenerator.Instance;

        BuildMinimap();
    }

    private void BuildMinimap() {
        if (container == null) {
            container = GetComponent<RectTransform>();
        }

        ClearMinimap();

        RoomBehaviour[] rooms = dungeonGenerator.GetComponentsInChildren<RoomBehaviour>(true);

        // Icone delle stanze.
        // Una stanza multi-cella non viene disegnata cella per cella (verrebbe una fila di
        // quadrati staccati): la si scompone in rettangoli massimali e si disegna
        // un'icona sola per rettangolo. Cosi' una 2x1 diventa un unico rettangolo allungato.
        foreach (RoomBehaviour room in rooms) {

            foreach (Vector2Int gridPos in room.OccupiedGridPositions) {
                roomByGridPosition[gridPos] = room;
            }

            roomIcons[room] = CreateRoomIcons(room);

            if (room.IsVisited && currentRoom == null) {
                currentRoom = room;
            }
        }

        // Poi creo i corridoi tra celle di stanze DIVERSE
        foreach (RoomBehaviour room in rooms) {

            foreach (Vector2Int gridPos in room.OccupiedGridPositions) {

                bool[] status = dungeonGenerator.GetCellStatus(gridPos);

                if (status == null) continue;

                // 0 = Up, 1 = Down, 2 = Right, 3 = Left
                // Creo solo Right e Down per evitare duplicati.

                if (status[2]) {
                    // Right = x + 1
                    CreateCorridor(gridPos, new Vector2Int(1, 0));
                }

                if (status[1]) {
                    // Down nella tua griglia = y + 1, NON Vector2Int.down
                    CreateCorridor(gridPos, new Vector2Int(0, 1));
                }
            }
        }

        RefreshMinimap();
    }

    private void ClearMinimap() {
        roomIcons.Clear();
        roomByGridPosition.Clear();
        corridors.Clear();

        for (int i = container.childCount - 1; i >= 0; i--) {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Crea le immagini che compongono una stanza: un'icona per ogni rettangolo massimale,
    /// piu' un raccordo pieno dove due rettangoli della stessa stanza si toccano.
    /// </summary>
    private List<Image> CreateRoomIcons(RoomBehaviour room) {

        List<Image> images = new List<Image>();

        List<RectInt> rectangles = DecomposeIntoRectangles(room.OccupiedGridPositions);

        // a quale rettangolo appartiene ogni cella
        Dictionary<Vector2Int, int> rectangleByCell = new Dictionary<Vector2Int, int>();

        for (int r = 0; r < rectangles.Count; r++) {
            for (int y = 0; y < rectangles[r].height; y++) {
                for (int x = 0; x < rectangles[r].width; x++) {
                    rectangleByCell[new Vector2Int(rectangles[r].x + x, rectangles[r].y + y)] = r;
                }
            }
        }

        // raccordi fra rettangoli diversi: creati per primi cosi' restano sotto le icone
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(room.OccupiedGridPositions);

        foreach (Vector2Int cell in room.OccupiedGridPositions) {

            // solo Right e Down, per non creare doppioni
            for (int k = 0; k < 2; k++) {

                Vector2Int direction = k == 0 ? new Vector2Int(1, 0) : new Vector2Int(0, 1);
                Vector2Int neighbour = cell + direction;

                if (!occupied.Contains(neighbour)) continue;
                if (rectangleByCell[cell] == rectangleByCell[neighbour]) continue; // stesso rettangolo: gia' pieno

                Image joint = Instantiate(corridorIconPrefab, container);

                joint.rectTransform.anchoredPosition =
                    (GridToUIPosition(cell) + GridToUIPosition(neighbour)) * 0.5f;

                // +1 per evitare la riga di pixel scoperta sul bordo
                joint.rectTransform.sizeDelta = direction.x != 0
                    ? new Vector2(spacing - roomSize + 1f, roomSize)
                    : new Vector2(roomSize, spacing - roomSize + 1f);

                images.Add(joint);
            }
        }

        // un'icona per rettangolo
        foreach (RectInt rectangle in rectangles) {

            Vector2Int min = new Vector2Int(rectangle.x, rectangle.y);
            Vector2Int max = new Vector2Int(rectangle.x + rectangle.width - 1, rectangle.y + rectangle.height - 1);

            Image icon = Instantiate(roomIconPrefab, container);

            icon.rectTransform.anchoredPosition = (GridToUIPosition(min) + GridToUIPosition(max)) * 0.5f;

            icon.rectTransform.sizeDelta = new Vector2(
                (rectangle.width - 1) * spacing + roomSize,
                (rectangle.height - 1) * spacing + roomSize
            );

            images.Add(icon);
        }

        return images;
    }

    /// <summary>
    /// Scompone un insieme di celle nel minor numero ragionevole di rettangoli pieni
    /// (greedy: allarga a destra il piu' possibile, poi verso il basso finche' la
    /// larghezza si mantiene).
    /// </summary>
    private static List<RectInt> DecomposeIntoRectangles(IReadOnlyList<Vector2Int> cells) {

        List<RectInt> rectangles = new List<RectInt>();

        if (cells == null || cells.Count == 0) return rectangles;

        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(cells);

        List<Vector2Int> ordered = new List<Vector2Int>(cells);
        ordered.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        foreach (Vector2Int cell in ordered) {

            if (!remaining.Contains(cell)) continue;

            int width = 1;
            while (remaining.Contains(new Vector2Int(cell.x + width, cell.y))) width++;

            int height = 1;
            while (true) {

                bool rowComplete = true;

                for (int x = 0; x < width; x++) {
                    if (!remaining.Contains(new Vector2Int(cell.x + x, cell.y + height))) {
                        rowComplete = false;
                        break;
                    }
                }

                if (!rowComplete) break;

                height++;
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    remaining.Remove(new Vector2Int(cell.x + x, cell.y + y));
                }
            }

            rectangles.Add(new RectInt(cell.x, cell.y, width, height));
        }

        return rectangles;
    }

    private void CreateCorridor(Vector2Int from, Vector2Int direction) {
        Vector2Int to = from + direction;

        if (!AreCellsConnected(from, to)) return;

        if (!roomByGridPosition.ContainsKey(to)) return;

        // celle della stessa stanza: gia' coperte dall'icona della stanza
        if (roomByGridPosition.TryGetValue(from, out RoomBehaviour fromRoom) &&
            roomByGridPosition.TryGetValue(to, out RoomBehaviour toRoom) &&
            fromRoom == toRoom) {
            return;
        }

        Image corridor = Instantiate(corridorIconPrefab, container);

        Vector2 fromPos = GridToUIPosition(from);
        Vector2 toPos = GridToUIPosition(to);

        corridor.rectTransform.anchoredPosition = (fromPos + toPos) * 0.5f;

        bool isHorizontal = direction.x != 0;

        if (isHorizontal) {
            corridor.rectTransform.sizeDelta = new Vector2(spacing - roomSize, corridorThickness);
        }
        else {
            corridor.rectTransform.sizeDelta = new Vector2(corridorThickness, spacing - roomSize);
        }

        corridor.color = corridorColor;
        corridor.transform.SetAsFirstSibling();

        corridors.Add(new CorridorData(corridor, from, to));
    }

    private bool AreCellsConnected(Vector2Int from, Vector2Int to) {
        bool[] fromStatus = dungeonGenerator.GetCellStatus(from);
        bool[] toStatus = dungeonGenerator.GetCellStatus(to);

        if (fromStatus == null || toStatus == null) return false;

        Vector2Int dir = to - from;

        // from -> to a destra
        if (dir == new Vector2Int(1, 0))
            return fromStatus[2] && toStatus[3];

        // from -> to in basso
        if (dir == new Vector2Int(0, 1))
            return fromStatus[1] && toStatus[0];

        // from -> to a sinistra
        if (dir == new Vector2Int(-1, 0))
            return fromStatus[3] && toStatus[2];

        // from -> to in alto
        if (dir == new Vector2Int(0, -1))
            return fromStatus[0] && toStatus[1];

        return false;
    }

    private Vector2 GridToUIPosition(Vector2Int gridPosition) {
        float centerX = (dungeonGenerator.size.x - 1) * 0.5f;
        float centerY = (dungeonGenerator.size.y - 1) * 0.5f;

        float x = (gridPosition.x - centerX) * spacing;
        float y = -(gridPosition.y - centerY) * spacing;

        return new Vector2(x, y);
    }

    private void HandleRoomEntered(RoomBehaviour room) {
        currentRoom = room;
        RefreshMinimap();
    }

    private void HandleRoomVisited(RoomBehaviour room) {
        RefreshMinimap();
    }

    private void RefreshMinimap() {
        foreach (var pair in roomIcons) {
            RoomBehaviour room = pair.Key;
            List<Image> icons = pair.Value;

            bool visible = !showOnlyVisitedRooms || room.IsVisited || room == currentRoom;
            Color color = GetRoomColor(room);

            for (int i = 0; i < icons.Count; i++) {
                if (icons[i] == null) continue;

                icons[i].gameObject.SetActive(visible);

                if (!visible) continue;

                icons[i].color = color;
            }
        }

        foreach (CorridorData corridor in corridors) {
            bool fromVisible = IsRoomVisible(corridor.from);
            bool toVisible = IsRoomVisible(corridor.to);

            bool visible;

            if (!showOnlyVisitedRooms) {
                visible = true;
            }
            else if (showCorridorsToUnknownRooms) {
                visible = fromVisible || toVisible;
            }
            else {
                visible = fromVisible && toVisible;
            }

            corridor.image.gameObject.SetActive(visible);
        }
    }

    private bool IsRoomVisible(Vector2Int gridPosition) {
        if (!roomByGridPosition.TryGetValue(gridPosition, out RoomBehaviour room)) {
            return false;
        }

        return !showOnlyVisitedRooms || room.IsVisited || room == currentRoom;
    }

    private Color GetRoomColor(RoomBehaviour room) {
        if (room == currentRoom) {
            return currentRoomColor;
        }

        switch (room.RoomType) {
            case RoomType.StartRoom:
                return startRoomColor;

            case RoomType.BossRoom:
                return bossRoomColor;

            case RoomType.TreasureRoom:
                return treasureRoomColor;

            case RoomType.VendorRoom:
                return vendorRoomColor;

            case RoomType.TrapRoom:
                return trapRoomColor;

            default:
                return normalRoomColor;
        }
    }
}

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

    private readonly Dictionary<RoomBehaviour, Image> roomIcons = new Dictionary<RoomBehaviour, Image>();
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

        // Prima creo le icone delle stanze
        foreach (RoomBehaviour room in rooms) {
            Vector2Int gridPos = room.GridPosition;

            roomByGridPosition[gridPos] = room;

            Image icon = Instantiate(roomIconPrefab, container);
            icon.rectTransform.anchoredPosition = GridToUIPosition(gridPos);
            icon.rectTransform.sizeDelta = new Vector2(roomSize, roomSize);

            roomIcons[room] = icon;

            if (room.IsVisited && currentRoom == null) {
                currentRoom = room;
            }
        }

        // Poi creo i corridoi tra le stanze
        foreach (RoomBehaviour room in rooms) {
            bool[] status = dungeonGenerator.GetCellStatus(room.GridPosition);

            if (status == null) continue;

            // 0 = Up, 1 = Down, 2 = Right, 3 = Left
            // Creo solo Right e Down per evitare duplicati.

            if (status[2]) {
                // Right = x + 1
                CreateCorridor(room.GridPosition, new Vector2Int(1, 0));
            }

            if (status[1]) {
                // Down nella tua griglia = y + 1, NON Vector2Int.down
                CreateCorridor(room.GridPosition, new Vector2Int(0, 1));
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

    private void CreateCorridor(Vector2Int from, Vector2Int direction) {
        Vector2Int to = from + direction;

        if (!AreCellsConnected(from, to)) return;

        if (!roomByGridPosition.ContainsKey(to)) return;

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
            Image icon = pair.Value;

            bool visible = !showOnlyVisitedRooms || room.IsVisited || room == currentRoom;

            icon.gameObject.SetActive(visible);

            if (!visible) continue;

            icon.color = GetRoomColor(room);
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
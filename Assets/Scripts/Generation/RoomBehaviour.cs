using NavMeshPlus.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    FirstStartRoom, // start room del primo piano (con anche arma)
    StartRoom,
    EnemiesRoom,
    TrapRoom,
    TreasureRoom,
    VendorRoom,
    BossRoom
}

/// <summary>
/// Descrive una singola cella di griglia occupata dalla stanza.
/// Per una stanza 1x1 non serve compilare nulla: viene usato il vecchio setup blocks/doors.
///
/// offset: posizione della cella rispetto alla cella "ancora" della stanza.
///         (0,0) e' obbligatoria e corrisponde all'origine del prefab.
///         x cresce verso destra, y cresce verso il BASSO.
///         Esempio 2x1 orizzontale: (0,0) e (1,0)
///         Esempio a L:             (0,0), (1,0) e (0,1)
///
/// blocks / doors: array di 4 elementi, indici 0=Up 1=Down 2=Right 3=Left,
///                 riferiti ai lati ESTERNI di QUELLA cella.
///                 I lati che confinano con un'altra cella della stessa stanza
///                 vengono ignorati automaticamente (possono essere lasciati vuoti).
/// </summary>
[System.Serializable]
public class RoomCellSetup {

    [Tooltip("Solo per leggibilita' nell'inspector")]
    public string label = "Cell";

    [Tooltip("Offset in celle rispetto all'ancora. x = destra, y = basso")]
    public Vector2Int offset;

    [Tooltip("0 = Up, 1 = Down, 2 = Right, 3 = Left")]
    public GameObject[] blocks = new GameObject[4];

    [Tooltip("0 = Up, 1 = Down, 2 = Right, 3 = Left")]
    public GameObject[] doors = new GameObject[4];

    [Tooltip("Opzionale. Se assegnato, quando il player si trova in questa cella la camera " +
             "viene clampata su questi bounds invece che su quelli dell'intera stanza. " +
             "Utile per le stanze a L (evita di inquadrare l'angolo vuoto). " +
             "Lasciare vuoto per far scorrere la camera su tutta la stanza (consigliato per le 2x1).")]
    public BoxCollider2D cameraBounds;
}

public class RoomBehaviour : MonoBehaviour {

    // ===============================
    // CONVENZIONI DI NOMI PER L'AUTO-CONFIGURAZIONE
    // ===============================
    // Se il root della stanza ha figli chiamati Cell_<x>_<y>, la forma e i riferimenti
    // a muri/porte vengono ricavati AUTOMATICAMENTE dalla gerarchia: non serve
    // compilare nulla nell'inspector.

    public const string CellNamePrefix = "Cell_";
    public const string CameraBoundsName = "CameraBounds";

    // 0 = Up, 1 = Down, 2 = Right, 3 = Left
    public static readonly string[] BlockNames = { "ClosedUp", "ClosedDown", "ClosedRight", "ClosedLeft" };

    public static readonly string[][] DoorNames = {
        new[] { "DoorUp", "DoorTop" },
        new[] { "DoorBottom", "DoorDown" },
        new[] { "DoorRight" },
        new[] { "DoorLeft" }
    };

    [SerializeField] private RoomType roomType;

    [Header("Setup 1x1 (legacy) - usato se non ci sono celle multiple")]
    // 0 - Up, 1 - Down, 2 - Right, 3 - Left
    [SerializeField] private GameObject[] blocks; // muri
    [SerializeField] private GameObject[] doors;

    [Header("Setup multi-cella (2x1, a L, ...)")]
    [Tooltip("Opzionale: se il root ha figli chiamati Cell_<x>_<y> questa lista viene " +
             "ricavata da sola dalla gerarchia. Compilala a mano solo se vuoi forzare " +
             "una configurazione diversa.")]
    [SerializeField] private RoomCellSetup[] cells;

    // configurazione effettivamente usata (serializzata oppure dedotta dalla gerarchia)
    private RoomCellSetup[] resolvedCells;
    private bool cellsResolved;

    // stato porte per cella: chiave = offset di cella, valore = 4 bool
    private readonly Dictionary<Vector2Int, bool[]> doorExistsByCell = new Dictionary<Vector2Int, bool[]>();

    private bool isVisited = false;
    private bool isCleared = false;

    // quante volte il collider del player e' dentro un trigger di questa stanza
    // (le stanze multi-cella possono avere piu' di un trigger)
    private int playerTriggerCount = 0;

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

    public static event Action<RoomBehaviour> OnAnyRoomEntered;
    public static event Action<RoomBehaviour> OnAnyRoomVisited;
    public static event Action<RoomBehaviour> OnAnyRoomCleared;

    public RoomType RoomType => roomType;

    /// <summary>Cella "ancora" della stanza (in coordinate di griglia del dungeon).</summary>
    public Vector2Int GridPosition { get; private set; }

    public bool IsVisited => isVisited;

    /// <summary>True se la stanza occupa piu' di una cella di griglia.</summary>
    public bool IsMultiCell => GetShapeOffsets().Length > 1;

    private readonly List<Vector2Int> occupiedGridPositions = new List<Vector2Int>();

    /// <summary>Tutte le celle di griglia realmente occupate dalla stanza.</summary>
    public IReadOnlyList<Vector2Int> OccupiedGridPositions => occupiedGridPositions;

    private Vector2Int[] cachedShape;

    // ===============================
    // CONFIGURAZIONE CELLE
    // ===============================

    /// <summary>
    /// Configurazione delle celle: quella serializzata se presente, altrimenti quella
    /// dedotta dai figli chiamati Cell_x_y. Null = stanza 1x1 legacy.
    /// Funziona anche su un PREFAB non istanziato.
    /// </summary>
    public RoomCellSetup[] GetCells() {

        // in editor ricalcolo sempre, cosi' le modifiche si vedono subito
        if (!Application.isPlaying) cellsResolved = false;

        if (cellsResolved) return resolvedCells;

        cellsResolved = true;

        // 1) La gerarchia ha la precedenza: e' la fonte piu' affidabile, non dipende
        //    da come e' andata la serializzazione del prefab.
        RoomCellSetup[] fromHierarchy = BuildCellsFromHierarchy(transform);

        if (fromHierarchy != null && fromHierarchy.Length > 1) {
            resolvedCells = fromHierarchy;
            return resolvedCells;
        }

        // 2) Altrimenti la lista compilata a mano nell'inspector.
        if (cells != null && cells.Length > 0) {
            resolvedCells = cells;
            return resolvedCells;
        }

        resolvedCells = fromHierarchy;
        return resolvedCells;
    }

    /// <summary>
    /// Descrizione di cosa vede il sistema: serve a capire perche' una stanza
    /// multi-cella viene letta come 1x1.
    /// </summary>
    public string GetShapeDiagnostics() {

        string serialized = cells == null ? "null" : cells.Length.ToString();

        RoomCellSetup[] fromHierarchy = BuildCellsFromHierarchy(transform);
        string hierarchy = fromHierarchy == null ? "0" : fromHierarchy.Length.ToString();

        string childNames = "";
        int shown = 0;

        foreach (Transform child in transform) {
            if (shown > 0) childNames += ", ";
            childNames += child.name;
            shown++;
            if (shown >= 8) { childNames += ", ..."; break; }
        }

        if (shown == 0) childNames = "(nessun figlio)";

        string offsets = "";

        if (cells != null) {
            for (int i = 0; i < cells.Length; i++) {
                offsets += (i > 0 ? " " : "") + (cells[i] == null ? "null" : cells[i].offset.ToString());
            }
        }

        return $"cells serializzate = {serialized} [{offsets}] | celle dalla gerarchia = {hierarchy} | figli del root: {childNames}";
    }

    /// <summary>
    /// Ricava la configurazione delle celle dalla gerarchia cercando i figli diretti
    /// chiamati Cell_&lt;x&gt;_&lt;y&gt; e, dentro ognuno, gli oggetti ClosedUp/ClosedDown/...,
    /// DoorUp/DoorBottom/... e CameraBounds.
    /// Ritorna null se non ci sono celle (stanza 1x1 normale).
    /// </summary>
    public static RoomCellSetup[] BuildCellsFromHierarchy(Transform root) {

        List<RoomCellSetup> result = new List<RoomCellSetup>();
        bool hasAnchor = false;

        foreach (Transform child in root) {

            if (!TryParseCellName(child.name, out Vector2Int offset)) continue;

            if (offset == Vector2Int.zero) hasAnchor = true;

            RoomCellSetup setup = new RoomCellSetup {
                label = child.name,
                offset = offset,
                blocks = new GameObject[4],
                doors = new GameObject[4]
            };

            for (int d = 0; d < 4; d++) {
                setup.blocks[d] = FindByName(child, BlockNames[d]);
                setup.doors[d] = FindByName(child, DoorNames[d]);
            }

            result.Add(setup);
        }

        if (result.Count == 0) return null;

        if (!hasAnchor) {
            Debug.LogWarning($"[{root.name}] Trovati figli Cell_x_y ma manca 'Cell_0_0' (cella ancora): la stanza resta 1x1.");
            return null;
        }

        // camera per cella solo se la forma NON e' un rettangolo pieno:
        // per una 2x1 e' meglio far scorrere la camera su tutta la stanza,
        // per una L serve limitarla alla cella (altrimenti si inquadra l'angolo vuoto).
        if (!IsFullRectangle(result)) {
            foreach (Transform child in root) {

                if (!TryParseCellName(child.name, out Vector2Int offset)) continue;

                GameObject camera = FindByName(child, CameraBoundsName);
                if (camera == null) continue;

                RoomCellSetup setup = result.Find(c => c.offset == offset);
                if (setup != null) setup.cameraBounds = camera.GetComponent<BoxCollider2D>();
            }
        }

        return result.ToArray();
    }

    /// <summary>Riconosce i nomi del tipo "Cell_1_0" oppure "Cell_-1_2".</summary>
    public static bool TryParseCellName(string name, out Vector2Int offset) {

        offset = Vector2Int.zero;

        if (string.IsNullOrEmpty(name) || !name.StartsWith(CellNamePrefix)) return false;

        string[] parts = name.Substring(CellNamePrefix.Length).Split('_');

        if (parts.Length != 2) return false;

        if (!int.TryParse(parts[0], out int x)) return false;
        if (!int.TryParse(parts[1], out int y)) return false;

        offset = new Vector2Int(x, y);
        return true;
    }

    private static bool IsFullRectangle(List<RoomCellSetup> cells) {

        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

        for (int i = 0; i < cells.Count; i++) {
            minX = Mathf.Min(minX, cells[i].offset.x);
            maxX = Mathf.Max(maxX, cells[i].offset.x);
            minY = Mathf.Min(minY, cells[i].offset.y);
            maxY = Mathf.Max(maxY, cells[i].offset.y);
        }

        return (maxX - minX + 1) * (maxY - minY + 1) == cells.Count;
    }

    private static GameObject FindByName(Transform root, string name) {
        Transform found = FindTransform(root, name);
        return found != null ? found.gameObject : null;
    }

    private static GameObject FindByName(Transform root, string[] names) {
        for (int i = 0; i < names.Length; i++) {
            GameObject found = FindByName(root, names[i]);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindTransform(Transform root, string name) {

        if (root.name == name) return root;

        foreach (Transform child in root) {
            Transform found = FindTransform(child, name);
            if (found != null) return found;
        }

        return null;
    }

    // ===============================
    // FORMA DELLA STANZA
    // ===============================

    /// <summary>
    /// Offset delle celle occupate. Funziona anche chiamato su un PREFAB non istanziato.
    /// </summary>
    public Vector2Int[] GetShapeOffsets() {

        // in editor ricalcolo sempre, cosi' le modifiche si vedono subito
        if (!Application.isPlaying) cachedShape = null;

        if (cachedShape != null) return cachedShape;

        RoomCellSetup[] activeCells = GetCells();

        if (activeCells == null || activeCells.Length == 0) {
            cachedShape = RoomShape.Single;
            return cachedShape;
        }

        List<Vector2Int> result = new List<Vector2Int>();
        bool hasAnchor = false;

        for (int i = 0; i < activeCells.Length; i++) {
            if (activeCells[i] == null) continue;

            Vector2Int offset = activeCells[i].offset;

            if (offset == Vector2Int.zero) hasAnchor = true;
            if (result.Contains(offset)) continue;

            result.Add(offset);
        }

        if (result.Count == 0) {
            cachedShape = RoomShape.Single;
            return cachedShape;
        }

        if (!hasAnchor) {
            Debug.LogWarning($"[{name}] Nessuna cella con offset (0,0): la forma della stanza non e' valida. Uso 1x1.");
            cachedShape = RoomShape.Single;
            return cachedShape;
        }

        if (!RoomShape.IsContiguous(result)) {
            Debug.LogWarning($"[{name}] Le celle della stanza non sono contigue. Uso 1x1.");
            cachedShape = RoomShape.Single;
            return cachedShape;
        }

        cachedShape = result.ToArray();
        return cachedShape;
    }

    /// <summary>Setup usato dal DungeonGenerator per una stanza 1x1.</summary>
    public void SetGridPosition(Vector2Int gridPosition) {
        SetPlacement(gridPosition, GetShapeOffsets());
    }

    /// <summary>Setup usato dal DungeonGenerator: ancora + celle occupate.</summary>
    public void SetPlacement(Vector2Int anchor, Vector2Int[] shape) {
        GridPosition = anchor;

        occupiedGridPositions.Clear();

        if (shape == null || shape.Length == 0) shape = RoomShape.Single;

        // la forma decisa dal generatore ha la precedenza su quella letta dal prefab:
        // cosi' UpdateRoom riconosce correttamente i lati interni anche se la forma
        // arriva da uno 'Shape Override' impostato nell'inspector.
        if (shape.Length > 1) {
            cachedShape = shape;
        }

        for (int i = 0; i < shape.Length; i++) {
            occupiedGridPositions.Add(anchor + shape[i]);
        }
    }

    private void Awake() {
        // NB: niente fallback su GetComponent<BoxCollider2D>() qui.
        // Su una stanza multi-cella prenderebbe il trigger della PRIMA cella e la camera
        // resterebbe incollata a un solo riquadro. I bounds veri li risolve
        // TryGetRoomWorldBounds, che tiene conto di tutte le celle.

        if (occupiedGridPositions.Count == 0) {
            SetPlacement(GridPosition, GetShapeOffsets());
        }
    }

    private void Start() {
        if (roomCentre == null) {
            roomCentre = transform;
        }

        // BakeRoomNavMesh(); // fatto in dungeon generator
    }

    public void MarkAsVisited() {
        if (isVisited) return;

        isVisited = true;
        OnAnyRoomVisited?.Invoke(this);
    }

    // ===============================
    // PORTE / MURI
    // ===============================

    /// <summary>
    /// Overload legacy: stanza 1x1. Chiamato dal DungeonGenerator.
    /// </summary>
    public void UpdateRoom(bool[] status) {
        Dictionary<Vector2Int, bool[]> statusByCell = new Dictionary<Vector2Int, bool[]> {
            { Vector2Int.zero, status }
        };

        UpdateRoom(statusByCell);
    }

    /// <summary>
    /// Applica lo stato delle porte cella per cella.
    /// La chiave del dizionario e' l'OFFSET della cella rispetto all'ancora.
    /// I lati interni alla stanza non ricevono ne' muro ne' porta.
    /// </summary>
    public void UpdateRoom(Dictionary<Vector2Int, bool[]> statusByCell) {

        Vector2Int[] shape = GetShapeOffsets();
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(shape);

        doorExistsByCell.Clear();

        RoomCellSetup[] activeCells = GetCells();

        if (activeCells == null || activeCells.Length == 0) {
            bool[] status = GetStatusFor(statusByCell, Vector2Int.zero);
            ApplyCell(Vector2Int.zero, blocks, doors, status, occupied);
            return;
        }

        for (int i = 0; i < activeCells.Length; i++) {
            RoomCellSetup cell = activeCells[i];
            if (cell == null) continue;

            bool[] status = GetStatusFor(statusByCell, cell.offset);

            ApplyCell(cell.offset, cell.blocks, cell.doors, status, occupied);
        }
    }

    private static bool[] GetStatusFor(Dictionary<Vector2Int, bool[]> statusByCell, Vector2Int offset) {
        if (statusByCell != null && statusByCell.TryGetValue(offset, out bool[] status) && status != null && status.Length >= 4) {
            return status;
        }

        return new bool[4];
    }

    private void ApplyCell(Vector2Int cellOffset, GameObject[] cellBlocks, GameObject[] cellDoors, bool[] status, HashSet<Vector2Int> occupied) {

        bool[] doorExists = new bool[4];

        for (int d = 0; d < 4; d++) {

            // lato che confina con un'altra cella della stessa stanza:
            // niente muro, niente porta, passaggio libero disegnato nella tilemap
            bool isInternalEdge = occupied.Contains(cellOffset + RoomShape.Directions[d]);

            bool hasDoor = !isInternalEdge && status[d];
            doorExists[d] = hasDoor;

            GameObject block = GetAt(cellBlocks, d);
            GameObject door = GetAt(cellDoors, d);

            // il muro c'e' solo se il lato e' esterno e non ha porta
            if (block != null) {
                block.SetActive(!hasDoor && !isInternalEdge);
            }

            if (door != null) {
                door.SetActive(hasDoor);

                // tutte le porte sono settate come aperte all'inizio
                if (hasDoor && door.TryGetComponent(out Door doorComponent)) {
                    doorComponent.SetClosed(false);
                }
            }
        }

        doorExistsByCell[cellOffset] = doorExists;
    }

    private static GameObject GetAt(GameObject[] array, int index) {
        if (array == null || index < 0 || index >= array.Length) return null;
        return array[index];
    }

    // chiudi SOLO porte esistenti
    public void CloseDoors() {
        SetDoorsClosed(true);
    }

    // Si attivano solo porte effettivamente esistenti
    public void OpenDoors() {
        SetDoorsClosed(false);
    }

    private void SetDoorsClosed(bool closed) {

        RoomCellSetup[] activeCells = GetCells();

        if (activeCells == null || activeCells.Length == 0) {
            ApplyDoorState(Vector2Int.zero, doors, closed);
            return;
        }

        for (int i = 0; i < activeCells.Length; i++) {
            if (activeCells[i] == null) continue;
            ApplyDoorState(activeCells[i].offset, activeCells[i].doors, closed);
        }
    }

    private void ApplyDoorState(Vector2Int cellOffset, GameObject[] cellDoors, bool closed) {
        if (!doorExistsByCell.TryGetValue(cellOffset, out bool[] doorExists)) return;

        for (int d = 0; d < 4; d++) {
            if (!doorExists[d]) continue;

            GameObject door = GetAt(cellDoors, d);
            if (door == null) continue;

            if (door.TryGetComponent(out Door doorComponent)) {
                doorComponent.SetClosed(closed);
            }
        }
    }

    // ===============================
    // CAMERA
    // ===============================

    private List<Bounds> cameraRegion;

    private Bounds roomWorldBounds;
    private bool roomWorldBoundsResolved;

    private static void GetShapeBounds(Vector2Int[] shape, out Vector2Int min, out Vector2Int size) {

        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

        for (int i = 0; i < shape.Length; i++) {
            minX = Mathf.Min(minX, shape[i].x);
            maxX = Mathf.Max(maxX, shape[i].x);
            minY = Mathf.Min(minY, shape[i].y);
            maxY = Mathf.Max(maxY, shape[i].y);
        }

        min = new Vector2Int(minX, minY);
        size = new Vector2Int(maxX - minX + 1, maxY - minY + 1);
    }

    /// <summary>
    /// Area totale della stanza nel mondo. Non si fida del solo campo serializzato:
    /// prova in ordine il campo, un figlio chiamato "RoomBounds" e infine l'unione di
    /// tutti i collider del root (che per una stanza multi-cella sono i trigger di ogni cella).
    /// </summary>
    private bool TryGetRoomWorldBounds(out Bounds result) {

        if (roomWorldBoundsResolved) {
            result = roomWorldBounds;
            return true;
        }

        // 1. campo assegnato nell'inspector
        if (roomBounds != null) {
            roomWorldBounds = roomBounds.bounds;
            roomWorldBoundsResolved = true;
            result = roomWorldBounds;
            return true;
        }

        // 2. figlio chiamato "RoomBounds"
        Transform boundsChild = FindTransform(transform, "RoomBounds");

        if (boundsChild != null && boundsChild.TryGetComponent(out BoxCollider2D boundsCollider)) {
            roomBounds = boundsCollider;
            roomWorldBounds = boundsCollider.bounds;
            roomWorldBoundsResolved = true;
            result = roomWorldBounds;
            return true;
        }

        // 3. unione dei collider sul root (un trigger per cella + i "ponti")
        Collider2D[] ownColliders = GetComponents<Collider2D>();

        if (ownColliders.Length > 0) {

            Bounds union = ownColliders[0].bounds;

            for (int i = 1; i < ownColliders.Length; i++) {
                union.Encapsulate(ownColliders[i].bounds);
            }

            roomWorldBounds = union;
            roomWorldBoundsResolved = true;
            result = union;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Area di una singola cella, ricavata suddividendo l'area totale della stanza
    /// secondo il bounding box della forma. Non richiede collider per cella.
    /// </summary>
    private Bounds GetCellWorldBounds(Vector2Int cellOffset, Bounds total, Vector2Int shapeMin, Vector2Int shapeSize) {

        float cellWidth = total.size.x / shapeSize.x;
        float cellHeight = total.size.y / shapeSize.y;

        // x cresce verso destra, y cresce verso il basso: parto dall'alto
        float centerX = total.min.x + (cellOffset.x - shapeMin.x + 0.5f) * cellWidth;
        float centerY = total.max.y - (cellOffset.y - shapeMin.y + 0.5f) * cellHeight;

        return new Bounds(
            new Vector3(centerX, centerY, total.center.z),
            new Vector3(cellWidth, cellHeight, total.size.z)
        );
    }

    /// <summary>
    /// Passa alla camera l'area della stanza, descritta CELLA PER CELLA.
    /// E' la camera a ricavarne i limiti asse per asse in funzione della posizione del
    /// player, interpolando fra una fila e l'altra: cosi' anche su una forma concava
    /// (a L, a T) il movimento resta continuo, senza gli scatti di un clamp per cella.
    /// </summary>
    private void UpdateCameraBounds() {

        if (!TryGetRoomWorldBounds(out Bounds total)) return;

        if (cameraRegion == null) cameraRegion = new List<Bounds>();

        cameraRegion.Clear();

        Vector2Int[] shape = GetShapeOffsets();

        GetShapeBounds(shape, out Vector2Int shapeMin, out Vector2Int shapeSize);

        for (int i = 0; i < shape.Length; i++) {
            cameraRegion.Add(GetCellWorldBounds(shape[i], total, shapeMin, shapeSize));
        }

        if (cameraRegion.Count == 0) cameraRegion.Add(total);

        if (Camera.main != null && Camera.main.TryGetComponent<CameraDungeonBehaviour>(out CameraDungeonBehaviour cdb)) {
            cdb.SetRoomCells(cameraRegion);
        }
    }

    // ===============================
    // TRIGGER
    // ===============================
    // Una stanza multi-cella puo' avere piu' di un BoxCollider2D trigger (uno per cella):
    // il contatore evita che gli eventi vengano lanciati piu' volte.

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.GetComponent<Player>()) return;

        playerTriggerCount++;

        if (playerTriggerCount > 1) return; // gia' dentro la stanza

        OnRoomEnter?.Invoke(this, EventArgs.Empty);
        OnAnyRoomEntered?.Invoke(this);

        if (DungeonGenerator.Instance != null && !DungeonGenerator.Instance.IsDungeonReady) {
            return;
        }

        UpdateCameraBounds();

        if (!isVisited) {
            MarkAsVisited();

            if (roomType != RoomType.StartRoom) {
                StartRoom();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.GetComponent<Player>()) return;

        playerTriggerCount = Mathf.Max(0, playerTriggerCount - 1);

        if (playerTriggerCount > 0) return; // ancora dentro un'altra cella della stessa stanza

        OnRoomExit?.Invoke(this, EventArgs.Empty);
    }

    // prima entrata
    private void StartRoom() {
        // per queste camere per ora non si chiudono semplicemente le porte
        if (roomType == RoomType.TrapRoom || roomType == RoomType.VendorRoom || roomType == RoomType.TreasureRoom || roomType == RoomType.StartRoom) return;
        // startRoom anche rimane chiusa, in quanto si aspetta che il player raccolga l'arma iniziale

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.SetCurrentRoom(this);
        }

        // per le altre stanze, chiudo le porte
        CloseDoors();
        // se necessario, spawno i nemici
        SpawnEnemies();
    }

    // spawn nemici (placeholder)
    private void SpawnEnemies() {
        if (EnemySpawner.Instance == null) {
            Debug.Log("Enemy spawner non trovato");
            return;
        }

        EnemySpawner.Instance.SetSpawner(enemiesSpawnPoints, minEnemiesToSpawn);
        EnemySpawner.Instance.SpawnEnemies();
    }

    // stanza completata
    public void RoomCleared() { // richiamato in enemyspawner (se la stanza ha nemici)
        isCleared = true;
        OpenDoors();
        OnRoomCleared?.Invoke(this, EventArgs.Empty);
        Debug.Log($"Room cleared: {name}");
        OnAnyRoomCleared?.Invoke(this);
    }

    public void BakeRoomNavMesh() {
        if (navSurface == null) {
            navSurface = GetComponentInChildren<NavMeshSurface>();
        }

        if (navSurface == null) {
            Debug.LogWarning($"NavMeshSurface non trovata nella stanza {name}");
            return;
        }

        navSurface.BuildNavMesh();
    }
}

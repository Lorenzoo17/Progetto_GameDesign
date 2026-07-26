using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;

// 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

public class DungeonGenerator : MonoBehaviour {

    public class Cell {
        public bool visited = false;
        public bool[] status = new bool[4];

        // -1 = cella libera / stanza 1x1
        // >= 0 = indice del piazzamento multi-cella che occupa questa cella
        public int placementId = -1;
    }

    [System.Serializable]
    public class Rule {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        [Tooltip("Solo per le stanze multi-cella. Lascia vuoto per leggere la forma dal prefab. " +
                 "Se lo compili vince su tutto: elenca le celle occupate, la prima deve essere (0,0). " +
                 "Es. 2x1 orizzontale = (0,0) e (1,0); a L = (0,0), (1,0) e (0,1).")]
        public Vector2Int[] shapeOverride;

        public int ProbabilityOfSpawning(int x, int y) {

            if (x >= minPosition.x && x <= maxPosition.x &&
                y >= minPosition.y && y <= maxPosition.y) {
                return 1;
            }

            return 0;
        }
    }

    /// <summary>Risultato di un piazzamento di stanza multi-cella.</summary>
    private class Placement {
        public int id;
        public GameObject prefab;
        public Vector2Int anchor;
        public Vector2Int[] shape;
    }

    /// <summary>Come viene scavata la pianta del dungeon.</summary>
    public enum DungeonLayoutMode {
        /// <summary>Maze DFS che riempie TUTTE le celle della griglia (comportamento storico).</summary>
        FullMaze = 0,

        /// <summary>
        /// Stile Binding of Isaac: la griglia fa solo da contenitore, si genera un numero
        /// fissato di stanze che si espandono a macchia dal centro. Forma irregolare.
        /// </summary>
        Organic = 1
    }

    [Header("Dungeon Size")]
    [Tooltip("In modalita' Organic questa e' solo la griglia MASSIMA disponibile: " +
             "mettila abbondante (es. 9x9) e regola il numero di stanze qui sotto.")]
    public Vector2Int size;
    public Vector2 offset;

    [Header("Layout")]
    public DungeonLayoutMode layoutMode = DungeonLayoutMode.FullMaze;

    [Tooltip("Solo Organic: numero minimo di stanze del piano.")]
    public int minRooms = 10;

    [Tooltip("Solo Organic: numero massimo di stanze del piano.")]
    public int maxRooms = 14;

    [Tooltip("Solo Organic: probabilita' di aprire un ramo verso una cella libera. " +
             "Valori bassi = dungeon lungo e stretto, valori alti = piu' compatto e ramificato.")]
    [Range(0.05f, 1f)] public float branchChance = 0.5f;

    [Tooltip("Solo Organic: quante volte riprovare la generazione se non si raggiunge minRooms.")]
    public int generationAttempts = 40;

    [Header("Rooms")]
    public GameObject bossRoom;
    public GameObject powerUpRoom;
    public GameObject vendorRoom;
    public GameObject startRoom;

    private int startCell;

    public Rule[] normalRooms;

    [Header("Stanze multi-cella (2x1, a L, ...)")]
    [Tooltip("Prefab la cui RoomBehaviour dichiara piu' di una cella nella lista 'cells'. " +
             "Vengono piazzate DOPO la generazione del maze, fondendo celle adiacenti gia' collegate.")]
    public Rule[] multiCellRooms;

    [Tooltip("Numero massimo di stanze multi-cella per dungeon.")]
    public int maxMultiCellRooms = 2;

    [Tooltip("Probabilita' di tentare un piazzamento su una cella candidata.")]
    [Range(0f, 1f)] public float multiCellRoomChance = 0.6f;

    [Tooltip("Se attivo, una forma viene piazzata solo se le sue celle erano GIA' collegate " +
             "tra loro dal maze. Mantiene intatta la struttura del labirinto. " +
             "Se disattivo si creano scorciatoie/anelli.")]
    public bool requireInternalConnection = true;

    private List<Cell> board;
    private readonly List<Placement> placements = new List<Placement>();
    private readonly HashSet<int> reservedCells = new HashSet<int>();

    public static DungeonGenerator Instance { get; private set; }
    public bool IsDungeonReady { get; private set; }
    private RoomBehaviour startRoomBehaviour;

    // mappa cella di griglia -> stanza che la occupa (utile a minimap e altri sistemi)
    private readonly Dictionary<Vector2Int, RoomBehaviour> roomByCell = new Dictionary<Vector2Int, RoomBehaviour>();

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

    // ===============================
    // HELPER GRIGLIA
    // ===============================

    private int GridToIndex(Vector2Int gridPosition) {
        return gridPosition.x + gridPosition.y * size.x;
    }

    private Vector2Int IndexToGrid(int index) {
        return new Vector2Int(index % size.x, index / size.x);
    }

    private bool IsInsideGrid(Vector2Int gridPosition) {
        return gridPosition.x >= 0 && gridPosition.x < size.x &&
               gridPosition.y >= 0 && gridPosition.y < size.y;
    }

    private Vector3 GridToWorld(Vector2Int gridPosition) {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        return new Vector3(
            (gridPosition.x - offsetX) * offset.x,
            -(gridPosition.y - offsetY) * offset.y,
            0f
        );
    }

    /// <summary>True se le due celle adiacenti sono collegate da una porta del maze.</summary>
    private bool AreCellsLinked(Vector2Int a, Vector2Int b) {
        if (!IsInsideGrid(a) || !IsInsideGrid(b)) return false;

        Vector2Int delta = b - a;

        for (int d = 0; d < 4; d++) {
            if (RoomShape.Directions[d] != delta) continue;

            return board[GridToIndex(a)].status[d] && board[GridToIndex(b)].status[RoomShape.Opposite(d)];
        }

        return false;
    }

    // ===============================
    // GENERAZIONE DUNGEON
    // ===============================
    void GenerateDungeon() {

        placements.Clear();
        reservedCells.Clear();
        roomByCell.Clear();

        // Trova celle valide
        List<int> validCells = new List<int>();

        for (int i = 0; i < board.Count; i++) {
            if (board[i].visited) {
                validCells.Add(i);
            }
        }

        // first room
        validCells.Remove(startCell);

        // int bossCell = GetFarthestCell(); // come era prima
        // Boss nella stanza piu' lontana che e' un deadEnd (una sola porta di accesso)
        int bossCell = GetFarthestDeadEndCell();

        validCells.Remove(bossCell);

        // PowerUp e Vendor: preferisco i dead-end, cosi' non finiscono in mezzo al percorso.
        // -1 = non piazzata (dungeon troppo piccolo)
        int powerUpCell = TakeSpecialCell(validCells);
        int vendorCell = TakeSpecialCell(validCells);

        if (powerUpCell == -1 || vendorCell == -1) {
            Debug.LogWarning("[Layout] Stanze insufficienti per tutte le stanze speciali: " +
                             "aumenta minRooms/maxRooms (o la size della griglia).");
        }

        // le stanze speciali restano sempre 1x1: le riservo prima del merge
        reservedCells.Add(startCell);
        reservedCells.Add(bossCell);
        if (powerUpCell != -1) reservedCells.Add(powerUpCell);
        if (vendorCell != -1) reservedCells.Add(vendorCell);

        // FASE DI MERGE: piazza le stanze multi-cella sulle celle rimaste
        PlaceMultiCellRooms();

        // SPAWN STANZE 1x1
        for (int i = 0; i < size.x; i++) {
            for (int j = 0; j < size.y; j++) {

                int index = i + j * size.x;
                Cell currentCell = board[index];

                if (!currentCell.visited) continue;

                // gia' coperta da una stanza multi-cella
                if (currentCell.placementId != -1) continue;

                GameObject roomPrefab;

                if (index == startCell) {
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

                Vector2Int gridPosition = new Vector2Int(i, j);

                var newRoom = Instantiate(
                    roomPrefab,
                    GridToWorld(gridPosition),
                    Quaternion.identity,
                    transform
                ).GetComponent<RoomBehaviour>();

                newRoom.SetPlacement(gridPosition, RoomShape.Single);
                newRoom.UpdateRoom(currentCell.status);
                newRoom.BakeRoomNavMesh(); // faccio il bake qui

                newRoom.name += $" {i}-{j}";

                RegisterRoom(newRoom);

                if (index == startCell) {
                    startRoomBehaviour = newRoom;
                }
            }
        }

        // SPAWN STANZE MULTI-CELLA
        for (int p = 0; p < placements.Count; p++) {
            SpawnPlacement(placements[p]);
        }
    }

    /// <summary>
    /// Estrae una cella per una stanza speciale, preferendo i dead-end (una sola porta).
    /// Ritorna -1 se non ci sono piu' celle disponibili.
    /// </summary>
    private int TakeSpecialCell(List<int> validCells) {

        if (validCells.Count == 0) return -1;

        List<int> deadEnds = new List<int>();

        for (int i = 0; i < validCells.Count; i++) {
            if (CountOpenDoors(board[validCells[i]]) == 1) deadEnds.Add(validCells[i]);
        }

        List<int> source = deadEnds.Count > 0 ? deadEnds : validCells;

        int chosen = source[Random.Range(0, source.Count)];
        validCells.Remove(chosen);

        return chosen;
    }

    private void RegisterRoom(RoomBehaviour room) {
        if (room == null) return;

        foreach (Vector2Int cell in room.OccupiedGridPositions) {
            roomByCell[cell] = room;
        }
    }

    private void SpawnPlacement(Placement placement) {

        var newRoom = Instantiate(
            placement.prefab,
            GridToWorld(placement.anchor),
            Quaternion.identity,
            transform
        ).GetComponent<RoomBehaviour>();

        if (newRoom == null) {
            Debug.LogWarning($"Il prefab {placement.prefab.name} non ha una RoomBehaviour.");
            return;
        }

        newRoom.SetPlacement(placement.anchor, placement.shape);

        // stato porte cella per cella
        Dictionary<Vector2Int, bool[]> statusByCell = new Dictionary<Vector2Int, bool[]>();

        for (int i = 0; i < placement.shape.Length; i++) {
            Vector2Int cellOffset = placement.shape[i];
            Vector2Int gridPosition = placement.anchor + cellOffset;

            statusByCell[cellOffset] = board[GridToIndex(gridPosition)].status;
        }

        newRoom.UpdateRoom(statusByCell);
        newRoom.BakeRoomNavMesh();

        newRoom.name += $" {placement.anchor.x}-{placement.anchor.y}";

        RegisterRoom(newRoom);
    }

    // ===============================
    // PIAZZAMENTO STANZE MULTI-CELLA
    // ===============================

    [Tooltip("Stampa in console il dettaglio della fase di merge (utile per capire perche' " +
             "una stanza multi-cella non viene piazzata).")]
    public bool logMultiCellPlacement = true;

    private void PlaceMultiCellRooms() {

        if (multiCellRooms == null || multiCellRooms.Length == 0) {
            if (logMultiCellPlacement) Debug.Log("[MultiCell] La lista 'Multi Cell Rooms' e' vuota.");
            return;
        }

        if (maxMultiCellRooms <= 0) {
            Debug.LogWarning($"[MultiCell] maxMultiCellRooms = {maxMultiCellRooms}: nessuna stanza multi-cella verra' piazzata.");
            return;
        }

        if (multiCellRoomChance <= 0f) {
            Debug.LogWarning("[MultiCell] multiCellRoomChance = 0: nessuna stanza multi-cella verra' piazzata.");
            return;
        }

        // filtra le regole valide (forma > 1 cella) e memorizza la forma risolta
        List<Rule> pool = new List<Rule>();
        List<Vector2Int[]> poolShapes = new List<Vector2Int[]>();

        for (int i = 0; i < multiCellRooms.Length; i++) {
            Rule rule = multiCellRooms[i];

            if (rule == null || rule.room == null) continue;

            Vector2Int[] shape;
            string source;

            // 1) forma forzata nell'inspector: vince su tutto
            if (rule.shapeOverride != null && rule.shapeOverride.Length > 1) {

                shape = rule.shapeOverride;
                source = "Shape Override";

                if (!IsValidShape(shape)) {
                    Debug.LogError($"[MultiCell] '{rule.room.name}': lo Shape Override non e' valido " +
                                   "(deve contenere (0,0), non avere duplicati ed essere contiguo).", rule.room);
                    continue;
                }
            }
            else {
                // 2) forma letta dal prefab
                RoomBehaviour prefabBehaviour = rule.room.GetComponent<RoomBehaviour>();

                if (prefabBehaviour == null) {
                    Debug.LogError($"[MultiCell] '{rule.room.name}' NON ha una RoomBehaviour (prefab importato male?): " +
                                   "selezionalo nel Project e lancia Tools > Dungeon > Configura celle multi-cella.", rule.room);
                    continue;
                }

                shape = prefabBehaviour.GetShapeOffsets();
                source = "prefab";

                if (shape.Length <= 1) {
                    Debug.LogWarning($"[MultiCell] '{rule.room.name}' viene letta come stanza 1x1.\n" +
                                     $"    {prefabBehaviour.GetShapeDiagnostics()}\n" +
                                     "    Soluzione rapida: compila 'Shape Override' su questa riga " +
                                     "nell'inspector del Generator, es. (0,0) e (1,0).", rule.room);
                    continue;
                }
            }

            if (logMultiCellPlacement) {
                Debug.Log($"[MultiCell] '{rule.room.name}' -> forma di {shape.Length} celle da {source}: {ShapeToString(shape)}");
            }

            pool.Add(rule);
            poolShapes.Add(shape);
        }

        if (pool.Count == 0) {
            Debug.LogWarning("[MultiCell] Nessun prefab multi-cella valido: fase di merge saltata.");
            return;
        }

        // ordine casuale delle celle candidate come ancora
        List<int> anchors = new List<int>();

        for (int i = 0; i < board.Count; i++) {
            if (board[i].visited) anchors.Add(i);
        }

        Shuffle(anchors);

        int placed = 0;

        for (int a = 0; a < anchors.Count && placed < maxMultiCellRooms; a++) {

            Vector2Int anchor = IndexToGrid(anchors[a]);

            if (Random.value > multiCellRoomChance) continue;

            // provo le regole in ordine casuale
            List<int> order = new List<int>();
            for (int r = 0; r < pool.Count; r++) order.Add(r);
            Shuffle(order);

            for (int o = 0; o < order.Count; o++) {

                Rule rule = pool[order[o]];
                Vector2Int[] shape = poolShapes[order[o]];

                if (!CanPlaceShape(anchor, shape, rule)) continue;

                ApplyShape(anchor, shape, rule.room);
                placed++;

                if (logMultiCellPlacement) {
                    Debug.Log($"[MultiCell] '{rule.room.name}' piazzata con ancora {anchor}.");
                }

                break;
            }
        }

        if (logMultiCellPlacement) {
            Debug.Log($"[MultiCell] Stanze multi-cella piazzate: {placed} / {maxMultiCellRooms}.");
        }
    }

    /// <summary>Una forma e' valida se contiene (0,0), non ha duplicati ed e' contigua.</summary>
    private static bool IsValidShape(Vector2Int[] shape) {

        if (shape == null || shape.Length < 2) return false;

        List<Vector2Int> unique = new List<Vector2Int>();
        bool hasAnchor = false;

        for (int i = 0; i < shape.Length; i++) {
            if (shape[i] == Vector2Int.zero) hasAnchor = true;
            if (unique.Contains(shape[i])) return false;

            unique.Add(shape[i]);
        }

        if (!hasAnchor) return false;

        return RoomShape.IsContiguous(unique);
    }

    private static string ShapeToString(Vector2Int[] shape) {
        string result = "";

        for (int i = 0; i < shape.Length; i++) {
            result += (i > 0 ? " " : "") + shape[i];
        }

        return result;
    }

    private bool CanPlaceShape(Vector2Int anchor, Vector2Int[] shape, Rule rule) {

        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        // la regola di posizione viene valutata sull'ancora.
        // Se min e max sono entrambi (0,0) la regola e' considerata "non impostata"
        // e la forma puo' essere piazzata ovunque: evita che una stanza multi-cella
        // appena aggiunta nell'inspector non compaia mai.
        bool positionRuleSet = rule.minPosition != rule.maxPosition ||
                               rule.minPosition != Vector2Int.zero;

        if (positionRuleSet && rule.ProbabilityOfSpawning(anchor.x - offsetX, anchor.y - offsetY) == 0) return false;

        for (int i = 0; i < shape.Length; i++) {

            Vector2Int gridPosition = anchor + shape[i];

            if (!IsInsideGrid(gridPosition)) return false;

            int index = GridToIndex(gridPosition);

            if (!board[index].visited) return false;      // cella non scavata dal maze
            if (board[index].placementId != -1) return false; // gia' occupata da un'altra forma
            if (reservedCells.Contains(index)) return false;  // start / boss / vendor / powerup
        }

        if (requireInternalConnection && !IsShapeInternallyConnected(anchor, shape)) return false;

        return true;
    }

    /// <summary>
    /// Verifica che le celle della forma siano gia' collegate tra loro dalle porte del maze,
    /// usando SOLO archi interni alla forma. Se lo sono, fondere le celle non altera la
    /// connettivita' globale del dungeon.
    /// </summary>
    private bool IsShapeInternallyConnected(Vector2Int anchor, Vector2Int[] shape) {

        if (shape.Length <= 1) return true;

        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>();

        for (int i = 0; i < shape.Length; i++) {
            remaining.Add(anchor + shape[i]);
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int start = anchor + shape[0];

        remaining.Remove(start);
        queue.Enqueue(start);

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();

            for (int d = 0; d < 4; d++) {
                Vector2Int neighbour = current + RoomShape.Directions[d];

                if (!remaining.Contains(neighbour)) continue;
                if (!AreCellsLinked(current, neighbour)) continue;

                remaining.Remove(neighbour);
                queue.Enqueue(neighbour);
            }
        }

        return remaining.Count == 0;
    }

    /// <summary>
    /// Marca le celle come occupate dalla forma e rimuove le porte interne:
    /// il collegamento tra quelle celle e' ora garantito dalla stanza stessa.
    /// </summary>
    private void ApplyShape(Vector2Int anchor, Vector2Int[] shape, GameObject prefab) {

        int id = placements.Count;

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        for (int i = 0; i < shape.Length; i++) {
            occupied.Add(anchor + shape[i]);
        }

        foreach (Vector2Int gridPosition in occupied) {

            int index = GridToIndex(gridPosition);
            board[index].placementId = id;

            for (int d = 0; d < 4; d++) {

                Vector2Int neighbour = gridPosition + RoomShape.Directions[d];

                if (!occupied.Contains(neighbour)) continue;

                // lato interno: niente porta, niente muro
                board[index].status[d] = false;
            }
        }

        placements.Add(new Placement {
            id = id,
            prefab = prefab,
            anchor = anchor,
            shape = shape
        });
    }

    private static void Shuffle<T>(IList<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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

    // TROVA STANZA PIU' LONTANA (BOSS)
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

    // ===============================
    // GENERAZIONE DELLA PIANTA
    // ===============================

    void MazeGenerator() {

        if (layoutMode == DungeonLayoutMode.Organic) {
            GenerateOrganicLayout();
        }
        else {
            GenerateFullMaze();
        }

        GenerateDungeon();
    }

    private void ResetBoard() {

        board = new List<Cell>();

        for (int i = 0; i < size.x * size.y; i++) {
            board.Add(new Cell());
        }

        startCell = (size.x / 2) + (size.y / 2) * size.x;
    }

    private int CountVisitedNeighbours(Vector2Int gridPosition) {

        int count = 0;

        for (int d = 0; d < 4; d++) {

            Vector2Int neighbour = gridPosition + RoomShape.Directions[d];

            if (!IsInsideGrid(neighbour)) continue;
            if (board[GridToIndex(neighbour)].visited) count++;
        }

        return count;
    }

    private void Link(Vector2Int from, Vector2Int to, int direction) {
        board[GridToIndex(from)].status[direction] = true;
        board[GridToIndex(to)].status[RoomShape.Opposite(direction)] = true;
    }

    /// <summary>
    /// Layout stile Binding of Isaac.
    ///
    /// Si parte dal centro e si espande a coda: per ogni stanza in coda si prova ad aprire
    /// un ramo verso ognuno dei 4 lati. Una cella nuova viene accettata solo se tocca
    /// UNA SOLA stanza gia' esistente: e' questa regola a dare la forma frastagliata tipica
    /// di TBOI, a evitare i blocchi compatti e a garantire che la pianta resti un albero
    /// (nessun anello, quindi le dead-end per boss/tesoro esistono sempre).
    ///
    /// La griglia fa solo da contenitore: il numero di stanze e' deciso da minRooms/maxRooms.
    /// </summary>
    private void GenerateOrganicLayout() {

        int cellCount = size.x * size.y;

        int min = Mathf.Max(1, minRooms);
        int max = Mathf.Max(min, maxRooms);

        if (maxRooms <= 0) {
            // parametri mai impostati sull'oggetto in scena: ricavo un default sensato
            max = Mathf.Max(4, Mathf.RoundToInt(cellCount * 0.45f));
            min = Mathf.Max(3, max - 3);
            Debug.LogWarning($"[Layout] minRooms/maxRooms non impostati: uso {min}-{max} stanze.");
        }

        // non posso chiedere piu' stanze di quante la regola di adiacenza ne permetta:
        // in pratica non si supera circa la meta' delle celle della griglia
        int hardCap = Mathf.Max(1, Mathf.FloorToInt(cellCount * 0.55f));

        if (max > hardCap) {
            Debug.LogWarning($"[Layout] La griglia {size.x}x{size.y} non regge {max} stanze: limito a {hardCap}. " +
                             "Aumenta 'size' per avere piani piu' grandi.");
            max = hardCap;
            min = Mathf.Min(min, max);
        }

        int target = Random.Range(min, max + 1);
        int attempts = Mathf.Max(1, generationAttempts);
        int best = 0;
        List<Cell> bestBoard = null;

        for (int attempt = 0; attempt < attempts; attempt++) {

            int placed = TryGrowLayout(target);

            if (placed >= target) {
                if (logMultiCellPlacement) {
                    Debug.Log($"[Layout] Organic: {placed} stanze (target {target}) al tentativo {attempt + 1}.");
                }
                return;
            }

            if (placed > best) {
                best = placed;
                bestBoard = CloneBoard(board);
            }
        }

        // nessun tentativo ha raggiunto il target: tengo il migliore
        if (bestBoard != null) board = bestBoard;

        Debug.LogWarning($"[Layout] Organic: raggiunte {best} stanze su {target} richieste. " +
                         "Aumenta 'size' oppure abbassa minRooms/maxRooms.");
    }

    /// <summary>Un singolo tentativo di espansione. Ritorna il numero di stanze create.</summary>
    private int TryGrowLayout(int target) {

        ResetBoard();

        board[startCell].visited = true;
        int roomCount = 1;

        Queue<int> frontier = new Queue<int>();
        frontier.Enqueue(startCell);

        // giri di espansione: quando la coda si svuota la riempio con le stanze esistenti,
        // cosi' i rami possono continuare a crescere anche dopo il primo passaggio
        int safety = 0;

        while (roomCount < target && safety < 10000) {

            if (frontier.Count == 0) {

                List<int> visitedCells = new List<int>();

                for (int i = 0; i < board.Count; i++) {
                    if (board[i].visited) visitedCells.Add(i);
                }

                Shuffle(visitedCells);

                // se nessuna stanza puo' piu' espandersi, mi fermo
                bool canExpand = false;

                for (int i = 0; i < visitedCells.Count; i++) {
                    if (HasFreeNeighbour(IndexToGrid(visitedCells[i]))) {
                        canExpand = true;
                        break;
                    }
                }

                if (!canExpand) break;

                for (int i = 0; i < visitedCells.Count; i++) frontier.Enqueue(visitedCells[i]);
            }

            safety++;

            int currentIndex = frontier.Dequeue();
            Vector2Int current = IndexToGrid(currentIndex);

            // ordine casuale dei lati, altrimenti i dungeon crescono sempre nella stessa direzione
            List<int> directions = new List<int> { 0, 1, 2, 3 };
            Shuffle(directions);

            for (int d = 0; d < directions.Count; d++) {

                if (roomCount >= target) break;

                int direction = directions[d];
                Vector2Int neighbour = current + RoomShape.Directions[direction];

                if (!IsInsideGrid(neighbour)) continue;

                int neighbourIndex = GridToIndex(neighbour);

                if (board[neighbourIndex].visited) continue;

                // REGOLA CHIAVE: la nuova stanza puo' toccare solo la stanza da cui nasce
                if (CountVisitedNeighbours(neighbour) > 1) continue;

                if (Random.value > branchChance) continue;

                board[neighbourIndex].visited = true;
                Link(current, neighbour, direction);

                roomCount++;
                frontier.Enqueue(neighbourIndex);
            }
        }

        return roomCount;
    }

    private bool HasFreeNeighbour(Vector2Int gridPosition) {

        for (int d = 0; d < 4; d++) {

            Vector2Int neighbour = gridPosition + RoomShape.Directions[d];

            if (!IsInsideGrid(neighbour)) continue;
            if (board[GridToIndex(neighbour)].visited) continue;
            if (CountVisitedNeighbours(neighbour) > 1) continue;

            return true;
        }

        return false;
    }

    private static List<Cell> CloneBoard(List<Cell> source) {

        List<Cell> copy = new List<Cell>(source.Count);

        for (int i = 0; i < source.Count; i++) {

            Cell cell = new Cell {
                visited = source[i].visited,
                placementId = source[i].placementId
            };

            for (int d = 0; d < 4; d++) cell.status[d] = source[i].status[d];

            copy.Add(cell);
        }

        return copy;
    }

    // MAZE GENERATION (riempie tutta la griglia)
    void GenerateFullMaze() {

        ResetBoard();

        int currentCell = startCell;

        Stack<int> path = new Stack<int>();

        int k = 0;
        int maxSteps = Mathf.Max(1000, board.Count * 10);

        while (k < maxSteps) {
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

        if (!IsInsideGrid(gridPosition)) return null;

        int index = GridToIndex(gridPosition);

        if (index < 0 || index >= board.Count) return null;

        return board[index].status;
    }

    /// <summary>Stanza che occupa una determinata cella di griglia (null se nessuna).</summary>
    public RoomBehaviour GetRoomAt(Vector2Int gridPosition) {
        return roomByCell.TryGetValue(gridPosition, out RoomBehaviour room) ? room : null;
    }

    /// <summary>True se le due celle appartengono alla stessa stanza multi-cella.</summary>
    public bool AreCellsSameRoom(Vector2Int a, Vector2Int b) {
        RoomBehaviour roomA = GetRoomAt(a);

        if (roomA == null) return false;

        return roomA == GetRoomAt(b);
    }

    private int CountOpenDoors(Cell cell) {
        int count = 0;

        for (int i = 0; i < cell.status.Length; i++) {
            if (cell.status[i]) {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Distanza in numero di stanze da startCell, seguendo le porte.
    /// -1 = non raggiungibile / cella vuota.
    /// </summary>
    private int[] GetDistancesFromStart() {

        int[] distance = new int[board.Count];

        for (int i = 0; i < distance.Length; i++) distance[i] = -1;

        distance[startCell] = 0;

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startCell);

        while (queue.Count > 0) {

            int currentIndex = queue.Dequeue();
            Vector2Int current = IndexToGrid(currentIndex);

            for (int d = 0; d < 4; d++) {

                if (!board[currentIndex].status[d]) continue;

                Vector2Int neighbour = current + RoomShape.Directions[d];

                if (!IsInsideGrid(neighbour)) continue;

                int neighbourIndex = GridToIndex(neighbour);

                if (!board[neighbourIndex].visited) continue;
                if (distance[neighbourIndex] != -1) continue;

                distance[neighbourIndex] = distance[currentIndex] + 1;
                queue.Enqueue(neighbourIndex);
            }
        }

        return distance;
    }

    int GetFarthestDeadEndCell() {

        // distanza reale di percorso: con un layout irregolare la distanza dal centro
        // della griglia non vuole dire niente
        int[] distance = GetDistancesFromStart();

        int bestIndex = -1;
        int maxDist = -1;

        for (int i = 0; i < board.Count; i++) {
            if (!board[i].visited) continue;
            if (i == startCell) continue;
            if (distance[i] < 0) continue;

            // Boss room solo in stanze con UNA sola porta
            if (CountOpenDoors(board[i]) != 1) continue;

            if (distance[i] > maxDist) {
                maxDist = distance[i];
                bestIndex = i;
            }
        }

        if (bestIndex != -1) return bestIndex;

        // Fallback 1: la stanza raggiungibile piu' lontana, anche se non e' un dead-end
        for (int i = 0; i < board.Count; i++) {
            if (!board[i].visited || i == startCell || distance[i] < 0) continue;

            if (distance[i] > maxDist) {
                maxDist = distance[i];
                bestIndex = i;
            }
        }

        if (bestIndex != -1) {
            Debug.LogWarning("Nessuna stanza dead-end trovata per la boss room. Uso la stanza piu' lontana dallo start.");
            return bestIndex;
        }

        // Fallback 2: vecchio metodo
        Debug.LogWarning("Nessuna stanza raggiungibile per la boss room. Uso la stanza piu' lontana dal centro.");
        return GetFarthestCell();
    }
}

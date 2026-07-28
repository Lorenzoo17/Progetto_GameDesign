using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraDungeonBehaviour : MonoBehaviour {

    private Transform target;

    [SerializeField] private float cameraInterpolationValue = 5f;
    [SerializeField] private Vector3 offset;

    private Camera cam;

    private float startInterpolationValue;
    private Coroutine quickClampCoroutine;

    // ===============================
    // AREA DELLA STANZA
    // ===============================
    // Una stanza puo' occupare piu' celle e avere forma concava (a L, a T).
    // Invece di clampare su un rettangolo, tengo l'elenco delle celle e ricavo i limiti
    // ASSE PER ASSE in funzione di dove sta il player:
    //   - limiti X = estensione orizzontale della stanza all'altezza del player
    //   - limiti Y = estensione verticale della stanza alla ascissa del player
    // Fra una fila e l'altra i limiti vengono interpolati, quindi si spostano con
    // continuita' e la camera non fa mai salti passando da un braccio all'altro.

    private struct Span {
        public float center;   // coordinata dell'asse su cui la fila e' allineata
        public float min;      // estensione della fila sull'altro asse
        public float max;
    }

    private readonly List<Bounds> roomCells = new List<Bounds>();
    private readonly List<Span> rows = new List<Span>();      // estensione X, indicizzata per Y
    private readonly List<Span> columns = new List<Span>();   // estensione Y, indicizzata per X

    private bool hasRoomBounds;

    private void Awake() {
        cam = GetComponent<Camera>();
    }

    private void Start() {
        startInterpolationValue = cameraInterpolationValue;
        target = Player.Instance.transform;
    }

    private void LateUpdate() {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (hasRoomBounds) {
            desiredPosition = ClampToRoom(desiredPosition);
        }

        desiredPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            cameraInterpolationValue * Time.deltaTime
        );
    }

    /// <summary>Overload storico: stanza 1x1 descritta da un collider.</summary>
    public void SetRoomBounds(BoxCollider2D roomBounds) {

        if (roomBounds == null) {
            hasRoomBounds = false;
            return;
        }

        SetRoomBounds(roomBounds.bounds);
    }

    /// <summary>Stanza rettangolare singola.</summary>
    public void SetRoomBounds(Bounds bounds) {
        SetRoomCells(new List<Bounds> { bounds });
    }

    /// <summary>
    /// Stanza descritta cella per cella. Serve per le stanze multi-cella: da qui si
    /// ricavano le file e le colonne usate per il clamp continuo.
    /// </summary>
    public void SetRoomCells(List<Bounds> cells) {

        if (cells == null || cells.Count == 0) return;

        if (hasRoomBounds && SameCells(cells)) return;

        roomCells.Clear();
        roomCells.AddRange(cells);

        BuildAxisSpans();

        hasRoomBounds = true;

        if (quickClampCoroutine != null)
            StopCoroutine(quickClampCoroutine);

        quickClampCoroutine = StartCoroutine(QuickClamp());
    }

    private bool SameCells(List<Bounds> cells) {

        if (roomCells.Count != cells.Count) return false;

        for (int i = 0; i < cells.Count; i++) {
            if (roomCells[i] != cells[i]) return false;
        }

        return true;
    }

    private void BuildAxisSpans() {

        rows.Clear();
        columns.Clear();

        for (int i = 0; i < roomCells.Count; i++) {
            Bounds cell = roomCells[i];

            AddToSpans(rows, cell.center.y, cell.min.x, cell.max.x);
            AddToSpans(columns, cell.center.x, cell.min.y, cell.max.y);
        }

        rows.Sort((a, b) => a.center.CompareTo(b.center));
        columns.Sort((a, b) => a.center.CompareTo(b.center));
    }

    private static void AddToSpans(List<Span> spans, float center, float min, float max) {

        for (int i = 0; i < spans.Count; i++) {

            if (Mathf.Abs(spans[i].center - center) > 0.01f) continue;

            Span existing = spans[i];
            existing.min = Mathf.Min(existing.min, min);
            existing.max = Mathf.Max(existing.max, max);
            spans[i] = existing;

            return;
        }

        spans.Add(new Span { center = center, min = min, max = max });
    }

    /// <summary>
    /// Estensione della stanza a una data coordinata, interpolata linearmente fra i centri
    /// delle file adiacenti: e' questa interpolazione a rendere il movimento continuo.
    /// </summary>
    private static void GetSpanAt(List<Span> spans, float coordinate, out float min, out float max) {

        min = 0f;
        max = 0f;

        if (spans.Count == 0) return;

        if (spans.Count == 1 || coordinate <= spans[0].center) {
            min = spans[0].min;
            max = spans[0].max;
            return;
        }

        Span last = spans[spans.Count - 1];

        if (coordinate >= last.center) {
            min = last.min;
            max = last.max;
            return;
        }

        for (int i = 0; i < spans.Count - 1; i++) {

            if (coordinate < spans[i].center || coordinate > spans[i + 1].center) continue;

            float t = Mathf.InverseLerp(spans[i].center, spans[i + 1].center, coordinate);

            min = Mathf.Lerp(spans[i].min, spans[i + 1].min, t);
            max = Mathf.Lerp(spans[i].max, spans[i + 1].max, t);

            return;
        }

        min = spans[0].min;
        max = spans[0].max;
    }

    private Vector3 ClampToRoom(Vector3 desiredPosition) {

        Vector3 playerPosition = target != null ? target.position : desiredPosition;

        GetSpanAt(rows, playerPosition.y, out float roomMinX, out float roomMaxX);
        GetSpanAt(columns, playerPosition.x, out float roomMinY, out float roomMaxY);

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = roomMinX + cameraWidth;
        float maxX = roomMaxX - cameraWidth;

        float minY = roomMinY + cameraHeight;
        float maxY = roomMaxY - cameraHeight;

        // Se la stanza e' piu' piccola della camera, blocca al centro
        if (minX > maxX) {
            desiredPosition.x = (roomMinX + roomMaxX) * 0.5f;
        }
        else {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        if (minY > maxY) {
            desiredPosition.y = (roomMinY + roomMaxY) * 0.5f;
        }
        else {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        return desiredPosition;
    }

    private IEnumerator QuickClamp() {
        cameraInterpolationValue = startInterpolationValue * 4;

        yield return new WaitForSeconds(1f);

        cameraInterpolationValue = startInterpolationValue;
        quickClampCoroutine = null;
    }
}

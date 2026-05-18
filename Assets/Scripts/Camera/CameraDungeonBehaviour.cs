using UnityEngine;
using System.Collections;

public class CameraDungeonBehaviour : MonoBehaviour {

    private Transform target;

    [SerializeField] private float cameraInterpolationValue = 5f;
    [SerializeField] private Vector3 offset;

    private BoxCollider2D currentRoomBounds;
    private Camera cam;

    private float startInterpolationValue;
    private Coroutine quickClampCoroutine;

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

        if (currentRoomBounds != null) {
            desiredPosition = ClampToRoom(desiredPosition);
        }

        desiredPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            cameraInterpolationValue * Time.deltaTime
        );
    }

    public void SetRoomBounds(BoxCollider2D roomBounds) {
        currentRoomBounds = roomBounds;
        
        if(quickClampCoroutine != null)
            StopCoroutine(quickClampCoroutine);

        quickClampCoroutine = StartCoroutine(QuickClamp());
    }

    private Vector3 ClampToRoom(Vector3 desiredPosition) {
        Bounds bounds = currentRoomBounds.bounds;

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = bounds.min.x + cameraWidth;
        float maxX = bounds.max.x - cameraWidth;

        float minY = bounds.min.y + cameraHeight;
        float maxY = bounds.max.y - cameraHeight;

        // Se la stanza è più piccola della camera, blocca al centro
        if (minX > maxX) {
            desiredPosition.x = bounds.center.x;
        }
        else {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        if (minY > maxY) {
            desiredPosition.y = bounds.center.y;
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
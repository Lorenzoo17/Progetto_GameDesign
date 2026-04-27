using UnityEngine;

public class CameraDungeonBehaviour : MonoBehaviour {

    private Vector3 targetPosition;
    [SerializeField] private float cameraPositionInterpolationValue;
    public void MoveToRoom(Transform target) {
        targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private void LateUpdate() {
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraPositionInterpolationValue);
    }
}

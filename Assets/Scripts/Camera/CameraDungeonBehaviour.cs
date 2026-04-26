using UnityEngine;

public class CameraDungeonBehaviour : MonoBehaviour {
    public void MoveToRoom(Transform target) {
        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );
    }
}

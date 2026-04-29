using UnityEngine;

public class CameraHubBehaviour : MonoBehaviour {

    private Transform target; // player ricavato tramite Singleton
    [SerializeField] private float cameraInterpolationValue;
    [SerializeField] private Vector3 offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        target = Player.Instance.transform;
    }

    private void LateUpdate() {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraInterpolationValue * Time.deltaTime);
    }
}

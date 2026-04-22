using UnityEngine;

public class MeleeEffect : MonoBehaviour {

    [SerializeField] private float effectMoveSpeed = 2f;
    private Vector2 moveDirection;

    private SpriteRenderer sr;
    private Color targetColor;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        targetColor = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
    }

    public void SetDirection(Vector2 dir) {
        moveDirection = dir.normalized;
    }

    void Update() {
        transform.position += (Vector3)(moveDirection * effectMoveSpeed * Time.deltaTime);

        sr.color = Color.Lerp(sr.color, targetColor, 0.05f);
    }
}

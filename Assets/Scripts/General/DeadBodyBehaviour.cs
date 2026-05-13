using UnityEngine;

public class DeadBodyBehaviour : MonoBehaviour {
    [SerializeField] private float throwVelocity = 10f;
    [SerializeField] private float rotationVelocity = 400f;
    [SerializeField] private GameObject throwEffect;

    private Rigidbody2D rb;
    private Vector2 direction;
    [SerializeField] private float targetScaleValue;
    [SerializeField] private float scaleSpeed = 2f;
    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        Invoke("Destruction", 2f); // distruzione automatica dopo 2 secondi
    }

    private void Update() {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            new Vector3(targetScaleValue, targetScaleValue, targetScaleValue),
            scaleSpeed * Time.deltaTime
        );
    }

    public void SetUpDeadBody(Vector2 attackDirection, Sprite sprite, string sortingLayer, int sortingOrder) {
        if(sr == null || rb == null) return;

        direction = attackDirection;
        sr.sprite = sprite;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;
        rb.linearVelocity = direction * throwVelocity;
        rb.angularVelocity = rotationVelocity;

        SpawnThrowEffect(direction);
    }

    private void SpawnThrowEffect(Vector2 attackDirection) {
        if (throwEffect == null) return;

        Vector2 oppositeDirection = -attackDirection.normalized;

        float angle = Mathf.Atan2(
            oppositeDirection.y,
            oppositeDirection.x
        ) * Mathf.Rad2Deg;

        GameObject effect = Instantiate(
            throwEffect,
            transform.position,
            Quaternion.Euler(0f, 0f, angle)
        );

        effect.transform.SetParent(transform, true);
    }

    private void Destruction() {
        Destroy(gameObject);
    }
}

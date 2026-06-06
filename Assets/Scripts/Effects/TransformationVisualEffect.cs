using UnityEngine;

public class AuraVisual : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private Gradient gradient;
    private float speed;

    public void Initialize(
        Gradient gradient,
        float speed)
    {
        this.gradient = gradient;
        this.speed = speed;
    }

    private void Update()
    {
        if (gradient == null)
            return;

        float t =
            Mathf.PingPong(
                Time.time * speed,
                1f);

        spriteRenderer.color =
            gradient.Evaluate(t);
    }
}

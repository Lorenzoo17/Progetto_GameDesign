using UnityEngine;

public class SpearAttackEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float scaleGrowth = 1.2f; // Cresce leggermente

    private float elapsedTime = 0f;
    private Color originalColor;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / fadeDuration);

        // Fade out
        Color newColor = originalColor;
        newColor.a = fadeCurve.Evaluate(progress);
        spriteRenderer.color = newColor;

        // Cresce leggermente
        float scale = Mathf.Lerp(1f, scaleGrowth, progress);
        transform.localScale = Vector3.one * scale;

        // Distruggi quando finisce
        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}

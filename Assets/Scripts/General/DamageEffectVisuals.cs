using UnityEngine;
using System.Collections;

public class DamageEffectVisuals : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Poison Effect")]
    [SerializeField] private Color poisonColor = new Color(0, 1, 0, 1); // Verde
    [SerializeField] private float poisonFlashDuration = 0.15f;
    [SerializeField] private AnimationCurve poisonFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Damage Boost Effect")]
    [SerializeField] private GameObject damageBoostParticlePrefab;
    [SerializeField] private Vector3 particleOffset = Vector3.zero;

    private Color originalColor;
    private Coroutine poisonFlashCoroutine;

    private void Start()
    {
        // 🔥 AUTO-DETECT se non è assegnato
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void PlayPoisonEffect()
    {
        Debug.Log($"[DamageEffectVisuals] Playing poison effect on {gameObject.name}");
        if (spriteRenderer == null) return;

        if (poisonFlashCoroutine != null)
            StopCoroutine(poisonFlashCoroutine);

        poisonFlashCoroutine = StartCoroutine(PoisonFlashCoroutine());
    }

    private IEnumerator PoisonFlashCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < poisonFlashDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / poisonFlashDuration;
            float curveValue = poisonFadeCurve.Evaluate(progress);

            spriteRenderer.color = Color.Lerp(originalColor, poisonColor, curveValue);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    public void PlayDamageBoostEffect()
    {
        if (damageBoostParticlePrefab == null) return;

        Vector3 spawnPos = transform.position + particleOffset;
        GameObject particleGO = Instantiate(damageBoostParticlePrefab, spawnPos, Quaternion.identity);
        Destroy(particleGO, 2f);
    }
}

using UnityEngine;
using System.Collections;

public class DamageEffectVisuals : MonoBehaviour
{
    [Header("Poison Effect")]
    [SerializeField] private Color poisonColor = Color.green;
    [SerializeField] private float poisonFlashDuration = 0.3f;
    [SerializeField] private Material poisonMaterial;

    [Header("Damage Boost Effect")]
    [SerializeField] private GameObject damageBoostParticlePrefab;
    [SerializeField] private Vector3 particleOffset = Vector3.zero;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Coroutine poisonFlashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
    }

    public void PlayPoisonEffect()
    {
        if (spriteRenderer == null) return;

        // Interrompi il flash precedente
        if (poisonFlashCoroutine != null)
        {
            StopCoroutine(poisonFlashCoroutine);
        }

        poisonFlashCoroutine = StartCoroutine(PoisonFlashCoroutine());
    }

    private IEnumerator PoisonFlashCoroutine()
    {
        // Flash verde
        spriteRenderer.color = poisonColor;

        yield return new WaitForSeconds(poisonFlashDuration);

        // Torna al colore originale
        spriteRenderer.color = Color.white;
    }

    public void PlayDamageBoostEffect()
    {
        if (damageBoostParticlePrefab == null) return;

        // Istanzia particella rossa on-hit
        Vector3 spawnPos = transform.position + particleOffset;
        GameObject particleGO = Instantiate(damageBoostParticlePrefab, spawnPos, Quaternion.identity);

        // Opzionale: distruggi la particella dopo un po'
        Destroy(particleGO, 2f);
    }
}

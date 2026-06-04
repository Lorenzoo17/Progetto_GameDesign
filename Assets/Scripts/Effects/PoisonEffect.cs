using UnityEngine;
using System.Collections;

public class PoisonEffect : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f; // Ogni quanti secondi applica danno

    private float poisonStack = 0f; // Valore attuale di veleno
    private IDamageable damageable;
    private GameObject poisonSource;
    private Coroutine poisonTickCoroutine;
    private DamageEffectVisuals damageEffectVisuals;

    private void Start()
    {
        damageable = GetComponent<IDamageable>();
        damageEffectVisuals = GetComponent<DamageEffectVisuals>();
    }

    public void AddPoison(GameObject source, float poisonAmount)
    {
        poisonSource = source;
        poisonStack += poisonAmount; // Accumula il veleno

        Debug.Log($"Poison stack: {poisonStack}");

        // Se il coroutine non è già avviato, avvialo
        if (poisonTickCoroutine == null)
        {
            poisonTickCoroutine = StartCoroutine(PoisonTickCoroutine());
        }
    }

    private IEnumerator PoisonTickCoroutine()
    {
        while (poisonStack > 0 && damageable != null)
        {
            yield return new WaitForSeconds(tickInterval);

            if (poisonStack <= 0) break;

            // Applica danno pari al valore di poison attuale
            DamageInfo poisonDamage = new DamageInfo(
                poisonStack,
                Vector2.zero,
                poisonSource,
                EntityType.Player
            );
            poisonDamage.AddEffect("PoisonTick");

            damageable.TakePoisonDamage(poisonDamage);

            // Visual effect ad ogni tick
            if (damageEffectVisuals != null)
            {
                damageEffectVisuals.PlayPoisonEffect();
            }

            // Decrementa il valore di poison
            poisonStack -= 1f;
            poisonStack = Mathf.Max(0, poisonStack); // Non scendere sotto 0

            Debug.Log($"Poison tick! Danno: {poisonStack + 1}, Stack rimanente: {poisonStack}");
        }

        // Veleno finito
        Destroy(this);
    }

    public float GetPoisonStack()
    {
        return poisonStack;
    }
}

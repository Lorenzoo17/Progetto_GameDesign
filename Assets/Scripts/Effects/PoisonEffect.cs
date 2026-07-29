using UnityEngine;
using System.Collections;

public class PoisonEffect : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f;

    private float poisonStack = 0f;
    private IDamageable damageable;
    private GameObject poisonSource;
    private Coroutine poisonTickCoroutine;
    private DamageEffectVisuals damageEffectVisuals;
    private bool isInitialized = false;

    // 🔥 USA AWAKE, NON START!
    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
        damageEffectVisuals = GetComponent<DamageEffectVisuals>();
        isInitialized = true;

        Debug.Log($"[PoisonEffect] Initialized on {gameObject.name}");
    }

    public void AddPoison(GameObject source, float poisonAmount)
    {
        // Assicurati che sia inizializzato
        if (!isInitialized)
        {
            Awake();
        }

        if (damageable == null)
        {
            Debug.LogError($"[PoisonEffect] damageable è NULL su {gameObject.name}!");
            return;
        }

        poisonSource = source;
        poisonStack += poisonAmount;

        Debug.Log($"[PoisonEffect] Added {poisonAmount} poison. Total stack: {poisonStack}");

        // Se il coroutine non è già avviato, avvialo
        if (poisonTickCoroutine == null)
        {
            Debug.Log($"[PoisonEffect] Starting poison tick coroutine");
            poisonTickCoroutine = StartCoroutine(PoisonTickCoroutine());
        }
        else
        {
            Debug.Log($"[PoisonEffect] Coroutine already running, just updated stack");
        }
    }

    private IEnumerator PoisonTickCoroutine()
    {
        Debug.Log($"[PoisonEffect] Poison tick coroutine started!");

        // Aspetta il primo tick
        yield return new WaitForSeconds(tickInterval);

        while (poisonStack > 0 && damageable != null)
        {
            if (poisonStack <= 0)
            {
                Debug.Log($"[PoisonEffect] Poison stack reached 0, exiting");
                break;
            }

            float damageThisTick = poisonStack;

            Debug.Log($"[PoisonEffect] Applying poison tick: {damageThisTick} damage");


            DamageInfo poisonDamage = new DamageInfo(
                0,  // Danno fisico = 0
                Vector2.zero,
                poisonSource,
                EntityType.Player
            );
            poisonDamage.Damage[DamageType.Poison] = damageThisTick;
            poisonDamage.addEffect("PoisonTick");

            damageable.TakePoisonDamage(poisonDamage);

            // Visual effect ad ogni tick
            if (damageEffectVisuals != null)
            {
                damageEffectVisuals.PlayPoisonEffect();
            }

            // Decrementa il valore di poison
            poisonStack -= 1f;
            poisonStack = Mathf.Max(0, poisonStack);

            Debug.Log($"[PoisonEffect] Poison stack after tick: {poisonStack}");

            // Aspetta il prossimo tick
            yield return new WaitForSeconds(tickInterval);
        }

        Debug.Log($"[PoisonEffect] Poison effect finished!");
        poisonTickCoroutine = null;
        Destroy(this);
    }

    public float GetPoisonStack()
    {
        return poisonStack;
    }
}

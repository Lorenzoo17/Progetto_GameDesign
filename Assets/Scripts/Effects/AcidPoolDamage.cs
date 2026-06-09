using UnityEngine;

public class AcidPoolDamage : MonoBehaviour
{
    [Header("Impostazioni Danno Pozza")]
    [SerializeField] private float poolDamage = 1f;
    [SerializeField] private float damageCooldown = 1f;

    private float timer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[POZZA] Qualcosa è entrato nella pozza: {other.gameObject.name}");

        if (other.TryGetComponent<Player>(out Player player))
        {
            //Debug.Log("[POZZA] Ho confermato che è il Player! Applico il primo danno.");
            DealDamageToPlayer(player);
            timer = damageCooldown;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                //Debug.Log("[POZZA] Il cooldown è finito, applico un nuovo tic di danno!");
                DealDamageToPlayer(player);
                timer = damageCooldown;
            }
        }
    }

    private void DealDamageToPlayer(Player player)
    {
        if (player.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            Vector2 attackDirection = (player.transform.position - transform.position).normalized;

            damageable.TakeDamage(new DamageInfo(
                poolDamage,
                attackDirection,
                this.gameObject,
                EntityType.Enemy
            ));

            //Debug.Log($"[POZZA] Danno di {poolDamage} inviato con successo a {player.gameObject.name}!");
        }
        else
        {
            Debug.LogWarning("[POZZA] Il Player non ha il componente IDamageable attaccato!");
        }
    }
}
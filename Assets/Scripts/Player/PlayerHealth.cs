using FirstGearGames.SmoothCameraShaker;
using System;
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    void TakePoisonDamage(DamageInfo damageInfo);
}

public class PlayerHealth : MonoBehaviour, IDamageable
{

    public event EventHandler OnHealthChanged;
    [SerializeField] private bool knockbackAfterTakingDamage;
    [SerializeField] private float knockbackForce;

    private Color initialColor; // usato per blink dopo take damage

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [SerializeField] public int maxHealthUnits = 6; // 3 hearts
    public int currentHealthUnits;

    private void Awake()
    {
        currentHealthUnits = maxHealthUnits;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();


        blinkAmount = (int)(invincibilityTime / (blinkAfterDamageRate * 2));

        initialColor = sr.color;
    }

    [SerializeField] private float invincibilityTime = 1;
    [SerializeField] private float blinkAfterDamageRate = 0.2f;
    private int blinkAmount;
    private bool invincible = false;
    [SerializeField] private bool forceInvincible; //this is to force the player to be invincible,
                                                   //useful for testing and for certain perks that grant invincibility
    private Coroutine blinkCoroutine;


    private void Update()
    {
        // sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (Player.Instance.playerMovement.IsDodging() || invincible || forceInvincible || IsDead()) return;

        UnityEngine.Debug.Log($"Player takes {damageInfo.Damage[DamageType.Physical]} damage from Enemy"); // Debug log per verificare i danni

        // Convert to half-hearts
        int damageUnits = Mathf.Max(1, Mathf.RoundToInt(damageInfo.Damage[DamageType.Physical] * 2));

        // PERK MODIFIER
        damageUnits = Player.Instance.perkController.ModifyIncomingDamage(damageUnits);

        currentHealthUnits -= damageUnits;
        currentHealthUnits = Mathf.Max(0, currentHealthUnits);

        if (knockbackAfterTakingDamage)
        {
            Player.Instance.playerMovement.ApplyKnockback(damageInfo.Direction, knockbackForce);
        }

        invincible = true;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(DamageBlink());

        CameraShakerHandler.Shake(Player.Instance.cameraShakeData);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.PlayerHit, .25f);
        }

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        //controllo morte del player
        if (IsDead())
        {
            Die();
        }
    }


    private IEnumerator DamageBlink()
    {
        for (int i = 0; i < blinkAmount; i++)
        {
            sr.color = Color.white * 3f;

            yield return new WaitForSeconds(blinkAfterDamageRate);

            sr.color = initialColor;

            yield return new WaitForSeconds(blinkAfterDamageRate);
        }
        sr.color = initialColor; // per sicurezza
        invincible = false;
    }

    public void Heal(int units)
    {
        currentHealthUnits += units;
        currentHealthUnits = Mathf.Min(currentHealthUnits, maxHealthUnits);
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealthUnits / maxHealthUnits;
    }

    public void IncreaseHealth(int units)
    {
        maxHealthUnits += units;
        Heal(units); // opzionale, dipende se si vuole che l'aumento di salute massima curi anche quella attuale
    }

    public void DecreaseHealth(int units)
    {
        maxHealthUnits = Mathf.Max(1, maxHealthUnits - units); // assicurati di non scendere sotto 1
        currentHealthUnits = Mathf.Min(currentHealthUnits, maxHealthUnits); // se la salute attuale è maggiore della nuova massima, riducila
    }

    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        // Implementation for taking poison damage
    }

    public int GetCurrentHealthUnits()
    {
        return currentHealthUnits;
    }

    public bool IsDead()
    {
        return currentHealthUnits <= 0;
    }

    private void Die()
    {
        Debug.Log("Che sega, sei morto!");

        // Disabilitiamo le collisioni per evitare ulteriori danni o interazioni
        GetComponent<Collider2D>().enabled = false;

        // Notifichiamo il controller principale del Player
        Player.Instance.OnPlayerDeath();
    }

    public void SetInvincible(bool value)
    {
        forceInvincible = value;
    }
}


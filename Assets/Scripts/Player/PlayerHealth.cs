using FirstGearGames.SmoothCameraShaker;
using System;
using System.Collections;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(DamageInfo damageInfo);
}

public class PlayerHealth : MonoBehaviour, IDamageable {

    public event EventHandler OnHealthChanged;
    [SerializeField] private bool knockbackAfterTakingDamage;
    [SerializeField] private float knockbackForce;

    private Color initialColor; // usato per blink dopo take damage

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [SerializeField] private int maxHealthUnits = 6; // 3 hearts
    private int currentHealthUnits;

    private void Awake() {
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
    private Coroutine blinkCoroutine;


    private void Update() {
        // sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime);
    }

    public void TakeDamage(DamageInfo damageInfo) {
        if (Player.Instance.playerMovement.IsDodging() || invincible) return;

        // Convert to half-hearts
        int damageUnits = Mathf.Max(1, Mathf.RoundToInt(damageInfo.Damage * 2));

        // PERK MODIFIER
        damageUnits = Player.Instance.perkController.ModifyIncomingDamage(damageUnits);

        currentHealthUnits -= damageUnits;
        currentHealthUnits = Mathf.Max(0, currentHealthUnits);

        if (knockbackAfterTakingDamage) {
            Player.Instance.playerMovement.ApplyKnockback(damageInfo.Direction, knockbackForce);
        }

        invincible = true;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(DamageBlink());

        CameraShakerHandler.Shake(Player.Instance.cameraShakeData);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }


    private IEnumerator DamageBlink() {
        for(int i = 0; i < blinkAmount; i++) {
            sr.color = Color.white * 3f;

            yield return new WaitForSeconds(blinkAfterDamageRate);

            sr.color = initialColor;

            yield return new WaitForSeconds(blinkAfterDamageRate);
        }
        sr.color = initialColor; // per sicurezza
        invincible = false;
    }

    public void Heal(int units) {
        currentHealthUnits += units;
        currentHealthUnits = Mathf.Min(currentHealthUnits, maxHealthUnits);
    }

    public float GetHealthPercentage() {
        return (float)currentHealthUnits / maxHealthUnits;
    }

    public void IncreaseHealth(int units) {
        maxHealthUnits += units;
        Heal(units); // opzionale, dipende se si vuole che l'aumento di salute massima curi anche quella attuale
    }

    public void DecreaseHealth(int units) {
        maxHealthUnits = Mathf.Max(1, maxHealthUnits - units); // assicurati di non scendere sotto 1
        currentHealthUnits = Mathf.Min(currentHealthUnits, maxHealthUnits); // se la salute attuale è maggiore della nuova massima, riducila
    }
}

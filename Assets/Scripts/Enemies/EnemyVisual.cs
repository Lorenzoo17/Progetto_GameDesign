using UnityEngine;

public class EnemyVisual : MonoBehaviour {

    private Animator anim;
    private SpriteRenderer sr;
    private Transform flipRoot;

    [Header("Damage Blink")]
    [SerializeField] private Color blinkAfterDamageTargetColor = Color.red;
    [SerializeField] private float blinkIntensity = 3f;
    [SerializeField] private float blinkAfterDamageTime = 10f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectSpawnPositionOffset = 0.5f;
    [SerializeField] private float hitEffectRotationOffset = -90f;

    [Header("Flip")]
    [SerializeField] private bool autoFlipTowardsPlayer = true;
    [SerializeField] private bool invertFlipDirection;

    // Hash invece di stringhe: piu' veloce e gli errori di battitura si trovano in un solo posto
    private static readonly int MovingHash = Animator.StringToHash("Moving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    private Color initialColor;

    // Servono a Enemy per configurare il dead body
    public Sprite CurrentSprite => sr != null ? sr.sprite : null;
    public string SortingLayerName => sr != null ? sr.sortingLayerName : "Default";
    public int SortingOrder => sr != null ? sr.sortingOrder : 0;

    private void Awake() {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (sr == null) {
            sr = GetComponent<SpriteRenderer>();

            if (sr == null) {
                Transform visual = transform.Find("Visual");
                if (visual != null)
                    sr = visual.GetComponent<SpriteRenderer>();
            }
        }

        if (sr == null)
            Debug.LogWarning($"[EnemyVisual] Nessuno SpriteRenderer trovato su {name}", this);
        else
            initialColor = sr.color;

        if (flipRoot == null)
            flipRoot = transform;
    }

    private void Update() {
        UpdateBlink();

        if (autoFlipTowardsPlayer && Player.Instance != null)
            FlipTowards(Player.Instance.transform.position);
    }

    // ---------- ANIMAZIONI ----------

    public void SetMoving(bool value) {
        if (anim != null)
            anim.SetBool(MovingHash, value);
    }

    public void PlayAttack() {
        if (anim != null)
            anim.SetTrigger(AttackHash);
    }

    public void PlayHurt() {
        if (anim != null)
            anim.SetTrigger(HurtHash);
    }

    public void PlayDeath() {
        if (anim != null)
            anim.SetTrigger(DeathHash);
    }

    // ---------- FEEDBACK DANNO ----------

    // Feedback completo del colpo ricevuto: animazione + blink + particella.
    public void PlayHitFeedback(Vector2 attackDirection) {
        PlayHurt();
        Blink();
        SpawnHitEffect(attackDirection);
    }

    public void Blink() {
        if (sr != null)
            sr.color = blinkAfterDamageTargetColor * blinkIntensity;
    }

    public void SpawnHitEffect(Vector2 attackDirection) {
        if (hitEffect == null)
            return;

        Vector2 spawnPos = (Vector2)transform.position +
                           attackDirection.normalized * hitEffectSpawnPositionOffset;

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        GameObject effect = Instantiate(hitEffect, spawnPos, Quaternion.identity);
        effect.transform.rotation = Quaternion.Euler(0f, 0f, angle + hitEffectRotationOffset);
    }

    private void UpdateBlink() {
        if (sr == null || sr.color == initialColor)
            return;

        sr.color = Color.Lerp(sr.color, initialColor, blinkAfterDamageTime * Time.deltaTime);
    }

    // ---------- FLIP ----------

    public void SetAutoFlip(bool value) {
        autoFlipTowardsPlayer = value;
    }

    public void FlipTowards(Vector3 worldPosition) {
        if (flipRoot == null)
            return;

        Vector3 scale = flipRoot.localScale;
        int flipDirection = invertFlipDirection ? -1 : 1;

        if (worldPosition.x > flipRoot.position.x)
            scale.x = -Mathf.Abs(scale.x) * flipDirection;
        else
            scale.x = Mathf.Abs(scale.x) * flipDirection;

        flipRoot.localScale = scale;
    }
}
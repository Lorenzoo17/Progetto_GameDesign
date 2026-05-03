using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Stain : MonoBehaviour {

    [SerializeField] private float damagePerTick = 1f;
    [SerializeField] private float tickRate = 0.5f;

    private GameObject owner;
    private EntityType ownerType;

    [SerializeField] private Color pulseColor;
    [SerializeField] private float pulseSpeed = 3f;

    private float tickTimer;
    private readonly List<IDamageable> entitiesInside = new();

    private SpriteRenderer sr;
    private Color initialColor;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;
    }

    private void Update() {
        HandleDamage();
        HandleVisual();
    }

    public void SetUpStain(GameObject owner, float size = 1f) {
        this.owner = owner;

        EntityOwner combatOwner = owner.GetComponent<EntityOwner>();
        ownerType = combatOwner != null ? combatOwner.GetEntityType : EntityType.Neutral;

        transform.localScale *= size;
    }

    private void HandleDamage() {
        tickTimer += Time.deltaTime;

        if (tickTimer < tickRate) return;
        tickTimer = 0f;

        for (int i = entitiesInside.Count - 1; i >= 0; i--) {
            IDamageable entity = entitiesInside[i];

            if (entity == null) {
                entitiesInside.RemoveAt(i);
                continue;
            }

            if (entity is not MonoBehaviour mb) continue;

            GameObject target = mb.gameObject;

            if (!CombatUtility.CanDamage(owner, target)) continue;

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;

            DamageInfo damageInfo = new DamageInfo(
                damagePerTick,
                direction,
                owner,
                ownerType
            );

            entity.TakeDamage(damageInfo);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            if (!entitiesInside.Contains(damageable)) {
                entitiesInside.Add(damageable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.TryGetComponent<IDamageable>(out IDamageable damageable)) {
            entitiesInside.Remove(damageable);
        }
    }

    private void HandleVisual() {
        if (sr == null) return;

        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        sr.color = Color.Lerp(initialColor, pulseColor, t);
    }
}

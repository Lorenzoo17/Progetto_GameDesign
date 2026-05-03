using System.Collections.Generic;
using UnityEngine;

public class Stain : MonoBehaviour {

    [SerializeField] private float damagePerTick = 1f;
    [SerializeField] private float tickRate = 0.5f;
    private float tickTimer;
    private List<IDamageable> entitiesInside = new();

    private SpriteRenderer sr;
    private Color initialColor;
    [SerializeField] private bool isFromPlayer;
    [SerializeField] private Color pulseColor;
    [SerializeField] private float pulseSpeed = 3f;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;
    }

    private void Update() {
        HandleDamage();
        HandleVisual();
    }

    private void HandleDamage() {
        tickTimer += Time.deltaTime;

        if (tickTimer < tickRate) return;
        tickTimer = 0f;

        foreach (var entity in entitiesInside) {
            if (entity == null) continue;

            if (entity is MonoBehaviour mb) {
                GameObject go = mb.gameObject;

                if (Player.Instance == null) continue;

                if (isFromPlayer) {
                    if (go != Player.Instance.gameObject) {
                        entity.TakeDamage(damagePerTick);
                    }
                }
                else {
                    if (go == Player.Instance.gameObject) {
                        entity.TakeDamage(damagePerTick);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent<IDamageable>(out var damageable)) {
            if (!entitiesInside.Contains(damageable)) {
                entitiesInside.Add(damageable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.TryGetComponent<IDamageable>(out var damageable)) {
            entitiesInside.Remove(damageable);
        }
    }

    private void HandleVisual() {
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);

        Color finalColor = Color.Lerp(initialColor, pulseColor, t);
        sr.color = finalColor;
    }

    public void SetUpStain(float size = 1f, bool isPlayer = false) {
        transform.localScale *= size;
        isFromPlayer = isPlayer;
    }
}

using UnityEngine;

public enum StatType {
    Attack,
    AttackRate,
    Speed,
    DodgeCoolDown
}

public enum ModifierType {
    Flat,
    Percent
}

[System.Serializable]
public class CharacterStats {
    // eventualmente si aggiungono altre statistiche per il player
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float attack;
    [SerializeField] private float attackRate;
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float currentMoveSpeed;
    [SerializeField] private float dodgeCooldown;

    public CharacterStats(float maxHealth, float attack, float attackRate, float moveSpeed, float dodgeCooldown) {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        this.attack = attack;
        this.attackRate = attackRate;
        this.baseMoveSpeed = moveSpeed;
        this.currentMoveSpeed = moveSpeed;
        this.dodgeCooldown = dodgeCooldown;
    }

    public CharacterStats(CharacterStats stats) {
        this.maxHealth = stats.maxHealth;
        this.currentHealth = stats.maxHealth;
        this.attack = stats.attack;
        this.attackRate = stats.attackRate;
        this.baseMoveSpeed = stats.baseMoveSpeed;
        this.currentMoveSpeed = stats.baseMoveSpeed;
        this.dodgeCooldown = stats.dodgeCooldown;
    }

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetAttack() => attack;
    public float GetAttackRate() => attackRate;
    public float GetMoveSpeed() => currentMoveSpeed;
    public float GetBaseMoveSpeed() => baseMoveSpeed;
    public float GetDodgeCooldown() => dodgeCooldown;

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);
    }

    public void Heal(float amount) {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public bool IsDead() {
        return currentHealth <= 0f;
    }

    public void SetMaxHealth(float value) {
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    // Modifiche addittive alle statistiche
    public void AddAttack(float value) => attack += value;
    public void AddAttackRate(float value) => attackRate += value;
    public void AddBaseMoveSpeed(float value) {
        baseMoveSpeed += value;
        RecalculateMoveSpeed();
    }
    public void AddMoveSpeed(float value) {
        currentMoveSpeed += value;
    }
    public void AddDodgeCooldown(float value) => dodgeCooldown += value;

    // Modifiche moltiplicative alle statistiche

    public void MultiplyAttack(float multiplier) => attack *= multiplier;
    public void MultiplyAttackRate(float multiplier) => attackRate *= multiplier;
    public void MultiplyBaseMoveSpeed(float multiplier) {
        baseMoveSpeed *= multiplier;
        RecalculateMoveSpeed();
    }
    public void MultiplyMoveSpeed(float multiplier) {
        currentMoveSpeed *= multiplier;
    }
    public void MultiplyDodgeCooldown(float multiplier) => dodgeCooldown *= multiplier;

    public void RecalculateMoveSpeed() {
        currentMoveSpeed = baseMoveSpeed;
    }
    public void ResetHealth() {
        currentHealth = maxHealth;
    }
}

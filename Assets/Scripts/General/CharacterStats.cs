using System;
using UnityEngine;

public enum StatType
{
    Attack,
    AttackRate,
    Speed,
    DodgeCoolDown,
    Poison
}

public enum ModifierType
{
    Flat,
    Percent
}

[System.Serializable]
public class CharacterStats
{
    // eventualmente si aggiungono altre statistiche per il player
    [SerializeField] private float attack;
    [SerializeField] private float attackRate;
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float currentMoveSpeed;
    [SerializeField] private float dodgeCooldown;
    [SerializeField] private float mutagenPower;

    public CharacterStats(float attack, float attackRate, float moveSpeed, float dodgeCooldown, float mutagenPower)
    {
        this.attack = attack;
        this.attackRate = attackRate;
        this.baseMoveSpeed = moveSpeed;
        this.currentMoveSpeed = moveSpeed;
        this.dodgeCooldown = dodgeCooldown;
        this.mutagenPower = mutagenPower;
    }

    public CharacterStats(CharacterStats stats)
    {
        this.attack = stats.attack;
        this.attackRate = stats.attackRate;
        this.baseMoveSpeed = stats.baseMoveSpeed;
        this.currentMoveSpeed = stats.baseMoveSpeed;
        this.dodgeCooldown = stats.dodgeCooldown;
        this.mutagenPower = stats.mutagenPower;
    }
    public float GetAttack() => attack;
    public float GetAttackRate() => attackRate;
    public float GetMoveSpeed() => currentMoveSpeed;
    public float GetBaseMoveSpeed() => baseMoveSpeed;
    public float GetDodgeCooldown() => dodgeCooldown;
    public float GetMutagenPower() => mutagenPower;

    // Modifiche addittive alle statistiche
    public void AddAttack(float value) => attack += value;
    public void AddAttackRate(float value) => attackRate += value;
    public void AddBaseMoveSpeed(float value)
    {
        baseMoveSpeed += value;
        RecalculateMoveSpeed();
    }
    public void AddMoveSpeed(float value)
    {
        currentMoveSpeed += value;
    }
    public void AddDodgeCooldown(float value) => dodgeCooldown += value;
    public void AddMutagenPower(float value) => mutagenPower += value;

    // Modifiche moltiplicative alle statistiche

    public void MultiplyAttack(float multiplier) => attack *= multiplier;
    public void MultiplyAttackRate(float multiplier) => attackRate *= multiplier;
    public void MultiplyBaseMoveSpeed(float multiplier)
    {
        baseMoveSpeed *= multiplier;
        RecalculateMoveSpeed();
    }
    public void MultiplyMoveSpeed(float multiplier) => currentMoveSpeed *= multiplier;
    
    public void MultiplyDodgeCooldown(float multiplier) => dodgeCooldown *= multiplier;
    public void MultiplyMutagenPower(float multiplier) => mutagenPower *= multiplier;
    // HELPER per ricalcolare la velocità di movimento
    public void RecalculateMoveSpeed()
    {
        currentMoveSpeed = baseMoveSpeed;
    }
}

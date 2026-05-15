using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    public event EventHandler OnManaChanged;

    [SerializeField] private int baseMaxMana = 3;
    private int maxMana;

    private int currentMana;

    private void Awake()
    {
        maxMana = baseMaxMana;
    }

    public void InitializeMana()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;

        currentMana = Mathf.Max(0, currentMana);

        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;

        currentMana = Mathf.Min(currentMana, maxMana);

        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }
    public void IncreaseMaxMana(int amount)
    {
        maxMana += amount;

        currentMana = Mathf.Min(currentMana + amount, maxMana);

        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }
    public void DecreaseMaxMana(int amount)
    {
        maxMana = Mathf.Max(0, maxMana - amount);

        currentMana = Mathf.Min(currentMana, maxMana);

        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }
}

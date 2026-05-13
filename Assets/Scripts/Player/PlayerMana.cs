using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    public event EventHandler OnManaChanged;

    [SerializeField] private int maxMana = 3;

    private int currentMana;

    private void Awake()
    {
        currentMana = maxMana;
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
}

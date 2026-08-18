using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct StatusMultiplier
{
    public StatusEffectType effectType;
    public float multiplier; // 1 = Normale, 0.5 = Dimezzato, 0 = Immune
}

public class EnemyStatus : MonoBehaviour
{
    // Aggiungi resistenze e debolezze da Inspector per ogni prefabbricato nemico
    [SerializeField] private List<StatusMultiplier> statusMultipliers = new List<StatusMultiplier>();

    private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

    public bool HasEffect(StatusEffectType type)
    {
        return activeEffects.Exists(e => e.data.effectType == type);
    }

    // Metodo helper per trovare il moltiplicatore corretto
    public float GetMultiplierForType(StatusEffectType type)
    {
        foreach (var sm in statusMultipliers)
        {
            if (sm.effectType == type)
            {
                return sm.multiplier;
            }
        }
        // Se non specificato nell'Inspector, il valore di base è 100% (1f)
        return 1f;
    }

        public void ApplyEffect(StatusEffectData newEffectData, int statusValue = 0)
    {
        // Calcoliamo il moltiplicatore PRIMA di applicare l'effetto
        float multiplier = GetMultiplierForType(newEffectData.effectType);
 
        // Se il moltiplicatore è 0 o meno, il nemico è immune. Interrompiamo tutto.
        if (multiplier <= 0f) return;
 
        ActiveStatusEffect existingEffect = activeEffects.Find(e => e.data == newEffectData);
 
        if (existingEffect != null)
        {
            // Stack: passiamo il moltiplicatore e il nuovo valore
            existingEffect.data.OnStack(gameObject, existingEffect, multiplier, statusValue);
            return;
        }
 
        // Calcoliamo la durata modificata
        float finalDuration = newEffectData.GetModifiedDuration(multiplier);
 
        // Creiamo un nuovo effetto con il valore iniziale
        ActiveStatusEffect newEffect = new ActiveStatusEffect(newEffectData, finalDuration);
        activeEffects.Add(newEffect);
 
        newEffect.data.OnApply(gameObject);
    }


    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveStatusEffect effect = activeEffects[i];

            // Troviamo il moltiplicatore per passarlo all'OnTick 
            // (Così il veleno può usarlo per scalare i danni al posto della durata)
            float multiplier = GetMultiplierForType(effect.data.effectType);

            effect.data.OnTick(gameObject, effect, multiplier);

            effect.remainingDuration -= Time.deltaTime;

            if (effect.remainingDuration <= 0f)
            {
                effect.data.OnRemove(gameObject);
                activeEffects.RemoveAt(i);
            }
        }
    }
}

[Serializable]
public class ActiveStatusEffect
{
    public StatusEffectData data;
    public float remainingDuration;
    public int currentStacks;

    // Costruttore aggiornato per ricevere la durata finale calcolata
    public ActiveStatusEffect(StatusEffectData effectData, float calculatedDuration)
    {
        this.data = effectData;
        this.remainingDuration = calculatedDuration;
        this.currentStacks = 1;
    }
}


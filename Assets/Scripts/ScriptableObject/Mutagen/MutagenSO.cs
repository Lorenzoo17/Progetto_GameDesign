using UnityEngine;

public enum MutagenBodyPart { Head, Body, Paws }

public abstract class MutagenSO : ScriptableObject
{
    [Header("Base Info")]
    public string mutagenName;
    [TextArea] public string description;
    public Sprite icon;
    public MutagenBodyPart bodyPart;

    [Header("Cost")]
    public int manaCost = 1;

    [Header("Behaviour")]
    public bool isToggle;
    public float duration;
    public GameObject animationEffect;

    public MutagenLootData mutagenLootData; // usato in treasureRoomSpawner, per capire se il player ha gia'
    // il mutagene corrispondente equipaggiato

    public abstract bool Activate(Player player, MutagenInstance instance);

    public abstract void Tick(Player player, MutagenInstance instance, float deltaTime);

    public abstract void Deactivate(Player player, MutagenInstance instance);

    public virtual float GetEnemyEffectDuration(Enemy enemy)
    {
        return duration;
    }
}


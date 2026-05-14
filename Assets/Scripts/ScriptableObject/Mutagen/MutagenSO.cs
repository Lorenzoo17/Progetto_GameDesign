using UnityEngine;

public abstract class MutagenSO : ScriptableObject
{
    [Header("Base Info")]
    public string mutagenName;
    [TextArea] public string description;

    [Header("Cost")]
    public int manaCost = 1;

    [Header("Behaviour")]
    public bool isToggle;
    public float duration;
    public GameObject animationEffect;

    public abstract bool Activate(Player player, MutagenInstance instance);

    public abstract void Tick(Player player, MutagenInstance instance, float deltaTime);

    public abstract void Deactivate(Player player, MutagenInstance instance);
}


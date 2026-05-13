using System.Collections.Generic;

[System.Serializable]
public class MutagenInstance
{
    public MutagenSO source;

    public float remainingDuration;

    public Dictionary<string, float> runtimeFloats = new();

    public bool IsExpired => remainingDuration <= 0f;

    public MutagenInstance(MutagenSO source)
    {
        this.source = source;
        remainingDuration = source.duration;
    }

    public void UpdateTime(float deltaTime)
    {
        remainingDuration -= deltaTime;
    }
}

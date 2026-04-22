using UnityEngine;

public enum VisualEffectType {
    Walk,
    Attack,
    Hit,
    Die
}

[System.Serializable]
public struct VisualEffect {
    public VisualEffectType groupType;
    public GameObject[] visualEffectPrefabs;
}

public class EffectManager : MonoBehaviour {

    public static EffectManager Instance { get; private set; }
    [SerializeField] private VisualEffect[] visualEffects;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public GameObject SpawnVisualEffect(VisualEffectType effectType, Vector3 position, Quaternion rotation) {
        GameObject visualEffectChosen = GetVisualEffectFromName(effectType);
        if (visualEffectChosen == null) return null;

        return Instantiate(visualEffectChosen, position, rotation);
    }

    private GameObject GetVisualEffectFromName(VisualEffectType effectType) {
        foreach (VisualEffect ve in visualEffects) {
            if (ve.groupType == effectType) {
                return ve.visualEffectPrefabs[Random.Range(0, ve.visualEffectPrefabs.Length)];
            }
        }
        return null;
    }
}

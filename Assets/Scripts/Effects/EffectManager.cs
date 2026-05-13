using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public enum VisualEffectType {
    Walk,
    Attack,
    Hit,
    Die
}

public enum ShakeDataType {
    Explosion,
    RangedAttack,
    MeleeAttack,
    SmashAttack
}

[System.Serializable]
public struct VisualEffect {
    public VisualEffectType groupType;
    public GameObject[] visualEffectPrefabs;
}

[System.Serializable]
public struct ShakeCollection {
    public ShakeDataType groupType;
    public ShakeData[] shakeDatas;
}

public class EffectManager : MonoBehaviour {

    public static EffectManager Instance { get; private set; }
    [SerializeField] private VisualEffect[] visualEffects;

    [SerializeField] private ShakeCollection[] shakeCollection;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public ShakeData GetShakeDataByType(ShakeDataType type) {
        foreach (var shakeElement in shakeCollection) {
            if(shakeElement.groupType == type) {
                return shakeElement.shakeDatas[Random.Range(0, shakeElement.shakeDatas.Length)];
            }
        }
        return null;
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

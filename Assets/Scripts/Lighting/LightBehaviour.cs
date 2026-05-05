using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightBehaviour : MonoBehaviour {
    [SerializeField] private float minIntensity = 2.5f;
    [SerializeField] private float maxIntensity = 3.5f;
    [SerializeField] private float flickerSpeed = 4f;

    private Light2D lightComponent;
    private float randomOffset;

    private void Awake() {
        lightComponent = GetComponent<Light2D>();
        randomOffset = Random.Range(0f, 1000f);
    }

    private void Update() {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
        lightComponent.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}

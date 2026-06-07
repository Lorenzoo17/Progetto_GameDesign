using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private GameObject destroyEffect;

    private void Start() {
        Invoke("Destruction", delay);
    }

    private void Destruction() {
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}

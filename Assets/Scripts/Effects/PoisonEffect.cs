using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    private float damagePerSecond = 2f;
    private float duration = 5f;
    private float timer = 0f;

    private HealthSystem healthSystem;

    private void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
            healthSystem.TakeDamage(new DamageInfo(damagePerSecond * Time.deltaTime, Vector2.zero, Player.Instance.gameObject, EntityType.Player));
        }
        else
        {
            Destroy(this);
        }
    }
}
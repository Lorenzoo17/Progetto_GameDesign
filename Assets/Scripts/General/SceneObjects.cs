using UnityEngine;

public class SceneObjects : MonoBehaviour, IDamageable
{
    private Animator anim;
    [SerializeField] private float itemDropChance = 0.3f;
    [SerializeField] private bool spawnOnStart;
    [SerializeField] private Transform spawnTransform;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        if(spawnTransform == null) {
            spawnTransform = transform;
        }
    }

    private void Start() {
        if (spawnOnStart) {
            if (SpawnItems.Instance != null) {
                SpawnItems.Instance.SpawnItem(spawnTransform.position, itemDropChance);
            }
        }
    }
    public void TakeDamage(DamageInfo damageInfo) {
        if (spawnOnStart) return;

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.WoodCrack, .10f);
        }

        if (SpawnItems.Instance != null) {
            SpawnItems.Instance.SpawnItem(transform.position, itemDropChance);
        }

        if (anim != null) {
            DestroyAnimation();
            return;
        }

        Destroy(gameObject); 
    }

    private void DestroyAnimation()
    {
        anim.SetTrigger("Destroy");
    }

    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        // Non applicabile a questo oggetto
    }

}

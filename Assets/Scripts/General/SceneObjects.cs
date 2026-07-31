using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class SceneObjects : MonoBehaviour, IDamageable
{
    private Animator anim;
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
                SpawnItems.Instance.SpawnItem(spawnTransform.position, gameObject);
            }
        }
    }
    public void TakeDamage(DamageInfo damageInfo) {
        if (spawnOnStart) return;

        // screen shake quanto entita' prende danno (tolto da playerAttack)
        if (EffectManager.Instance != null) {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));
        }

        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.WoodCrack, .10f);
        }

        if (SpawnItems.Instance != null) {
            SpawnItems.Instance.SpawnItem(transform.position, gameObject);
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

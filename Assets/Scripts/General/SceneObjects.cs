using UnityEngine;

public class SceneObjects : MonoBehaviour, IDamageable
{
    private Animator anim;
    [SerializeField] private float itemDropChance = 0.2f;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void TakeDamage(DamageInfo damageInfo) {
        if (SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.WoodCrack, .15f);
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

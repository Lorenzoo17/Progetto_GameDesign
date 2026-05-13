using UnityEngine;

public class SceneObjects : MonoBehaviour, IDamageable
{
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (anim == null) return;

        DestroyAnimation();
    }

    private void DestroyAnimation()
    {
        anim.SetTrigger("Destroy");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.WoodCrack, .15f);
        }
    }

    public void TakePoisonDamage(DamageInfo damageInfo)
    {
        // Non applicabile a questo oggetto
    }

}

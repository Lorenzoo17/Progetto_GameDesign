using UnityEngine;

public class SceneObjects : MonoBehaviour, IDamageable {
    private Animator anim;
    private void Awake() {
        anim = GetComponent<Animator>();
    }
    public void TakeDamage(DamageInfo damageInfo) {
        if (anim == null) return;

        DestroyAnimation();
    }

    private void DestroyAnimation() {
        anim.SetTrigger("Destroy");
    }

}

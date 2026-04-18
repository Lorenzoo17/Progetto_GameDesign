using UnityEngine;

public interface IWeapon {
    void Attack(Vector2 attackDirection);
}

public class Weapon : MonoBehaviour, IWeapon{

    public enum WeaponType {
        Melee,
        Ranged
    }

    [SerializeField] private WeaponType weaponType;
    private Animator anim;
    private void Awake() {
        anim = GetComponent<Animator>();
    }   

    public void Attack(Vector2 attackDirection) {
        if(weaponType == WeaponType.Melee) {
            // overlapCircle
        }else {
            // spawn bullet
        }

        if (anim != null) {
            anim.SetTrigger("Attack"); // animazione di attacco dell'arma
        }
    }
}

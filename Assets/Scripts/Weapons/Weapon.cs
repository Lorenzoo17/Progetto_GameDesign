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

    public WeaponType GetWeaponType => weaponType;

    public void Attack(Vector2 attackDirection) {
        if(weaponType == WeaponType.Melee) {
            // overlapCircle
        }else {
            // spawn bullet
        }
    }
}

using UnityEngine;

public class WeaponRanged : Weapon
{
    [SerializeField] private float weaponRotationOffsetZ;
    private SpriteRenderer sr;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();    
    }

    public override void Attack(Vector2 dir) {
        // gestione attacco con spawn di proiettili
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir) {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion weaponHolderRotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
        weaponHolder.rotation = weaponHolderRotation;
    }
}

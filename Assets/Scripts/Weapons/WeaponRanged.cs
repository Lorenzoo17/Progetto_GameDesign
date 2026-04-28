using UnityEngine;

public class WeaponRanged : Weapon
{
    [SerializeField] private float weaponRotationOffsetZ;
    private SpriteRenderer sr;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float weaponBaseDamage;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();    
        if(attackPoint == null) {
            attackPoint = transform;
        }
    }

    public override void Attack(Vector2 dir) {
        // gestione attacco con spawn di proiettili
        if(bulletPrefab == null) {
            Debug.Log($"Bullet non assegnato all'arma {gameObject.name}");
            return; 
        }

        GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
        if(bullet.TryGetComponent<BulletBehaviour>(out BulletBehaviour b)) {
            b.SetBullet(dir, bulletSpeed, weaponBaseDamage);
        }
    }

    public override void HandleRotation(Transform weaponHolder, Vector2 dir) {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion weaponHolderRotation = Quaternion.Euler(0, 0, angle + weaponRotationOffsetZ);
        weaponHolder.rotation = weaponHolderRotation;
    }
}

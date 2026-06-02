using UnityEngine;

public class ProjectileShooterBoss : ProjectileShooter
{
    [Header("Impostazioni Specifiche Boss")]
    [SerializeField] private GameObject pozzaPrefab;
    [SerializeField] private GameObject impactPrefab;

  
    [SerializeField] private float minProjectileLifeTime = 0.4f;
    [SerializeField] private float projectileLifeTime = 0.8f; 
    [SerializeField] private float poolDuration = 3f;

    public void ShootBossProjectile(GameObject owner, Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[SHOOTER BOSS] Manca il prefab del proiettile!");
            return;
        }


        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion launchRotation = Quaternion.Euler(0f, 0f, angle);


        GameObject proj = Instantiate(projectilePrefab, spawnPos, launchRotation);


        if (proj.TryGetComponent<BossAcidProjectile>(out BossAcidProjectile projScript))
        {
            BossCtrl boss = owner.GetComponent<BossCtrl>();

            
            float randomLifeTime = Random.Range(minProjectileLifeTime, projectileLifeTime);

            projScript.InitializeBossProjectile(
                owner, direction, projectileSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, randomLifeTime, boss
            );
        }
        else
        {
            Debug.LogError("Il prefab del proiettile non ha lo script BossAcidProjectile!");
        }
    }
}
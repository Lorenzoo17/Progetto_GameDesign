using UnityEngine;

public class ProjectileShooterBoss : ProjectileShooter
{
    [Header("Impostazioni Specifiche Boss")]
    [SerializeField] private GameObject pozzaPrefab;
    [SerializeField] private GameObject impactPrefab;

    [Header("Distanza Attacco Normale (Ranged State)")]
    [SerializeField] private float minNormalProjectileRange = 4f;
   

    [SerializeField] private float minProjectileLifeTime = 0.4f;
    [SerializeField] private float projectileLifeTime = 0.8f;
    [SerializeField] private float poolDuration = 3f;

    // ATTACCO A DISTANZA NORMALE
    public void ShootBossProjectile(GameObject owner, Vector2 direction, float maxRange)
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

            // Calcola una distanza casuale e la converte in tempo di volo (Lifetime = Spazio / Velocità)
            float randomDistance = Random.Range(minNormalProjectileRange, maxRange);
            randomDistance = Mathf.Ceil(randomDistance);
            //Debug.Log($"[SHOOTER BOSS] Distanza casuale per il proiettile: {randomDistance}");
            float customLifeTime = randomDistance / projectileSpeed;

            projScript.InitializeBossProjectile(
                owner, direction, projectileSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, customLifeTime, boss
            );
        }
        else
        {
            Debug.LogError("Il prefab del proiettile non ha lo script BossAcidProjectile!");
        }
    }       

    // ATTACCO TUBI
    public void ShootPipeProjectile(GameObject owner, Vector2 direction, float customSpeed, Vector3 spawnPos)
    {
        if (projectilePrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion launchRotation = Quaternion.Euler(0f, 0f, angle);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, launchRotation);

        if (proj.TryGetComponent<BossAcidProjectile>(out BossAcidProjectile projScript))
        {
            BossCtrl boss = owner.GetComponent<BossCtrl>();
            float randomLifeTime = Random.Range(minProjectileLifeTime, projectileLifeTime);

            projScript.InitializeBossProjectile(
                owner, direction, customSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, randomLifeTime, boss
            );

            projScript.EnablePipeMode();
        }
    }
}
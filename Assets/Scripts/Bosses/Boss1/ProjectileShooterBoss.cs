using UnityEngine;

public class ProjectileShooterBoss : ProjectileShooter
{
    [Header("Impostazioni Specifiche Boss")]
    [SerializeField] private GameObject pozzaPrefab;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float poolDuration = 3f;

    [Header("Attacco Normale (Ranged State)")]
    [SerializeField] private float damage = 8f;
    [SerializeField] private float normalProjectileSpeed = 8f;
    [SerializeField] private float minNormalProjectileRange = 10f;
    [SerializeField] private float maxNormalProjectileRange = 15f;

    [Header("Attacco Tubi (Special State)")]
    [SerializeField] private float pipeProjectileSpeed = 15f;
    [SerializeField] private float pipeProjectileRange = 25f;

    // ATTACCO A DISTANZA NORMALE
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

            // Sceglie una distanza esatta che il proiettile dovrà percorrere
            float targetDistance = Mathf.Ceil(Random.Range(minNormalProjectileRange, maxNormalProjectileRange));
            Debug.Log($"[SHOOTER BOSS] Proiettile normale lanciato con distanza target: {targetDistance}");
            projScript.InitializeBossProjectile(
                owner, direction, normalProjectileSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, targetDistance, boss
            );
        }
        else
        {
            Debug.LogError("Il prefab del proiettile non ha lo script BossAcidProjectile!");
        }
    }

    // ATTACCO TUBI
    public void ShootPipeProjectile(GameObject owner, Vector2 direction, Vector3 spawnPos)
    {
        if (projectilePrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion launchRotation = Quaternion.Euler(0f, 0f, angle);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, launchRotation);

        if (proj.TryGetComponent<BossAcidProjectile>(out BossAcidProjectile projScript))
        {
            BossCtrl boss = owner.GetComponent<BossCtrl>();
            
            projScript.InitializeBossProjectile(
                owner, direction, pipeProjectileSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, pipeProjectileRange, boss
            );

            projScript.EnablePipeMode();
        }
    }
}
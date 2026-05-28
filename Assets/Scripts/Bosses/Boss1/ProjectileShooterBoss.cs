using UnityEngine;

public class ProjectileShooterBoss : ProjectileShooter
{
    [Header("Impostazioni Specifiche Boss")]
    [SerializeField] private GameObject pozzaPrefab;
    [SerializeField] private GameObject impactPrefab;
    
    
    [SerializeField] private float projectileLifeTime = 0.8f;
    [SerializeField] private float poolDuration = 3f;

    public void ShootBossProjectile(GameObject owner, Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[SHOOTER BOSS] Manca il prefab del proiettile!");
            return;
        }

        // Usa il firePoint ereditato dalla classe base (se esiste), altrimenti usa se stesso
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // 🔥 FIX ROTAZIONE ANIMAZIONE: Calcola l'angolo esatto basato sulla direzione di sparo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion launchRotation = Quaternion.Euler(0f, 0f, angle);

        // Fa nascere il proiettile GIÀ RUOTATO nel frame 0
        GameObject proj = Instantiate(projectilePrefab, spawnPos, launchRotation);

        // Inizializza il proiettile
        if (proj.TryGetComponent<BossAcidProjectile>(out BossAcidProjectile projScript))
        {
            BossCtrl boss = owner.GetComponent<BossCtrl>();
            projScript.InitializeBossProjectile(
                owner, direction, projectileSpeed, damage,
                pozzaPrefab, impactPrefab, poolDuration, projectileLifeTime, boss
            );
        }
        else
        {
            Debug.LogError("Il prefab del proiettile non ha lo script BossAcidProjectile!");
        }
    }
}
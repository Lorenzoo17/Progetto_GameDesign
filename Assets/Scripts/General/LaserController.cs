using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private GameObject laserVisualInstance;
    private LineRenderer laserRenderer;
    private CircleCollider2D laserCollider;

    private Vector2 currentDirection = Vector2.right;
    private Vector3 laserStartPos;
    private Vector3 laserEndPos;

    private float damageAccumulator = 0f;
    private HashSet<GameObject> hitThisFrame = new();
    private HashSet<GameObject> hitThisSecond = new();
    private float secondTimer = 0f;

    private AudioSource audioSource;

    /// <summary>
    /// Initialize the laser with its visual effect prefab
    /// </summary>
    public void Initialize(GameObject laserEffectPrefab, Vector3 startPosition, Vector2 direction)
    {
        if (laserEffectPrefab == null)
        {
            Debug.LogWarning("Laser effect prefab is null!");
            return;
        }

        currentDirection = direction.normalized;
        laserStartPos = startPosition;

        // Instantiate the laser visual effect
        laserVisualInstance = Instantiate(laserEffectPrefab, startPosition, Quaternion.identity);
        laserRenderer = laserVisualInstance.GetComponent<LineRenderer>();
        laserCollider = laserVisualInstance.GetComponent<CircleCollider2D>();

        if (laserRenderer == null)
        {
            Debug.LogWarning("Laser effect prefab doesn't have a LineRenderer!");
        }

        if (laserCollider == null)
        {
            Debug.LogWarning("Laser effect prefab doesn't have a CircleCollider2D!");
        }
    }

    /// <summary>
    /// Update laser position and rotation each frame
    /// </summary>
    public void UpdateLaser(Vector2 newDirection)
    {
        if (laserVisualInstance == null)
            return;

        currentDirection = newDirection.normalized;

        // Calculate laser end position
        laserEndPos = laserStartPos + (Vector3)currentDirection * 4; // 4 units long

        // Update line renderer
        if (laserRenderer != null)
        {
            laserRenderer.SetPosition(0, laserStartPos);
            laserRenderer.SetPosition(1, laserEndPos);
        }

        // Rotate the visual to face the direction
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        laserVisualInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Update collider position
        if (laserCollider != null)
        {
            laserVisualInstance.transform.position = laserStartPos;
        }

        // Reset hit tracking per frame
        hitThisFrame.Clear();
    }

    /// <summary>
    /// Apply damage to enemies in the laser's path
    /// </summary>
    public void ApplyDamage(Player player, LaserMutagenSO laserData, float deltaTime)
    {
        damageAccumulator += deltaTime;
        secondTimer += deltaTime;

        // Reset hit tracking every second
        if (secondTimer >= 1f)
        {
            hitThisSecond.Clear();
            secondTimer = 0f;
        }

        // Apply damage on tick rate
        if (damageAccumulator < laserData.tickRate)
            return;

        damageAccumulator = 0f;

        // Raycast along the laser path to find enemies
        Vector2 laserDir = currentDirection;
        float laserLength = laserData.laserLength;
        float damagePerTick = laserData.damagePerSecond * laserData.tickRate;

        // Use raycast to find all enemies in the laser path
        RaycastHit2D[] hits = Physics2D.RaycastAll(laserStartPos, laserDir, laserLength);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            GameObject targetObject = hit.collider.gameObject;

            // Skip if it's the player or already hit this second
            if (targetObject == player.gameObject || hitThisSecond.Contains(targetObject))
                continue;

            // Check if it's an enemy we can damage
            if (!Utils.CombatUtility.CanDamage(player.gameObject, targetObject))
                continue;

            // Try to get damageable component
            if (targetObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                float damage = player.playerStats.playerCurrentStats.GetAttack() * laserData.damagePerSecond * laserData.tickRate;
                DamageInfo damageInfo = new DamageInfo(damage, laserDir, player.gameObject, EntityType.Player);
                damageable.TakeDamage(damageInfo);

                // Mark as hit this second to prevent multiple hits
                hitThisSecond.Add(targetObject);

                Debug.Log($"Laser hit: {targetObject.name}");
            }
        }
    }

    /// <summary>
    /// Play the laser sound loop
    /// </summary>
    public void PlayLaserSound(AudioClip laserLoopSound)
    {
        if (laserLoopSound == null || laserVisualInstance == null)
            return;

        audioSource = laserVisualInstance.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = laserVisualInstance.AddComponent<AudioSource>();
        }

        audioSource.clip = laserLoopSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    /// <summary>
    /// Clean up the laser effect
    /// </summary>
    public void Destroy()
    {
        if (laserVisualInstance != null)
        {
            Destroy(laserVisualInstance);
        }

        hitThisSecond.Clear();
        hitThisFrame.Clear();
    }
}

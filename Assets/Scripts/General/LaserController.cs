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
    private LaserVisualEffect visualEffectScript;

    public void Initialize(GameObject laserEffectPrefab, Vector3 startPosition, Vector2 direction)
    {
        if (laserEffectPrefab == null)
        {
            Debug.LogWarning("Laser effect prefab is null!");
            return;
        }

        // Se per caso esisteva già un'istanza vecchia non distrutta, facciamo pulizia prima di crearne una nuova
        if (laserVisualInstance != null)
        {
            Object.Destroy(laserVisualInstance);
        }

        currentDirection = direction.normalized;
        laserStartPos = startPosition;
        Debug.Log($"[LaserController] Initializing laser at {startPosition} with direction {direction}");
        // Spawna il prefab DIRETTAMENTE sulla posizione di partenza passata (quella del player)
        laserVisualInstance = Object.Instantiate(laserEffectPrefab, startPosition, Quaternion.identity);

        laserRenderer = laserVisualInstance.GetComponent<LineRenderer>();
        laserCollider = laserVisualInstance.GetComponent<CircleCollider2D>();
        visualEffectScript = laserVisualInstance.GetComponent<LaserVisualEffect>();

        if (laserRenderer == null)
            Debug.LogWarning("Laser effect prefab doesn't have a LineRenderer!");

        if (laserCollider == null)
            Debug.LogWarning("Laser effect prefab doesn't have a CircleCollider2D!");

        // Forza subito la posizione del transform al frame 0 per evitare che compaia al centro della mappa
        laserVisualInstance.transform.position = startPosition;
    }

    /// <summary>
    /// Update laser position, dynamic length and rotation each frame
    /// </summary>
    public void UpdateLaser(Vector3 newStartPosition, Vector2 newDirection, float actualLength)
    {
        if (laserVisualInstance == null)
            return;

        // AGGIORNAMENTO CRUCIALE: Sposta il Transform dell'oggetto insieme al Player!
        laserStartPos = newStartPosition;
        laserVisualInstance.transform.position = laserStartPos;

        currentDirection = newDirection.normalized;
        laserEndPos = laserStartPos + (Vector3)currentDirection * actualLength;

        // Invia i dati corretti di rendering allo script visivo
        if (visualEffectScript != null)
        {
            visualEffectScript.SetupLaserLength(laserStartPos, currentDirection, actualLength);
        }
        else if (laserRenderer != null)
        {
            laserRenderer.SetPosition(0, laserStartPos);
            laserRenderer.SetPosition(1, laserEndPos);
        }

        // Se hai un CircleCollider2D che deve seguire la punta o l'asse, lo aggiorniamo qui
        if (laserCollider != null)
        {
            // Esempio: sposta il collider a metà della lunghezza del laser
            laserCollider.offset = currentDirection * (actualLength * 0.5f);
        }

        hitThisFrame.Clear();
    }

    public void ApplyDamage(Player player, LaserMutagenSO laserData, float deltaTime)
    {
        damageAccumulator += deltaTime;
        secondTimer += deltaTime;

        if (secondTimer >= 1f)
        {
            hitThisSecond.Clear();
            secondTimer = 0f;
        }

        if (damageAccumulator < laserData.tickRate)
            return;

        damageAccumulator = 0f;

        Vector2 laserDir = currentDirection;
        float laserLength = laserData.laserLength;

        RaycastHit2D[] hits = Physics2D.RaycastAll(laserStartPos, laserDir, laserLength);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            GameObject targetObject = hit.collider.gameObject;

            if (targetObject == player.gameObject || hitThisSecond.Contains(targetObject))
                continue;
            if (!Utils.CombatUtility.CanDamage(player.gameObject, targetObject))
                continue;
            Debug.Log($"[LaserController] Laser hit: {targetObject.name} at position {hit.point}");
            if (targetObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                float damage = player.playerStats.playerCurrentStats.GetMutagenPower() * laserData.damageMultiplier* laserData.damagePerSecond * laserData.tickRate;
                DamageInfo damageInfo = new DamageInfo(damage, laserDir, player.gameObject, EntityType.Player);
                Debug.Log($"[LaserController] Laser hit: {targetObject.name} at position {hit.point}");
                damageable.TakeDamage(damageInfo);

                hitThisSecond.Add(targetObject);
                Debug.Log($"Laser hit: {targetObject.name}");
            }
        }
    }

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

    public void Destroy()
    {
        if (laserVisualInstance != null)
        {
            Object.Destroy(laserVisualInstance);
        }

        hitThisSecond.Clear();
        hitThisFrame.Clear();
    }
}
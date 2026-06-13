using System;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class PlayerAttack : MonoBehaviour
{

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float deadZoneRadius;

    [Header("Centro di attacco (offset rispetto alla direzione di attacco in melee)")]
    public float attackCentreOffset = 1f; // richiamato in WeaponMelee

    private bool canAttack = true;
    private float attackTimer = 0f;

    [Header("AOE Attack")]
    [SerializeField] private AOEMutagenSO aoeData;
    private float aoeLastUsedTime = -999f;

    public GameObject currentWeapon;

    public Transform attackDirectionUI; // Componente per indicare direzione in cui il player
    // sta guardando
    [SerializeField] private float attackDirectionUIDistanceFromPlayer = 0.5f;
    private Vector2 attackDirection;
    [SerializeField] private bool knockBackWhileAttacking;
    [SerializeField] private float knockBackForce;

    public bool StopPlayerFromAttacking { get; set; }

    private bool blockAttack = false; // impostato durante dialoghi per evitare che attacchi durante dialoghi
    public void BlockAttack() => blockAttack = true;
    public void UnlockAttack() => blockAttack = false;

    public Transform GetWeaponHolder()
    {
        return weaponHolder;
    }
    public Vector2 GetAttackDirection()
    {
        return attackDirection;
    }
    public void RegisterInput()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackEvent -= Attack;
            InputManager.Instance.OnAttackEvent += Attack;
        }
    }

    public void Reinitialize()
    {
        RegisterInput();

        if (weaponHolder == null)
        {
            GameObject holder = GameObject.Find("WeaponHolder");
            if (holder != null)
                weaponHolder = holder.transform;
        }
    }

    private void Update()
    {
        CalculateAttackDirection();

        if (currentWeapon != null)
        {
            if (currentWeapon.TryGetComponent<IWeapon>(out IWeapon weapon))
            {
                weapon.HandleRotation(weaponHolder, attackDirection);
            }
        }

        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                canAttack = true;
            }
        }
    }

    // ----------- GESTIONE ATTACCO ------------

    private void Attack(object sender, EventArgs e)
    {
        if (StopPlayerFromAttacking) return;

        if (blockAttack) return;

        if (!canAttack || currentWeapon == null) return;

        if (!Player.Instance.playerMovement.canMove) return; // in modo da non poter attaccare durante splash screen del boss ad esempio

        currentWeapon.GetComponent<IWeapon>().Attack(attackDirection);

        // camera shake e knockback dopo attacco anche per arma ranged
        if (EffectManager.Instance != null)
        {
            if (currentWeapon.GetComponent<Weapon>() is WeaponMelee)
            {
                CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));
            }
            else
            {
                CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.RangedAttack));
            }
        }

        // knockback solamente se l'arma e' melee
        if (knockBackWhileAttacking && currentWeapon.GetComponent<Weapon>() is WeaponMelee)
        {
            Player.Instance.playerMovement.ApplyKnockback(-attackDirection, knockBackForce);
        }

        // per ora suono generico e non dipendente dall'arma
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.PlayerAttack, .15f);
        }

        attackTimer = CalculateAttackRateBasedOnWeapon();
        canAttack = false;
    }

    private float CalculateAttackRateBasedOnWeapon() {
        float baseAttackRate = Player.Instance.playerStats.playerCurrentStats.GetAttackRate();
        if (currentWeapon == null) return baseAttackRate;

        return baseAttackRate + currentWeapon.GetComponent<Weapon>().weaponAttackRateSlowdown;
    }
    
    //AOE methods
    public bool TriggerAOE(AOEMutagenSO data)
    {
        if (!CanActivateAOE(data)) return false;

        ActivateAOE(data, attackDirection);
        aoeLastUsedTime = Time.time;
        return true;
    }

    private bool CanActivateAOE(AOEMutagenSO data)
    {
        if (data == null) return false;
        if (data.cooldown <= 0f) return true;
        return Time.time >= aoeLastUsedTime + data.cooldown;
    }

    private void ActivateAOE(AOEMutagenSO data, Vector2 direction)
    {
        if (data == null) return;

        Vector2 aoeCenter = transform.position;

        // Trovare tutti i nemici nel raggio AOE
        Collider2D[] colliders = Physics2D.OverlapCircleAll(aoeCenter, data.radius);

        foreach (Collider2D entity in colliders)
        {
            if (Utils.CombatUtility.CanDamage(Player.Instance.gameObject, entity.gameObject))
            {
                if (entity.gameObject.TryGetComponent<IDamageable>(out IDamageable entityDamageable))
                {
                    // Calcolo danno: danno base del player * moltiplicatore AOE
                    float aoeDamage = Player.Instance.playerStats.playerCurrentStats.GetAttack() * data.damageMultiplier;
                    // Direzione verso il bersaglio per il knockback
                    Vector2 directionToTarget = (entity.transform.position - transform.position).normalized;
                    DamageInfo damageInfo = new DamageInfo(aoeDamage, directionToTarget, Player.Instance.gameObject, EntityType.Player);
                    entityDamageable.TakeDamage(damageInfo);
                }
            }
        }

        // Istanzia effetto visivo AOE
        if (data.effectPrefab != null)
        {
            Instantiate(data.effectPrefab, aoeCenter, Quaternion.identity);
        }

        // Camera shake
        if (EffectManager.Instance != null)
        {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(data.shakeType));
        }

        // Suono effetto AOE
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(data.soundEffect, 0.25f);
        }
    }
    //End AOE methods
    private void HandleSorting(float angle)
    {
        if (angle > -90f && angle < 90f)
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 2;
        else
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    private void OnDrawGizmos()
    {
        if (aoeData == null) return;

        // Disegna il raggio AOE in giallo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeData.radius);
    }

    // Attack direction centrata su weaponHolder e non su transform.position in modo da non avere incoerenze
    // nella direzione dell'arma ranged
    private void CalculateAttackDirection()
    {
        Vector2 direction = InputManager.Instance.CalculateAimDirection(weaponHolder.position);

        if (direction.magnitude > deadZoneRadius)
        {
            attackDirection = direction.normalized;
        }

        if (attackDirectionUI == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        attackDirectionUI.rotation = rot;
        // centrato in weaponHolder
        attackDirectionUI.position = weaponHolder.position + (Vector3)(direction.normalized * attackDirectionUIDistanceFromPlayer);
    }

    // ----------- GESTIONE ARMI ------------
    // al seguito del pick up si assegna nuova arma
    public void SetCurrentWeapon(GameObject newWeapon)
    {
        // droppo arma corrente se presente
        if (currentWeapon != null)
        {
            if (currentWeapon.TryGetComponent<Weapon>(out Weapon w))
            {
                w.DropWeapon();
            }
        }

        // assegno nuova arma
        currentWeapon = newWeapon;
        currentWeapon.transform.SetParent(weaponHolder);
        currentWeapon.transform.localPosition = Vector2.zero;
        currentWeapon.transform.localRotation = Quaternion.Euler(currentWeapon.GetComponent<Weapon>().xRotationOffset, 0, 0);
    }

    public Weapon GetCurrentWeapon()
    {
        if (currentWeapon == null)
            return null;

        return currentWeapon.GetComponent<Weapon>();
    }
    /// <summary>
    /// Trigger a laser mutagen attack
    /// Called by LaserMutagenSO.Activate()
    /// </summary>
    public bool TriggerLaser(LaserMutagenSO laserData, MutagenInstance instance)
    {
        if (laserData == null)
            return false;

        // Create the laser controller
        LaserController laserController = new LaserController();
        Debug.Log($"[PlayerAttack] Triggering laser attack with data: {laserData.mutagenName}");
        // Initialize with visual effect at weapon holder position
        laserController.Initialize(laserData.laserEffectPrefab, weaponHolder.position, attackDirection);

        // Play looping sound if provided
        if (laserData.laserLoopSound != null)
        {
            laserController.PlayLaserSound(laserData.laserLoopSound);
        }

        // Camera shake
        if (EffectManager.Instance != null)
        {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(laserData.shakeType));
        }

        // Play sound effect
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(laserData.soundEffect, 0.25f);
        }

        // Store the controller in the instance's custom data for later access
        instance.customData = laserController;

        Debug.Log($"Laser attack triggered: {laserData.mutagenName}");

        return true;
    }


    //Acidic Burp mutagen
    public void SpawnGiantProjectile(
    GameObject projectilePrefab,
    float speed,
    float damageMultiplier,
    float scaleMultiplier)
    {
        Vector2 dir = GetAttackDirection();

        if (dir == Vector2.zero || projectilePrefab == null)
            return;
        if (dir == Vector2.zero || projectilePrefab == null)
            return;

        Transform spawnPoint = weaponHolder;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );


        projectile.transform.localScale *= scaleMultiplier;
        projectile.transform.localScale *= scaleMultiplier;

        float damage =
            Player.Instance.playerStats.playerCurrentStats.GetAttack()
            * damageMultiplier;

        if(EffectManager.Instance != null)
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));

        if (projectile.TryGetComponent<GiantProjectile>(out var gp))
        {
            gp.Initialize(
                dir,
                speed,
                damage,
                gameObject
            );
        }
    }

    // Puddle mutagen
    public void SpawnPuddle(
        GameObject puddlePrefab,
        float duration,
        float tickDamage,
        float tickInterval,
        float radius)
    {
        Vector3 pos = transform.position;

        GameObject puddle = Instantiate(puddlePrefab, pos, Quaternion.identity);

        MouthwashPuddle puddleScript = puddle.GetComponent<MouthwashPuddle>();

        if (puddleScript != null)
        {
            puddleScript.Initialize(
                tickDamage,
                tickInterval,
                radius,
                duration,
                gameObject // owner = player (per immunità)
            );
        }
    }

    private void OnDestroy() {
        if (InputManager.Instance != null) {
            InputManager.Instance.OnAttackEvent -= Attack;
        }
    }
}

using System;
using UnityEngine;
using FirstGearGames.SmoothCameraShaker;

public class PlayerAttack : MonoBehaviour {

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

    public Transform GetWeaponHolder() {
        return weaponHolder;
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

    private void Update() {
        CalculateAttackDirection();

        if (currentWeapon != null) {
            if (currentWeapon.TryGetComponent<IWeapon>(out IWeapon weapon)) {
                weapon.HandleRotation(weaponHolder, attackDirection);
            }
        }

        if (!canAttack) {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f) {
                canAttack = true;
            }
        }
    }

    // ----------- GESTIONE ATTACCO ------------

    private void Attack(object sender, EventArgs e) {
        if (!canAttack || currentWeapon == null) return;

        currentWeapon.GetComponent<IWeapon>().Attack(attackDirection);

        // camera shake e knockback dopo attacco anche per arma ranged
        if(EffectManager.Instance != null) {
            if (currentWeapon.GetComponent<Weapon>() is WeaponMelee) {
                CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.MeleeAttack));
            }
            else {
                CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(ShakeDataType.RangedAttack));
            }
        }

        // knockback solamente se l'arma e' melee
        if (knockBackWhileAttacking && currentWeapon.GetComponent<Weapon>() is WeaponMelee) {
            Player.Instance.playerMovement.ApplyKnockback(-attackDirection, knockBackForce);
        }

        // per ora suono generico e non dipendente dall'arma
        if(SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(SoundID.PlayerAttack, .15f);
        }

        attackTimer = Player.Instance.playerStats.playerCurrentStats.GetAttackRate();
        canAttack = false;
    }
    
    //AOE methods
    public bool TriggerAOE(AOEMutagenSO data) {
        if (!CanActivateAOE(data)) return false;
        
        ActivateAOE(data, attackDirection);
        aoeLastUsedTime = Time.time;
        return true;
    }

    private bool CanActivateAOE(AOEMutagenSO data) {
        if (data == null) return false;
        if (data.cooldown <= 0f) return true;
        return Time.time >= aoeLastUsedTime + data.cooldown;
    }

    private void ActivateAOE(AOEMutagenSO data, Vector2 direction) {
        if (data == null) return;

        Vector2 aoeCenter = transform.position;

        // Trovare tutti i nemici nel raggio AOE
        Collider2D[] colliders = Physics2D.OverlapCircleAll(aoeCenter, data.radius);

        foreach(Collider2D entity in colliders) {
            if(Utils.CombatUtility.CanDamage(Player.Instance.gameObject, entity.gameObject)) {
                if (entity.gameObject.TryGetComponent<IDamageable>(out IDamageable entityDamageable)) {
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
        if (data.effectPrefab != null) {
            Instantiate(data.effectPrefab, aoeCenter, Quaternion.identity);
        }

        // Camera shake
        if(EffectManager.Instance != null) {
            CameraShakerHandler.Shake(EffectManager.Instance.GetShakeDataByType(data.shakeType));
        }

        // Suono effetto AOE
        if(SoundManager.Instance != null) {
            SoundManager.Instance.PlaySound2D(data.soundEffect, 0.25f);
        }
    }
    //End AOE methods
    private void HandleSorting(float angle) {
        if (angle > -90f && angle < 90f)
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 2;
        else
            currentWeapon.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    private void OnDrawGizmos() {
        if (aoeData == null) return;
        
        // Disegna il raggio AOE in giallo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeData.radius);
    }

    // Attack direction centrata su weaponHolder e non su transform.position in modo da non avere incoerenze
    // nella direzione dell'arma ranged
    private void CalculateAttackDirection() {
        Vector2 direction = InputManager.Instance.CalculateAimDirection(weaponHolder.position);

        if(direction.magnitude > deadZoneRadius) {
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
    public void SetCurrentWeapon(GameObject newWeapon) {
        // droppo arma corrente se presente
        if(currentWeapon != null) {
            currentWeapon.transform.SetParent(null);
            if (currentWeapon.TryGetComponent<Weapon>(out Weapon w)) {
                w.DropWeapon();
            }
        }

        // assegno nuova arma
        currentWeapon = newWeapon;
        currentWeapon.transform.SetParent(weaponHolder);
        currentWeapon.transform.localPosition = Vector2.zero;
        currentWeapon.transform.localRotation = Quaternion.Euler(currentWeapon.GetComponent<Weapon>().xRotationOffset, 0, 0);
    }

    public Weapon GetCurrentWeapon() {
        if (currentWeapon == null)
            return null;

        return currentWeapon.GetComponent<Weapon>();
    }
}

using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class CurvedProjectile : ProjectileBase
{
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private bool spawnStain;
    [SerializeField] private float attackRange;
    private bool hasExploded = false;

    [SerializeField] private BulletCurvatureVisual projectileVisual;

    [SerializeField] private float mostlyVerticalCurveMultiplier = 0.05f;
    [SerializeField] private float verticalThreshold = 0.85f;
    [SerializeField] private float horizontalThreshold = 0.35f;

    private Vector3 trajectoryStartPoint;
    private Vector3 trajectoryEndPoint;
    private Vector3 trajectoryRange;

    private float moveSpeed;
    private float maxMoveSpeed;
    private float projectileRange;
    private float trajectoryMaxRelativeHeight;

    private AnimationCurve trajectoryAnimationCurve;
    private AnimationCurve axisCorrectionAnimationCurve;
    private AnimationCurve projectileSpeedAnimationCurve;

    private Vector3 projectileMoveDir;

    private float nextYTrajectoryPosition;
    private float nextXTrajectoryPosition;
    private float nextPositionYCorrectionAbsolute;
    private float nextPositionXCorrectionAbsolute;

    private float trajectoryProgress;

    private void Update()
    {
        if (!initialized)
            return;

        UpdateProjectilePosition();

        if (trajectoryProgress >= 1f)
        {
            OnBulletExplosion();
        }
    }

    private void OnBulletExplosion()
    { // richiamato in UpdateProjectilePosition()
        if (hasExploded || Player.Instance == null) return;

        hasExploded = true;
        // spawn di effetto
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        // screen shake
        if (EffectManager.Instance != null)
        {
            ShakeData data = EffectManager.Instance.GetShakeDataByType(ShakeDataType.Explosion);
            if (data != null)
            {
                CameraShakerHandler.Shake(data);
            }
        }

        // danno ad area
        Collider2D[] entities = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D entity in entities)
        {
            if (!Utils.CombatUtility.CanDamage(base.owner, entity.gameObject)) continue;

            Vector2 attackDirection = ((Vector2)entity.transform.position - (Vector2)transform.position).normalized;
            TryDealDamage(entity, attackDirection);
        }


        if (spawnStain && EffectManager.Instance != null)
        {
            GameObject stain = EffectManager.Instance.SpawnVisualEffect(
                VisualEffectType.Attack,
                transform.position,
                Quaternion.identity
            );

            if (stain.TryGetComponent<Stain>(out Stain stainComponent))
            {
                stainComponent.SetUpStain(owner, 1);
            }
        }

        // distruzione del bullet
        Destroy(gameObject);
    }

    private void UpdateProjectilePosition()
    {
        trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        if (trajectoryRange.sqrMagnitude <= 0.01f)
        {
            Destroy(gameObject);
            return;
        }

        if (Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y))
        {
            UpdatePositionWithXCurve();
        }
        else
        {
            UpdatePositionWithYCurve();
        }
    }

    private void UpdatePositionWithXCurve()
    {
        float verticalSign = Mathf.Sign(trajectoryRange.y);

        float nextPositionY = transform.position.y + moveSpeed * verticalSign * Time.deltaTime;

        float nextPositionYNormalized =
            (nextPositionY - trajectoryStartPoint.y) / trajectoryRange.y;

        trajectoryProgress = nextPositionYNormalized;

        nextPositionYNormalized = Mathf.Clamp01(nextPositionYNormalized);

        float nextPositionXNormalized = trajectoryAnimationCurve.Evaluate(nextPositionYNormalized);
        nextXTrajectoryPosition = nextPositionXNormalized * trajectoryMaxRelativeHeight;

        float nextPositionXCorrectionNormalized = axisCorrectionAnimationCurve.Evaluate(nextPositionYNormalized);
        nextPositionXCorrectionAbsolute = nextPositionXCorrectionNormalized * trajectoryRange.x;

        if (trajectoryRange.x > 0 && trajectoryRange.y > 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        if (trajectoryRange.x < 0 && trajectoryRange.y < 0)
        {
            nextXTrajectoryPosition = -nextXTrajectoryPosition;
        }

        float nextPositionX = trajectoryStartPoint.x + nextXTrajectoryPosition + nextPositionXCorrectionAbsolute;

        Vector3 newPosition = new Vector3(nextPositionX, nextPositionY, 0f);

        CalculateNextProjectileSpeed(nextPositionYNormalized);

        projectileMoveDir = newPosition - transform.position;
        transform.position = newPosition;
    }

    private void UpdatePositionWithYCurve()
    {
        float horizontalSign = Mathf.Sign(trajectoryRange.x);

        float nextPositionX = transform.position.x + moveSpeed * horizontalSign * Time.deltaTime;

        float nextPositionXNormalized =
            (nextPositionX - trajectoryStartPoint.x) / trajectoryRange.x;

        trajectoryProgress = nextPositionXNormalized;

        nextPositionXNormalized = Mathf.Clamp01(nextPositionXNormalized);

        float nextPositionYNormalized = trajectoryAnimationCurve.Evaluate(nextPositionXNormalized);
        nextYTrajectoryPosition = nextPositionYNormalized * trajectoryMaxRelativeHeight;

        float nextPositionYCorrectionNormalized = axisCorrectionAnimationCurve.Evaluate(nextPositionXNormalized);
        nextPositionYCorrectionAbsolute = nextPositionYCorrectionNormalized * trajectoryRange.y;

        float nextPositionY = trajectoryStartPoint.y + nextYTrajectoryPosition + nextPositionYCorrectionAbsolute;

        Vector3 newPosition = new Vector3(nextPositionX, nextPositionY, 0f);

        CalculateNextProjectileSpeed(nextPositionXNormalized);

        projectileMoveDir = newPosition - transform.position;
        transform.position = newPosition;
    }

    private void CalculateNextProjectileSpeed(float normalizedPosition)
    {
        float nextMoveSpeedNormalized = projectileSpeedAnimationCurve.Evaluate(normalizedPosition);
        moveSpeed = nextMoveSpeedNormalized * maxMoveSpeed;
    }

    public void InitializeProjectile(
        GameObject owner,
        Vector2 shootDirection,
        float range,
        float maxMoveSpeed,
        float trajectoryMaxHeight,
        float damage
    )
    {
        InitializeProjectile(owner, shootDirection, damage);

        if (shootDirection.sqrMagnitude <= 0.01f)
        {
            shootDirection = Vector2.right;
        }

        this.direction = shootDirection.normalized;
        this.projectileRange = range;
        this.maxMoveSpeed = maxMoveSpeed;

        trajectoryStartPoint = transform.position;
        trajectoryEndPoint = trajectoryStartPoint + (Vector3)direction * projectileRange;
        if (owner == Player.Instance.gameObject)
        {
            damage += Player.Instance.playerStats.playerCurrentStats.getAttack() / 2;
        }

        trajectoryRange = trajectoryEndPoint - trajectoryStartPoint;

        bool isMostlyVerticalShot = Mathf.Abs(direction.y) > verticalThreshold && Mathf.Abs(direction.x) < horizontalThreshold;

        float curveMultiplier = isMostlyVerticalShot ? mostlyVerticalCurveMultiplier : 1f;

        this.trajectoryMaxRelativeHeight = projectileRange * trajectoryMaxHeight * curveMultiplier;

        moveSpeed = maxMoveSpeed;

        initialized = true;

        // Se il tuo ProjectileVisual dipendeva dal target, va modificato.
        // Per ora puoi commentarlo oppure creare un metodo SetDirection.
        if (projectileVisual != null)
        {
            projectileVisual.InitializeVisual(trajectoryStartPoint, trajectoryEndPoint);
        }
    }

    public void InitializeAnimationCurves(
        AnimationCurve trajectoryAnimationCurve,
        AnimationCurve axisCorrectionAnimationCurve,
        AnimationCurve projectileSpeedAnimationCurve
    )
    {
        this.trajectoryAnimationCurve = trajectoryAnimationCurve;
        this.axisCorrectionAnimationCurve = axisCorrectionAnimationCurve;
        this.projectileSpeedAnimationCurve = projectileSpeedAnimationCurve;
    }

    public Vector3 GetProjectileMoveDir()
    {
        return projectileMoveDir;
    }

    public float GetNextYTrajectoryPosition()
    {
        return nextYTrajectoryPosition;
    }

    public float GetNextPositionYCorrectionAbsolute()
    {
        return nextPositionYCorrectionAbsolute;
    }

    public float GetNextXTrajectoryPosition()
    {
        return nextXTrajectoryPosition;
    }

    public float GetNextPositionXCorrectionAbsolute()
    {
        return nextPositionXCorrectionAbsolute;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

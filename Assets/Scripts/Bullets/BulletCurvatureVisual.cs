using UnityEngine;

public class BulletCurvatureVisual : MonoBehaviour {
    [SerializeField] private Transform projectileVisual;
    [SerializeField] private Transform projectileShadow;
    [SerializeField] private CurvedProjectile projectile;

    [SerializeField] private float shadowPositionDivider = 6f;
    [SerializeField] private float minShadowOffset = 0.25f;
    [SerializeField] private float shadowHeightCurveMultiplier = 1f;

    private Vector3 trajectoryStartPosition;
    private Vector3 trajectoryEndPosition;

    private bool initialized;

    private void Update() {
        if (!initialized)
            return;

        UpdateProjectileRotation();
        UpdateShadowPosition();

        float trajectoryProgressMagnitude =
            (transform.position - trajectoryStartPosition).magnitude;

        float trajectoryMagnitude =
            (trajectoryEndPosition - trajectoryStartPosition).magnitude;

        float trajectoryProgressNormalized =
            trajectoryProgressMagnitude / trajectoryMagnitude;

        if (trajectoryProgressNormalized < .7f) {
            UpdateProjectileShadowRotation();
        }
    }

    private void UpdateShadowPosition() {
        Vector3 newPosition = transform.position;
        Vector3 trajectoryRange = trajectoryEndPosition - trajectoryStartPosition;

        float trajectoryProgressMagnitude =
            (transform.position - trajectoryStartPosition).magnitude;

        float trajectoryMagnitude = trajectoryRange.magnitude;

        if (trajectoryMagnitude <= 0.01f)
            return;

        float t = Mathf.Clamp01(trajectoryProgressMagnitude / trajectoryMagnitude);

        float shadowArcHeight = Mathf.Sin(t * Mathf.PI) * minShadowOffset * shadowHeightCurveMultiplier;

        if (Mathf.Abs(trajectoryRange.normalized.x) < Mathf.Abs(trajectoryRange.normalized.y)) {
            // Tiro verticale: separo l'ombra sul lato X
            float sideSign = trajectoryRange.x >= 0f ? 1f : -1f;

            if (Mathf.Abs(trajectoryRange.x) < 0.01f) {
                sideSign = 1f;
            }

            newPosition.x =
                trajectoryStartPosition.x +
                projectile.GetNextXTrajectoryPosition() / shadowPositionDivider +
                projectile.GetNextPositionXCorrectionAbsolute() +
                sideSign * shadowArcHeight;
        }
        else {
            // Tiro orizzontale: separo l'ombra sull'asse Y
            newPosition.y =
                trajectoryStartPosition.y +
                projectile.GetNextYTrajectoryPosition() / shadowPositionDivider +
                projectile.GetNextPositionYCorrectionAbsolute() -
                shadowArcHeight;
        }

        projectileShadow.position = newPosition;
    }

    private void UpdateProjectileRotation() {
        Vector3 projectileMoveDir = projectile.GetProjectileMoveDir();

        if (projectileMoveDir.sqrMagnitude <= 0.01f)
            return;

        projectileVisual.rotation = Quaternion.Euler(
            0,
            0,
            Mathf.Atan2(projectileMoveDir.y, projectileMoveDir.x) * Mathf.Rad2Deg
        );
    }

    private void UpdateProjectileShadowRotation() {
        Vector3 projectileMoveDir = projectile.GetProjectileMoveDir();

        if (projectileMoveDir.sqrMagnitude <= 0.01f)
            return;

        projectileShadow.rotation = Quaternion.Euler(
            0,
            0,
            Mathf.Atan2(projectileMoveDir.y, projectileMoveDir.x) * Mathf.Rad2Deg
        );
    }

    public void InitializeVisual(Vector3 startPosition, Vector3 endPosition) {
        trajectoryStartPosition = startPosition;
        trajectoryEndPosition = endPosition;
        initialized = true;
    }
}

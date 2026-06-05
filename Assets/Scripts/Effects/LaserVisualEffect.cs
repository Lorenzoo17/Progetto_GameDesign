using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserVisualEffect : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Sprite Sheet Animation")]
    [SerializeField] private int totalFrames = 4;
    [SerializeField] private float framesPerSecond = 12f;

    [Header("Tentacle Scaling (for shape)")]
    [SerializeField] private float startWidthMultiplier = 1.2f;
    [SerializeField] private float endWidthMultiplier = 0.4f;
    [SerializeField] private AnimationCurve tentacleProfile;

    private Material lineMaterial;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;

    private void OnEnable()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.alignment = LineAlignment.View;

            lineMaterial = lineRenderer.material;
            lineMaterial.mainTextureScale = new Vector2(1f / totalFrames, 1f);
        }

        animationTimer = 0f;
        currentFrameIndex = 0;
        ApplyTextureOffset();
    }

    /// <summary>
    /// QUESTO È IL METODO MANCANTE!
    /// Imposta la lunghezza e la direzione del tentacolo nel mondo 2D.
    /// </summary>
    public void SetupLaserLength(Vector3 startPosition, Vector2 direction, float length)
    {
        if (lineRenderer == null) return;

        // Forziamo il LineRenderer ad avere esattamente 2 punti (Inizio e Fine)
        lineRenderer.positionCount = 2;

        // Calcoliamo i punti bloccando l'asse Z a 0 per il 2D
        Vector3 startPoint = new Vector3(startPosition.x, startPosition.y, 0f);
        Vector3 endPoint = startPoint + (Vector3)(direction.normalized * length);

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    private void Update()
    {
        if (lineRenderer == null) return;

        // 1. Gestione dell'animazione fotogramma per fotogramma
        animationTimer += Time.deltaTime;
        float timePerFrame = 1f / framesPerSecond;

        if (animationTimer >= timePerFrame)
        {
            currentFrameIndex = (currentFrameIndex + 1) % totalFrames;
            animationTimer -= timePerFrame;
            ApplyTextureOffset();
        }

        // 2. Aggiornamento della forma del tentacolo
        UpdateLaserWidths();
    }

    private void ApplyTextureOffset()
    {
        if (lineMaterial != null)
        {
            float offsetX = (float)currentFrameIndex / totalFrames;
            lineMaterial.mainTextureOffset = new Vector2(offsetX, 0f);
        }
    }

    private void UpdateLaserWidths()
    {
        if (tentacleProfile == null || tentacleProfile.keys.Length == 0)
        {
            lineRenderer.startWidth = startWidthMultiplier;
            lineRenderer.endWidth = endWidthMultiplier;
        }
        else
        {
            lineRenderer.widthCurve = tentacleProfile;
        }
    }
}

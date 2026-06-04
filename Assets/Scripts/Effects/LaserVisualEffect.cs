using UnityEngine;

/// <summary>
/// Attach this to the root of your laser effect prefab.
/// Handles frame-by-frame sprite sheet animation and custom scaling over a LineRenderer.
/// This version FORCES Stretched texture mode for single-frame stretching.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserVisualEffect : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Sprite Sheet Animation")]
    [SerializeField] private int totalFrames = 4;        // Based on your 4 frames
    [SerializeField] private float framesPerSecond = 12f; // Animation speed

    [Header("Tentacle Scaling (for shape)")]
    // These now define the overall shape curve, not frame dimensions
    [SerializeField] private float startWidthMultiplier = 1.2f; // Thicker base
    [SerializeField] private float endWidthMultiplier = 0.4f;   // Tapers out like a tip
    [SerializeField] private AnimationCurve tentacleProfile;    // Custom shape profile

    private Material lineMaterial;
    private float animationTimer = 0f;
    private int currentFrameIndex = 0;

    private void OnEnable()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            // CRITICAL ADDITION: Force the texture mode to stretch!
            // This tells Unity to use the full texture, stretched across the entire line.
            lineRenderer.textureMode = LineTextureMode.Stretch;

            // Create an instance of the material to avoid modifying the asset.
            lineMaterial = lineRenderer.material;

            // Set the horizontal texture scaling to match exactly 1 frame's width (0.25).
            // Example: 4 frames = 0.25 scale (1/4th of the entire sheet is shown at once)
            lineMaterial.mainTextureScale = new Vector2(1f / totalFrames, 1f);
        }

        animationTimer = 0f;
        currentFrameIndex = 0;
        ApplyTextureOffset();
    }

    private void Update()
    {
        if (lineRenderer == null) return;

        // 1. Handle Frame-by-Frame Flipbook Animation
        animationTimer += Time.deltaTime;
        float timePerFrame = 1f / framesPerSecond;

        if (animationTimer >= timePerFrame)
        {
            // Move to next frame and loop back to 0
            currentFrameIndex = (currentFrameIndex + 1) % totalFrames;
            animationTimer -= timePerFrame;
            ApplyTextureOffset();
        }

        // 2. Handle Custom Tentacle Scaling (Shape only, not frames)
        UpdateLaserWidths();
    }

    private void ApplyTextureOffset()
    {
        if (lineMaterial != null)
        {
            // Shifts the horizontal offset to match the target frame index block
            float offsetX = (float)currentFrameIndex / totalFrames;
            lineMaterial.mainTextureOffset = new Vector2(offsetX, 0f);
        }
    }

    private void UpdateLaserWidths()
    {
        // Fallback default shape if no curve is specified
        if (tentacleProfile == null || tentacleProfile.keys.Length == 0)
        {
            lineRenderer.startWidth = startWidthMultiplier;
            lineRenderer.endWidth = endWidthMultiplier;
        }
        else
        {
            // Advanced scaling: reads along the LineRenderer's points length for shape
            lineRenderer.widthCurve = tentacleProfile;
        }
    }
}

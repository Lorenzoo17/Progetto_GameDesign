using UnityEngine;

/// <summary>
/// Attach this to the root of your laser effect prefab
/// Handles the visual animations for the laser beam
/// </summary>
public class LaserVisualEffect : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minWidth = 0.8f;
    [SerializeField] private float maxWidth = 1.5f;

    private Material lineMaterial;
    private float elapsedTime = 0f;

    private void OnEnable()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineMaterial = lineRenderer.material;
        }

        elapsedTime = 0f;
    }

    private void Update()
    {
        if (lineRenderer == null)
            return;

        elapsedTime += Time.deltaTime;

        // Pulse the width for a pulsing laser effect
        float width = Mathf.Lerp(minWidth, maxWidth, Mathf.Sin(elapsedTime * pulseSpeed) * 0.5f + 0.5f);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width * 0.5f; // Taper to a point

        // Animate material offset for a flowing effect (if using a scrolling texture)
        if (lineMaterial != null)
        {
            lineMaterial.mainTextureOffset = new Vector2(elapsedTime * 2f, 0);
        }
    }
}

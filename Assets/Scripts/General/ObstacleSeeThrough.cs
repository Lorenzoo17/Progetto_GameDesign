using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleSeeThrough : MonoBehaviour {
    [SerializeField] private float transparentAlpha = 0.35f;

    private SpriteRenderer sr;
    private Tilemap tilemap;

    private Color startSpriteColor;
    private Color startTilemapColor;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
        tilemap = GetComponent<Tilemap>();

        if (sr != null) {
            startSpriteColor = sr.color;
        }

        if (tilemap != null) {
            startTilemapColor = tilemap.color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.GetComponent<Player>())
            return;

        SetAlpha(transparentAlpha);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!other.GetComponent<Player>())
            return;

        RestoreAlpha();
    }

    private void SetAlpha(float alpha) {
        if (sr != null) {
            Color newColor = sr.color;
            newColor.a = alpha;
            sr.color = newColor;
        }

        if (tilemap != null) {
            Color newColor = tilemap.color;
            newColor.a = alpha;
            tilemap.color = newColor;
        }
    }

    private void RestoreAlpha() {
        if (sr != null) {
            sr.color = startSpriteColor;
        }

        if (tilemap != null) {
            tilemap.color = startTilemapColor;
        }
    }
}
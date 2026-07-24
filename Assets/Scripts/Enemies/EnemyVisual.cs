using UnityEngine;

public class EnemyVisual : MonoBehaviour
{
    [SerializeField] private Color blinkAfterDamageTargetColor;
    [SerializeField] private float blinkAfterDamageTime;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float hitEffectSpawnPositionOffset = 0.5f;
    [SerializeField] private float hitEffectRotationOffset = -90f;
    [SerializeField] private bool invertFlipDirection;
    
    private SpriteRenderer sr;
    private Color initialColor;

    private Animator anim;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = transform.Find("Visual").GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning("Componente Visual non trovato nel transform");
            }
            Debug.Log("SpriteRenderer non trovato sul GameObject principale, cercato nei figli.");
        }
        initialColor = sr.color;

        anim = GetComponent<Animator>();

    }

    public void FlipBasedOnPlayer()
    {
        if (Player.Instance == null) return;

        Vector3 scale = transform.localScale;
        int flipDirection = invertFlipDirection ? -1 : 1;

        if (Player.Instance.transform.position.x > transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x) * flipDirection;
        }
        else
        {
            scale.x = Mathf.Abs(scale.x) * flipDirection;
        }

        transform.localScale = scale;
    }

    public void BlinkAfterDamage(DamageEventArgs e)
    {
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        if (sr != null)
        {
            sr.color = blinkAfterDamageTargetColor * 3f;
        }

        if (hitEffect != null)
        {
            Vector2 spawnPos = (Vector2)transform.position + e.AttackDirection.normalized * hitEffectSpawnPositionOffset;
            float angle = Mathf.Atan2(e.AttackDirection.y, e.AttackDirection.x) * Mathf.Rad2Deg;

            GameObject effect = Instantiate(hitEffect, spawnPos, Quaternion.identity);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, angle + hitEffectRotationOffset);
        }
    }



}

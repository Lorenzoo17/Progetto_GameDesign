using UnityEngine;

public class CutscenePlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Impostazioni Transizione")]
    [SerializeField] private float moveSpeed = 4f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        
    }

    private void Start()
    {
        // Forza lo sprite a guardare a destra (non flippato)
        if (sr != null)
        {
            sr.flipX = false;
        }

        // Attiva lo stato di camminata usando lo stesso parametro del gioco principale
        if (anim != null)
        {
            anim.SetBool("Moving", true);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Muove costantemente l'oggetto verso destra usando il Rigidbody2D (compatibile con Unity 6)
        rb.linearVelocity = Vector2.right * moveSpeed;
    }
}
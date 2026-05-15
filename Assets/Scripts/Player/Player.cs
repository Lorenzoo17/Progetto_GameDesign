using System.Collections;
using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerStats playerStats;
    public PlayerHealth playerHealth;
    public PlayerInteract playerInteract;
    public PlayerMana playerMana;
    public MutagenController mutagenController;

    public PerkController perkController;
    public StatusController statusController;
    public ShakeData cameraShakeData;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        playerInteract = GetComponent<PlayerInteract>();
        playerMana = GetComponent<PlayerMana>();
        mutagenController = GetComponent<MutagenController>();
        perkController = GetComponent<PerkController>();
        statusController = GetComponent<StatusController>();

        Debug.Log(perkController);
    }

    // Interazione per ora gestita in questo script direttamente
    // Per ora gestito con ontriggerenter
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<ICollectible>() != null)
        {
            other.gameObject.GetComponent<ICollectible>().Collect(this);
        }

    }

    public void OnPlayerDeath()
    {
        //inibisco le azioni del player (movimento + attacco)
        playerMovement.enabled = false;
        playerAttack.enabled = false;

        //Fermo il rigidbody del player
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; // Blocca fisicamente il corpo
        }

        //Se si volessero aggiungere animazioni di morte
        /*Animator anim = GetComponent<Animator>();
        if (anim != null) {
            anim.SetTrigger("Die"); // Assicurati di avere un trigger "Die" nell'Animator
        }*/

        //Esegui il reload della scena TODO: sostituire con schermata di game over
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1.5f); // Aspetta un attimo per enfasi drammatica
        LevelLoader.Instance.RestartLevel();   // Fa tutto lui (animazione + caricamento)
    }
}

using System.Collections;
using FirstGearGames.SmoothCameraShaker;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool isDead = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {   
        ReinitializeSceneReferences();
        MoveToSpawn();

    }

    //Metodo per trasportare oggetti tra scene
    private void ReinitializeSceneReferences()
    {
        // Input
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMutagenPressed -= mutagenController.TryUseMutagen;
            InputManager.Instance.OnMutagenPressed += mutagenController.TryUseMutagen;
        }

        mutagenController.RegisterInput();
        playerAttack.Reinitialize();
        playerMana.InitializeMana();
    }

    public void OnPlayerDeath()
    {   
        isDead = true;
        //inibisco le azioni del player (movimento + attacco)
        playerMovement.enabled = false;
        playerAttack.enabled = false;
        InputManager.Instance.inputEnabled = false;

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
        GameOverManager.Instance.ShowGameOver();
    }

    //Distruzione player per cambio scena, da chiamare dal level loader
    public static void DestroyPlayer()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }

    public void DestroySelf()
    {
        Instance = null;
        Destroy(gameObject);
    }

    private void MoveToSpawn()
    {
        if (SpawnPoint.currentSpawn != null)
        {
            transform.position = SpawnPoint.currentSpawn.position;
        }
        else
        {
            Debug.LogWarning("NO SPAWN POINT FOUND IN SCENE");
        }
    }
}

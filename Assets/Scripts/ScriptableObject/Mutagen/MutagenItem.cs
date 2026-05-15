using UnityEngine;

public class MutagenItem : MonoBehaviour, ICollectible{

    // aggiungere scriptableobject di riferimento!! (si fa il set di sprite, nome ecc)
    [Header("Xrotation offset on equip")]
    public float xRotationOffset;

    [Header("Data Reference")]
    public MutagenSO mutagenData;

    [Header("Drop details")]
    public bool pickedUp;
    [SerializeField] private float dropDuration = 0.25f;
    [SerializeField] private float colliderReactivateDelay = 1f;
    [SerializeField] private Transform shadow;
    public float initialZRotation;

    [Header("Idle animation details")]
    [SerializeField] private Transform visual;
    [SerializeField] private float floatAmplitude = 0.08f;
    [SerializeField] private float floatSpeed = 2.5f;
    private Vector3 idleStartPosition;
    private void Start() {
        initialZRotation = transform.eulerAngles.z;
        idleStartPosition = transform.position;
        if(visual == null) {
            visual = transform;
        }
    }

    private void Update() {

        // Idle floating animation SOLO quando l'arma non � equipaggiata
        if (!pickedUp) {

            float yOffset =
                Mathf.Sin(Time.time * floatSpeed)
                * floatAmplitude;

            visual.position =
                idleStartPosition +
                new Vector3(0f, yOffset, 0f);
        }
    }

    public void Collect(Player player) 
    {
        if (pickedUp || mutagenData == null) return;

        // Find the controller on the player
        MutagenController controller = player.GetComponent<MutagenController>();

        if (controller != null) 
        {
            // Pass the SO to the controller's list
            controller.EquipMutagen(mutagenData);
            
            // Visual/Logic cleanup
            pickedUp = true;
            if(shadow != null) shadow.gameObject.SetActive(false);
            if(GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            
            // Optionally destroy the item or disable it
            gameObject.SetActive(false); 
            Debug.Log($"Picked up and equipped: {mutagenData.mutagenName}");
        }
    }

    // Add this to handle the actual collision trigger
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player")) 
        {
            Player player = collision.GetComponent<Player>();
            if (player != null) 
            {
                Collect(player);
            }
        }
    }

    public void DropWeapon() {
        if (GetComponent<Collider2D>() != null) {
            GetComponent<Collider2D>().enabled = false; // disattivo collider se per caso attivo (lo riattivo dopo il drop)
        }

        ChooseDropDirection(Player.Instance.transform.position);
    }

    private void ChooseDropDirection(Vector2 origin, float dropDistance = .2f) {
        Vector2 randomDirection = Random.insideUnitCircle.normalized; // direzion casuale
        Vector2 dropPosition = origin + randomDirection * dropDistance;

        // animazione di drop
        StartCoroutine(
            AnimateDrop(dropPosition)
        );
    }

    private System.Collections.IEnumerator AnimateDrop(Vector2 targetPosition) {
        // resetto posizione
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, initialZRotation);
        // riabilito ombra
        if (shadow != null) {
            shadow.gameObject.SetActive(true);
        }
        Vector2 startPosition = transform.position;

        float elapsed = 0f;

        while (elapsed < dropDuration) {

            elapsed += Time.deltaTime;

            float t = elapsed / dropDuration;

            // Movimento smooth 
            transform.position = Vector2.Lerp(
                startPosition,
                targetPosition,
                t
            );

            yield return null;
        }

        transform.position = targetPosition;
        idleStartPosition = transform.position; // aggiorno idle position per animazione

        // Si aspetta prima di riattivare il collider
        yield return new WaitForSeconds(
            colliderReactivateDelay
        );

        Collider2D col = GetComponent<Collider2D>();

        if (col != null) {
            col.enabled = true;
        }

        pickedUp = false;
    }

}
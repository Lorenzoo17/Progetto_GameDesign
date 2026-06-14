using UnityEngine;

public class TransitionExitTrigger : MonoBehaviour
{
    [Header("Lista Sequenziale dei Livelli")]
    [Tooltip("Slot 0 = Basement 1, Slot 1 = Basement 2")]
    [SerializeField] private string[] levelSequence;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        // Recuperiamo il livello corrente dalla transizione
        int currentLevelIndex = TransitionLevelController.visitCount;
        Debug.Log($"[TRIGGER] Livello corrente (visitCount): {currentLevelIndex}");
        if (currentLevelIndex < 0) { 
            Debug.LogWarning($"[TRIGGER] visitCount è negativo ({currentLevelIndex}). Forzo a 0.");
            return;
        }

        // --- LA LOGICA DEL LOOP ---
        // Se la lista è vuota, blocchiamo per evitare crash totali
        if (levelSequence == null || levelSequence.Length == 0)
        {
            Debug.LogError("[TRIGGER] La lista delle scene è vuota nell'Inspector!");
            return;
        }

        // Se l'indice è maggiore o uguale alla lunghezza della lista (es. è 2, ma abbiamo solo slot 0 e 1)
        // forziamo l'indice a rimanere bloccato sull'ultimo slot disponibile (il secondo livello)
        if (currentLevelIndex >= levelSequence.Length)
        {
            currentLevelIndex = 1;
            Debug.Log($"[TRIGGER] Raggiunto la fine dei livelli creati. Forzo il loop sull'ultimo livello. Indice usato: {currentLevelIndex}");
        }
        // --------------------------

        hasTriggered = true;
        string nextSceneName = levelSequence[currentLevelIndex];

        Debug.Log($"[TRIGGER] Caricamento scena in corso: {nextSceneName}");

        Canvas mutagenCanvas = GameObject.Find("MutagenUICanva")?.GetComponent<Canvas>();
        if (mutagenCanvas != null)
        {
            mutagenCanvas.enabled = true;
        }

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.LoadNextScene(nextSceneName);
            
        }
        else
        {
            Debug.LogError("LevelLoader.Instance non trovato nella scena!");
        }
    }
}
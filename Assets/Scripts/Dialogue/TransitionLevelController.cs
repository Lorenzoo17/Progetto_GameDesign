using UnityEngine;

public class TransitionLevelController : MonoBehaviour
{
    // Partiamo da -1 (Stato iniziale dell'HUB)
    public static int visitCount = -1;

    [Header("Oggetti della scena")]
    [Tooltip("Trascina qui i barili IN ORDINE: slot 0 = primo barile, slot 1 = secondo, ecc.")]
    public SpriteRenderer[] barrelRenderers;

    [Header("Sprite specifici per ogni Barile")]
    public Sprite firstVisitSprite;  // Lo sprite per il primo barile (dopo Livello 1)
    public Sprite secondVisitSprite; // Lo sprite per il secondo barile (dopo Livello 2)

    private void Start()
    {

        Canvas mutagenCanvas = GameObject.Find("MutagenUICanva")?.GetComponent<Canvas>();
        if (mutagenCanvas != null)
        {
            mutagenCanvas.enabled = false;
        }

        if (LevelLoader.previousSceneName == "HUB")
        {
            visitCount = -1;
            Debug.Log("[TRANSITION] Rilevato arrivo dall'HUB. Il contatore della run è stato resettato a -1!");
        }
        // Incrementiamo il contatore appena entriamo nella transizione
        visitCount++;
        Debug.Log($"[PROGRESSIONE] Livelli completati finora: {visitCount}");

        // Cicliamo tutti i barili
        for (int i = 0; i < barrelRenderers.Length; i++)
        {
            if (barrelRenderers[i] == null) continue;

            // La magia matematica:
            // Se visitCount è 0 (Prima transizione): 'i < 0' è sempre FALSO -> Nessun barile cambia.
            // Se visitCount è 1 (Dopo Livello 1): 'i < 1' è VERO solo per i=0 -> Cambia il primo barile.
            // Se visitCount è 2 (Dopo Livello 2): 'i < 2' è VERO per i=0 e i=1 -> Cambiano primo e secondo.
            if (i < visitCount)
            {
                if (i == 0 && firstVisitSprite != null)
                {
                    barrelRenderers[i].sprite = firstVisitSprite;
                }
                else if (i == 1 && secondVisitSprite != null)
                {
                    barrelRenderers[i].sprite = secondVisitSprite;
                }
            }
        }
    }
}
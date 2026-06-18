using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TransitionLevelController : MonoBehaviour
{
    // Partiamo da -1 (Stato iniziale dell'HUB)
    public static int visitCount = -1;

    [Header("Oggetti della scena")]
    [Tooltip("Trascina qui i barili IN ORDINE: slot 0 = primo barile, slot 1 = secondo, ecc.")]
    public SpriteRenderer[] barrelRenderers;

    [Header("Sprite specifici per ogni Barile")]
    public Sprite firstVisitSprite;  
    public Sprite secondVisitSprite; 

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
                    AttivaLuceDalBarile(barrelRenderers[i]);

                }
                else if (i == 1 && secondVisitSprite != null)
                {
                    barrelRenderers[i].sprite = secondVisitSprite;
                    AttivaLuceDalBarile(barrelRenderers[i]);
                }
            }
        }
    }

    public void AttivaLuceDalBarile(SpriteRenderer mioSpriteRenderer)
    {
        // Controllo di sicurezza: se non hai passato nulla, esci per evitare crash
        if (mioSpriteRenderer == null) return;

        // 1. Prendiamo l'oggetto a cui è assegnato lo SpriteRenderer
        GameObject oggettoPadre = mioSpriteRenderer.gameObject;

        // 2. Cerchiamo lo script "Light2D" nei figli (anche se l'oggetto è spento)
        Light2D scriptLuce = oggettoPadre.GetComponentInChildren<Light2D>(true);

        if (scriptLuce != null)
        {
            // 3. ABBIAMO DUE OPZIONI PER ATTIVARLO (Scegli quella che fa al caso tuo):

            // OPZIONE A: Se vuoi attivare solo lo SCRIPT (lasciando l'oggetto figlio acceso)
            scriptLuce.enabled = true;

            // OPZIONE B: Se l'intero OGGETTO FIGLIO è disattivato nella Hierarchy e vuoi accenderlo
            // scriptLuce.gameObject.SetActive(true);

            Debug.Log($"[LUCE] Script Light2D trovato e attivato con successo su un figlio di: {oggettoPadre.name}");
        }
        else
        {
            Debug.LogWarning($"[LUCE] Attenzione: non ho trovato nessun figlio con lo script Light2D sotto l'oggetto: {oggettoPadre.name}");
        }
    }
}
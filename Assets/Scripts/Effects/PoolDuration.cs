using UnityEngine;
using System.Collections;

public class PoolDuration : MonoBehaviour 
{
    private Animator anim;

    private void Awake() 
    {
        // Recuperiamo l'animator all'inizio
        anim = GetComponent<Animator>();
    }

    // Questa funzione viene chiamata dal proiettile subito dopo l'Instantiate
    public void StartLifeCycle(float duration) 
    {
        StartCoroutine(LifeCycleRoutine(duration));
    }

    private IEnumerator LifeCycleRoutine(float duration) 
    {
        // 1. La pozza resta attiva per il tempo stabilito
        yield return new WaitForSeconds(duration);
        
        // 2. Cerchiamo di attivare il trigger di uscita
        if (anim != null) 
        {
            anim.SetTrigger("Dissolve");
        }

        // 3. Aspettiamo il brevissimo tempo dell'animazione (0.1s) e distruggiamo
        yield return new WaitForSeconds(0.5f); 
        //Debug.Log($"sono poolduration e voglio far chiudere la pozza.");
        Destroy(gameObject);
        //Debug.Log($"sono poolduration e ho distrutto la pozza.");
    }
}
using System.Collections;
using UnityEngine;

public class TrophyHandler : MonoBehaviour
{
    public void StartChallenge()
    {
        StartCoroutine(TrophyChallengeRoutine());
    }

    private IEnumerator TrophyChallengeRoutine()
    {
        
        yield return null;

        Debug.Log("[TrophyHandler] Inizio coroutine della sfida per il trofeo Fungus.");

        if (Player.Instance == null)
        {
            Debug.LogWarning("[TrophyHandler] Player.Instance non trovato! Impossibile avviare la sfida.");
            yield break;
        }

        
        HealthSystem playerHealth = Player.Instance.GetComponent<HealthSystem>();
        if (playerHealth == null) { 
            Debug.LogWarning("[TrophyHandler] HealthSystem del player non trovato! Impossibile avviare la sfida.");
        }

        float lastHealth = playerHealth != null ? playerHealth.CurrentHealth : 0;
        bool tookDamage = false;

        float maxDuration = 40f;
        float elapsedTime = 0f;

        Debug.Log("[CHALLENGE] Sfida avviata! Uccidi il Boss Fungus in meno di 40 secondi senza subire colpi.");

        
        while (elapsedTime < maxDuration)
        {
            Debug.Log($"[CHALLENGE] Tempo trascorso: {elapsedTime} secondi. Tempo massimo: {maxDuration} secondi.");
            if (playerHealth != null)
            {
                float currentHealth = playerHealth.CurrentHealth;

                
                if (currentHealth < lastHealth)
                {
                    tookDamage = true;
                    Debug.Log("[CHALLENGE] Il player è stato colpito! Trofeo fallito per questa run.");
                }

               
                lastHealth = currentHealth;
            }

            
            BossFungus boss = FindFirstObjectByType<BossFungus>();

            
            if (boss == null)
            {
               
                if (!tookDamage)
                {
                    TrophieManager.isFungusTrophieUnlocked = true;
                    Debug.Log("[CHALLENGE] ECCELLENTE! Boss sconfitto in tempo e senza subire danni. Trofeo sbloccato!");
                }
                else
                {
                    Debug.Log("[CHALLENGE] Il boss è morto in tempo, ma purtroppo hai subito danni durante la run.");
                }

                
                yield break;
            }

            
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        
        Debug.Log("[CHALLENGE] Tempo scaduto! Sono passati 40 secondi e il boss è ancora vivo.");
    }
}
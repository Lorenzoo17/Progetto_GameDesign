using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class TrophieManager : MonoBehaviour
{

    public static bool isFungusTrophieUnlocked = false;
    public static bool isRacitTrophieUnlocked = false;


    private bool isEnterOnHUB = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (isFungusTrophieUnlocked || isRacitTrophieUnlocked) {
            
            if (scene == "HUB" && !isEnterOnHUB)
            {
                isEnterOnHUB = true;
                Debug.Log("Entrato nell'HUB con almeno un trofeo sbloccato!");
                //spengo la porta chiusa per la stanza trofei
                GameObject stanzaObj = GameObject.Find("DoorTrophie");

                if (stanzaObj != null)
                {

                    TilemapRenderer tmRenderer = stanzaObj.GetComponent<TilemapRenderer>();
                    CompositeCollider2D tmCollider = stanzaObj.GetComponent<CompositeCollider2D>();

                    if (tmRenderer != null)
                    {
                        tmRenderer.enabled = false;
                        tmCollider.isTrigger = true; 

                    }
                    else { 
                        Debug.LogWarning("TilemapRenderer non trovato su DoorTrophie!");
                    }
                }
                //la apro
                stanzaObj = GameObject.Find("DoorTrophieOpen");

                if (stanzaObj != null)
                {

                    TilemapRenderer tmRenderer = stanzaObj.GetComponent<TilemapRenderer>();

                    if (tmRenderer != null)
                    {
                        tmRenderer.enabled = true;

                    }
                    else { 
                        Debug.LogWarning("TilemapRenderer non trovato su DoorTrophieOpen!");
                    }
                }

                //dialogo opzionale npc
                GameObject npcSpecifico = GameObject.Find("Mousdomoro");

                if (npcSpecifico != null)
                {

                    DialogueTrigger npcDialogue = npcSpecifico.GetComponent<DialogueTrigger>();

                    if (npcDialogue != null)
                    {

                        npcDialogue.isOptionalConditionMet = true;

                        Debug.Log($"[HUB] Condizione opzionale attivata per l'NPC: {npcSpecifico.name}");
                    }
                    else { 
                        Debug.LogWarning($"DialogueTrigger non trovato su {npcSpecifico.name}!");

                    }
                }

            }
            else if (scene != "HUB" && isEnterOnHUB)
            {
                isEnterOnHUB = false;
                Debug.LogWarning("Uscito dall'HUB senza trofei sbloccati, resetto la porta e la condizione opzionale!");
            }


        }
        
    }
}

using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellingStation : MonoBehaviour {
    [SerializeField] private GameObject[] itemsToSell;
    // per ora array di gameobject (tutti i prefab di tutti gli oggetti possibili)
    // poi magari gestirlo meglio, tramite ScriptableObjects, appena facciamo il merge dei vari lavori
    // anche perche gli oggetti dovranno essere 1 arma, 1 perk e 1 mutagene
    // e ad ognuno ci deve essere un prezzo specifico associato
    [SerializeField] private SellingSlot[] slots;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        SpawnItemsToSell();
    }

    private void SpawnItemsToSell() {
        if(itemsToSell == null || itemsToSell.Length == 0) return;
        if (slots == null || slots.Length == 0) return;

        List<GameObject> availableItems = new List<GameObject>(itemsToSell);
        // a questi availableItems, vanno tolti quelli gia' sbloccati, guardando il MetaProgressionManager!!

        // minimo tra numero di slot e numero di oggetti disponibili
        int itemsToSpawn = Mathf.Min(3, slots.Length, availableItems.Count);

        for (int i = 0; i < itemsToSpawn; i++) {
            int randomIndex = Random.Range(0, availableItems.Count);

            GameObject selectedItem = availableItems[randomIndex];

            Vector3 spawnPosition = slots[i].transform.position;
            if (slots[i].itemPositioningTransform != null) {
                spawnPosition = slots[i].itemPositioningTransform.position;
            }
            GameObject item = Instantiate(
                selectedItem,
                spawnPosition,
                Quaternion.identity
            );
            item.transform.SetParent(slots[i].transform);
            slots[i].SetUpSellingSlot(item, 100); // per ora prezzo fisso, poi dipendera' da SO

            availableItems.RemoveAt(randomIndex);
        }
    }
}

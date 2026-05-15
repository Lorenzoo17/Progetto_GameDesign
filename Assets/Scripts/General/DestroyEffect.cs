using System;
using UnityEngine;

public class DestroyEffect : MonoBehaviour 
{
    [SerializeField] private float delay = 0.5f; // Durata dell'animazione (es. 0.5 secondi)

    void Start()
    {
        // Distrugge l'oggetto dopo il tempo stabilito per non appesantire la gerarchia
        Destroy(gameObject, delay);
    }
}
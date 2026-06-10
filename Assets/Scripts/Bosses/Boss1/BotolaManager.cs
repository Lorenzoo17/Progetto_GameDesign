using UnityEngine;
using System.Collections.Generic;

public class BotolaManager : MonoBehaviour 
{
    public static BotolaManager Instance { get; private set; }
    public List<Transform> botole = new List<Transform>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() {
        if(botole.Count <= 0) {
            Debug.LogWarning("Botole non assegnate!");
        }
    }

    public Transform GetRandomBotola() 
    {
        if (botole.Count == 0) return null; 
        int index = Random.Range(0, botole.Count);
        return botole[index];
    }
}
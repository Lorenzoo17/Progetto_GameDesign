using UnityEngine;
using System.Collections.Generic;

public class PipeManager : MonoBehaviour
{
    public static PipeManager Instance { get; private set; }
    public List<Transform> tubi = new List<Transform>();
    public int lastUsedIndex = -1;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start() 
    {
        if (tubi.Count <= 0) {
            Debug.LogWarning("Tubi non assegnati!");
        }
    }

    public Transform GetRandomPipe() 
    {
        if (tubi.Count == 0) return null; 
        int index = Random.Range(0, tubi.Count);
        if (index != lastUsedIndex) {
            lastUsedIndex = index;
            return tubi[index];
        }
        else return GetRandomPipe();
    }
}

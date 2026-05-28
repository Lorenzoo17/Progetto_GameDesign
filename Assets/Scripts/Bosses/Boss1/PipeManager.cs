using UnityEngine;
using System.Collections.Generic;

public class PipeManager : MonoBehaviour
{
    // Usiamo una lista pubblica accessibile
    public List<Transform> tubi = new List<Transform>();

    private void Start() 
    {
        if (tubi.Count <= 0) {
            Debug.LogWarning("Tubi non assegnati!");
        }
    }

    public Transform GetRandomPipe() 
    {
        if (tubi.Count == 0) return null; // Nessun tubo disponibile
        int index = Random.Range(0, tubi.Count);
        return tubi[index];
    }
}

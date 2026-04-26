using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour {

    [SerializeField] private GameObject visual;

    public void SetClosed(bool closed) {
        visual.SetActive(closed);
    }
}

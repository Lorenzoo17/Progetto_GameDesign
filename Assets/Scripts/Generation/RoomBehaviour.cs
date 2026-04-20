using UnityEngine;

public class RoomBehaviour : MonoBehaviour {
    // 0 - Up 1 - Down 2 - Right 3 - Left
    public GameObject[] blocks;

    public void UpdateRoom(bool[] status) {
        for (int i = 0; i < status.Length; i++) {
            // se voglio attivare il passaggio allora disattivo il blocco
            blocks[i].SetActive(!status[i]);
        }
    }
}

using UnityEngine;

public class ChangeScene : MonoBehaviour {

    [SerializeField] private string nextSceneName;

    // Si passa a scena successiva tramite trigger
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.GetComponent<Player>() != null) {
            if(LevelLoader.Instance != null) {
                LevelLoader.Instance.LoadNextScene(nextSceneName);
            }
            else {
                Debug.LogWarning("Nella scena non è presente un LevelLoader!");
            }
        }
    }
}

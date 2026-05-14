using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    [SerializeField] private Animator anim;
    [SerializeField] private float transitionTime = 0.2f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void LoadNextScene(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadLevel(currentSceneName));
    }

    private System.Collections.IEnumerator LoadLevel(string sceneName)
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        Debug.Log($"Loading scene: {sceneName}");

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

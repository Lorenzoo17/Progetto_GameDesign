using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    [SerializeField] private Animator anim;
    [SerializeField] private float transitionTime = 1f;

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

    private System.Collections.IEnumerator LoadLevel(string sceneName)
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        Debug.Log($"Loading scene: {sceneName}");

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

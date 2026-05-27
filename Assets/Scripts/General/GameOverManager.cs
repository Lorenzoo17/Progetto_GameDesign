using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    
    [SerializeField] private GameObject gameOverPrefab;
    [SerializeField] private string hubSceneName = "HubScene";

    private GameObject currentUI;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("SHOW GAME OVER");

        currentUI = Instantiate(gameOverPrefab);

        currentUI.SetActive(true);
        currentUI.transform.SetAsLastSibling();
        currentUI.transform.localScale = Vector3.one;
        currentUI.transform.position = Vector3.zero;

        Button btn = currentUI.GetComponentInChildren<Button>();

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(GoToHub);
        }

        Time.timeScale = 0f;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = false;
        }
    }

    public void GoToHub()
    {
        Time.timeScale = 1f;
        isGameOver = false;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = true;
        }

        if (Player.Instance != null)
        {
            Player.Instance.DestroySelf();
        }

        if(MetaProgressionManager.Instance != null) {
            MetaProgressionManager.Instance.DungeonCoin = 0; // resetto dungeonCoin
        }

        SceneManager.LoadScene(hubSceneName);
    }
}
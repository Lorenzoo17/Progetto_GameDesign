using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private string hubSceneName = "HUB";
    [SerializeField] private string startingMenuSceneName = "MainMenu";

    private GameObject currentUI;
    private bool isPaused = false;
    private float previousTimeScale = 1f;

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

    private void Start()
    {
        // Il menu viene istanziato solo quando necessario
    }

    private void Update()
    {
        // Gestire ESC per chiudere il menu stats se aperto
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused && currentUI != null)
        {
            Transform statsPanel = currentUI.transform.Find("StatsPanel");
            if (statsPanel != null && statsPanel.gameObject.activeInHierarchy)
            {
                CloseStats();
            }
        }
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Fermiamo il tempo del gioco

        Debug.Log("Gioco in pausa");

        // Disabilitiamo gli input del player
        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = false;
        }

        // Istanziamo il menu di pausa se non esiste
        if (currentUI == null && pauseMenuPrefab != null)
        {
            currentUI = Instantiate(pauseMenuPrefab);
            currentUI.SetActive(true);
            currentUI.transform.SetAsLastSibling();
            currentUI.transform.localScale = Vector3.one;
            currentUI.transform.position = Vector3.zero;

            // Configuriamo i bottoni
            Button resumeButton = currentUI.transform.Find("PauseMenuPanel/ResumeButton")?.GetComponent<Button>();
            Button statsButton = currentUI.transform.Find("PauseMenuPanel/StatsButton")?.GetComponent<Button>();
            Button surrenderButton = currentUI.transform.Find("PauseMenuPanel/SurrenderButton")?.GetComponent<Button>();
            Button exitButton = currentUI.transform.Find("PauseMenuPanel/ExitButton")?.GetComponent<Button>();

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(Resume);
            }

            if (statsButton != null)
            {
                statsButton.onClick.RemoveAllListeners();
                statsButton.onClick.AddListener(OpenStats);
            }

            if (surrenderButton != null)
            {
                surrenderButton.onClick.RemoveAllListeners();
                surrenderButton.onClick.AddListener(Surrender);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(Exit);
            }
        }
        else if (currentUI != null)
        {
            currentUI.SetActive(true);
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale; // Riprendi il tempo del gioco

        if (currentUI != null)
            currentUI.SetActive(false);

        // Riabilitiamo gli input del player
        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = true;
        }

        Debug.Log("Gioco ripreso");
    }

    private void OpenStats()
    {
        if (currentUI == null) return;

        Transform pausePanel = currentUI.transform.Find("PauseMenuPanel");
        Transform statsPanel = currentUI.transform.Find("StatsPanel");

        if (statsPanel != null)
            statsPanel.gameObject.SetActive(true);

        if (pausePanel != null)
            pausePanel.gameObject.SetActive(false);

        Debug.Log("Menu statistiche aperto");
    }

    private void CloseStats()
    {
        if (currentUI == null) return;

        Transform pausePanel = currentUI.transform.Find("PauseMenuPanel");
        Transform statsPanel = currentUI.transform.Find("StatsPanel");

        if (statsPanel != null)
            statsPanel.gameObject.SetActive(false);

        if (pausePanel != null)
            pausePanel.gameObject.SetActive(true);

        Debug.Log("Menu statistiche chiuso");
    }

    private void Surrender()
    {
        // Ripristiniamo il tempo normale prima di cambiare scena
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("Abbandono della partita - Ritorno all'HUB");
        SceneManager.LoadScene(hubSceneName);
    }

    private void Exit()
    {
        // Ripristiniamo il tempo normale prima di cambiare scena
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log("Uscita dal gioco - Ritorno al menu iniziale");
        SceneManager.LoadScene(startingMenuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}

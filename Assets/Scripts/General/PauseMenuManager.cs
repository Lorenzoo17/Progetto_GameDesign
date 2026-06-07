using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenuPrefab;
    private string hubSceneName = "HUB";
    private string startingMenuSceneName = "MainMenu";

    private GameObject currentUI;
    private bool isPaused = false;
    private float previousTimeScale = 1f;

    [Header("Button Hover Settings")]
    [SerializeField] private float selectedButtonScale = 1.15f;

    [Header("Disabled Button Settings")]
    [SerializeField] private Color disabledTextColor = Color.gray;
    [SerializeField] private Color enabledTextColor = Color.white;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused && currentUI != null)
        {
            HandleCloseStats();
        }
    }

    // =========================
    // PAUSE
    // =========================
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Debug.Log("Gioco in pausa");

        if (InputManager.Instance != null)
            InputManager.Instance.inputEnabled = false;

        if (currentUI == null && pauseMenuPrefab != null)
        {
            currentUI = Instantiate(pauseMenuPrefab);
            currentUI.SetActive(true);

            currentUI.transform.SetAsLastSibling();
            currentUI.transform.localScale = Vector3.one;
            currentUI.transform.position = Vector3.zero;

            BindButtons();
        }
        else if (currentUI != null)
        {
            currentUI.SetActive(true);
        }
    }

    // =========================
    // BUTTON BINDING
    // =========================
    private void BindButtons()
    {
        Button resumeButton = currentUI.transform.Find("PauseMenuPanel/ResumeButton")?.GetComponent<Button>();
        Button statsButton = currentUI.transform.Find("PauseMenuPanel/StatsButton")?.GetComponent<Button>();
        Button surrenderButton = currentUI.transform.Find("PauseMenuPanel/SurrenderButton")?.GetComponent<Button>();
        Button exitButton = currentUI.transform.Find("PauseMenuPanel/ExitButton")?.GetComponent<Button>();
        Button goBackButton = currentUI.transform.Find("StatsPanel/GoBackButton")?.GetComponent<Button>();

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

        if (surrenderButton != null) {
            surrenderButton.onClick.RemoveAllListeners();

            bool canSurrender = SceneManager.GetActiveScene().name != hubSceneName;

            surrenderButton.interactable = canSurrender;

            TextMeshProUGUI text = surrenderButton.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.color = canSurrender ? enabledTextColor : disabledTextColor;

            if (canSurrender)
                surrenderButton.onClick.AddListener(Surrender);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(Exit);
        }

        // 🔥 GO BACK BUTTON (Stats -> Pause)
        if (goBackButton != null)
        {
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(HandleCloseStats);
        }

        ConfigureButtonVisual(resumeButton);
        ConfigureButtonVisual(statsButton);
        ConfigureButtonVisual(surrenderButton);
        ConfigureButtonVisual(exitButton);
        ConfigureButtonVisual(goBackButton);
    }

    // =========================
    // RESUME
    // =========================
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (currentUI != null)
            currentUI.SetActive(false);

        if (InputManager.Instance != null)
            InputManager.Instance.inputEnabled = true;

        Debug.Log("Gioco ripreso");
    }

    // =========================
    // STATS
    // =========================
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

    private void HandleCloseStats()
    {
        if (!isPaused || currentUI == null)
            return;

        Transform pausePanel = currentUI.transform.Find("PauseMenuPanel");
        Transform statsPanel = currentUI.transform.Find("StatsPanel");

        if (statsPanel != null && statsPanel.gameObject.activeInHierarchy)
        {
            if (statsPanel != null)
                statsPanel.gameObject.SetActive(false);

            if (pausePanel != null)
                pausePanel.gameObject.SetActive(true);

            Debug.Log("Stats chiuso");
        }
    }

    // =========================
    // SCENE ACTIONS
    // =========================
    private void Surrender()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (InputManager.Instance != null)
            InputManager.Instance.inputEnabled = true;

        Debug.Log("Abbandono della partita - Ritorno all'HUB");
        if(Player.Instance != null) {
            Player.Instance.DestroySelf(); // distruzione del player
        }
        if (MetaProgressionManager.Instance != null) {
            MetaProgressionManager.Instance.DungeonCoin = 0; // resetto monete dungeon
        }
        SceneManager.LoadScene(hubSceneName);
    }

    private void Exit()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (InputManager.Instance != null)
            InputManager.Instance.inputEnabled = true;

        Debug.Log("Uscita dal gioco - Ritorno al menu iniziale");
        if (Player.Instance != null) {
            Player.Instance.DestroySelf(); // distruzione del player
        }
        if(MetaProgressionManager.Instance != null) {
            MetaProgressionManager.Instance.DungeonCoin = 0; // resetto monete dungeon
        }
        SceneManager.LoadScene(startingMenuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    private void ConfigureButtonVisual(Button button) {
        if (button == null) return;

        Transform divider = button.transform.Find("Divider");

        Vector3 originalScale = button.transform.localScale;

        bool isPointerOver = false;
        bool isSelected = false;

        RefreshVisual();

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            isPointerOver = true;
            RefreshVisual();
        });

        AddEventTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            isPointerOver = false;
            RefreshVisual();
        });

        AddEventTrigger(trigger, EventTriggerType.Select, () =>
        {
            isSelected = true;
            RefreshVisual();
        });

        AddEventTrigger(trigger, EventTriggerType.Deselect, () =>
        {
            isSelected = false;
            RefreshVisual();
        });

        button.onClick.AddListener(() =>
        {
            isPointerOver = false;
            isSelected = false;
            RefreshVisual();
        });

        void RefreshVisual() {
            bool active = button.interactable && (isPointerOver || isSelected);

            if (divider != null)
                divider.gameObject.SetActive(active);

            button.transform.localScale = active
                ? originalScale * selectedButtonScale
                : originalScale;
        }
    }
    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityAction action) {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;

        entry.callback.AddListener((eventData) =>
        {
            action.Invoke();
        });

        trigger.triggers.Add(entry);
    }
}
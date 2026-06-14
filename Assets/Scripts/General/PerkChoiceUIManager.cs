using UnityEngine;
using UnityEngine.SceneManagement;

public class PerkChoiceUIManager : MonoBehaviour
{
    public static PerkChoiceUIManager Instance { get; private set; }

    [SerializeField] private GameObject perkChoiceUIPrefab;
    private GameObject currentUI;
    private PerkChoiceUIController currentController;

    private void Awake()
    {
        Debug.Log("🔧 PerkChoiceUIManager.Awake()");

        if (Instance != null)
        {
            Debug.Log("⚠️ PerkChoiceUIManager già esiste, distruggo questo");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("✅ PerkChoiceUIManager inizializzato");
    }

    private void OnEnable()
    {
        Debug.Log("📌 PerkChoiceUIManager.OnEnable()");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Debug.Log("📌 PerkChoiceUIManager.OnDisable()");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Scena caricata: {scene.name}");

        // Distruggi l'UI vecchia quando cambi scena
        if (currentUI != null)
        {
            Debug.Log("🗑️ Distruggo UI precedente");
            Destroy(currentUI);
            currentUI = null;
            currentController = null;
        }
    }

    public void ShowPerkChoices(PerkPair[] perkPairs, PerkPickup pickup)
    {
        if (perkChoiceUIPrefab == null)
        {
            Debug.LogError("❌ PerkChoiceUIManager: prefab non assegnato!");
            return;
        }

        Debug.Log("🎯 ShowPerkChoices chiamato");

        // Istanzia il UI se non esiste
        if (currentUI == null)
        {
            currentUI = Instantiate(perkChoiceUIPrefab);
            currentUI.transform.localScale = Vector3.one;
            currentUI.SetActive(true);
            currentUI.transform.SetAsLastSibling();
            currentController = currentUI.GetComponentInChildren<PerkChoiceUIController>(includeInactive: true);

            if (currentController == null)
            {
                Debug.LogError("❌ PerkChoiceUIController non trovato!");
                return;
            }


            // ✅ IMPORTANTE: Attiva il Panel/uiRoot subito (non è automatico)
            Transform perkChoicePanel = currentUI.transform.Find("PerkChoicePanel");
            if (perkChoicePanel != null)
            {
                perkChoicePanel.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("♻️ currentUI esiste già, riuso");
        }

        // Mostra le scelte
        Debug.Log("📋 Chiamo ShowChoices...");
        if (currentController != null)
        {
            currentController.ShowChoices(perkPairs, pickup);
        }
    }

    public void HideChoices()
    {
        Debug.Log("👻 HideChoices chiamato");

        if (currentController != null)
        {
            currentController.HideChoices();
            Debug.Log("✅ HideChoices completato");
        }
        else
        {
            Debug.LogWarning("⚠️ currentController è null");
        }
    }
}

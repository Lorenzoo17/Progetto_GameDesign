using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class MutagenSlotSelectorManager : MonoBehaviour
{
    public static MutagenSlotSelectorManager Instance { get; private set; }

    [SerializeField] private GameObject slotSelectorPrefab;
    private GameObject currentUI;
    private MutagenSO pendingMutagen;
    private MutagenItem pendingMutagenItem;
    private float previousTimeScale = 1f;
    private bool isSelectingSlot = false;

    //[Header("Button Hover Settings")]
    //SerializeField] private float selectedButtonScale = 1.15f;

    private MutagenController currentMutagenController;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (currentMutagenController != null) {
            currentMutagenController.OnRequestSlotSelection -= ShowSlotSelector;
        }
    }

    private void Start() {
        RebindToPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        RebindToPlayer();
    }

    private void RebindToPlayer() {
        if (currentMutagenController != null) {
            currentMutagenController.OnRequestSlotSelection -= ShowSlotSelector;
            currentMutagenController = null;
        }

        Player player = FindObjectOfType<Player>();

        if (player == null)
            return;

        currentMutagenController = player.GetComponent<MutagenController>();

        if (currentMutagenController != null) {
            currentMutagenController.OnRequestSlotSelection -= ShowSlotSelector;
            currentMutagenController.OnRequestSlotSelection += ShowSlotSelector;

            Debug.Log("MutagenSlotSelectorManager collegato al nuovo Player");
        }
    }

    public void ShowSlotSelector(MutagenSO mutagen)
    {
        if (isSelectingSlot || mutagen == null) return;

        isSelectingSlot = true;
        pendingMutagen = mutagen;

        // Troviamo il MutagenItem che ha richiesto la selezione
        MutagenItem[] mutagenItems = FindObjectsOfType<MutagenItem>();
        foreach (MutagenItem item in mutagenItems)
        {
            if (item.mutagenData == mutagen)
            {
                pendingMutagenItem = item;
                break;
            }
        }
        Debug.Log($"ShowSlotSelector: pendingMutagenItem found = {pendingMutagenItem != null}");

        // Mettiamo in pausa il gioco
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Disabilitiamo gli input del player
        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = false;
        }

        // Istanziamo il UI se non esiste
        if (currentUI == null && slotSelectorPrefab != null)
        {
            currentUI = Instantiate(slotSelectorPrefab);
            currentUI.SetActive(true);
            currentUI.transform.SetAsLastSibling();
            currentUI.transform.localScale = Vector3.one;
            currentUI.transform.position = Vector3.zero;

            // Assicuriamo che il Canvas abbia il GraphicRaycaster
            Canvas canvas = currentUI.GetComponent<Canvas>();
            if (canvas != null && currentUI.GetComponent<GraphicRaycaster>() == null)
            {
                currentUI.AddComponent<GraphicRaycaster>();
                Debug.Log("GraphicRaycaster aggiunto al Canvas");
            }

            SetupUI();
        }
        else if (currentUI != null)
        {
            currentUI.SetActive(true);
            SetupUI();
        }

        Debug.Log("Slot Selector opened");
    }

    private void SetupUI()
    {
        if (currentUI == null) return;

        // Mostriamo le info del mutagen item
        TextMeshProUGUI mutagenNameText = currentUI.transform.Find("MutagenInfoPanel/MutagenNameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI mutagenDescText = currentUI.transform.Find("MutagenInfoPanel/MutagenDescText")?.GetComponent<TextMeshProUGUI>();
        Image mutagenIcon = currentUI.transform.Find("MutagenInfoPanel/MutagenIcon")?.GetComponent<Image>();

        if (mutagenNameText != null && pendingMutagen != null)
        {
            mutagenNameText.text = pendingMutagen.mutagenName;
        }

        if (mutagenDescText != null && pendingMutagen != null)
        {
            mutagenDescText.text = pendingMutagen.description + $"\n\nMana Cost: {pendingMutagen.manaCost}";
        }

        if (mutagenIcon != null && pendingMutagen != null)
        {
            mutagenIcon.sprite = pendingMutagen.icon;
            mutagenIcon.preserveAspect = true;
        }

        // Configuriamo i bottoni degli slot
        ConfigureSlotButton(0, "Slot1Button");
        ConfigureSlotButton(1, "Slot2Button");

        // Configuriamo il bottone di annullamento
        Button cancelButton = currentUI.transform.Find("CancelButton")?.GetComponent<Button>();
        if (cancelButton != null) {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelSelection);

            ConfigureButtonVisual(cancelButton);
        }
    }

    private void ConfigureSlotButton(int slotIndex, string buttonName)
    {
        Transform slotTransform = currentUI.transform.Find(buttonName);
        if (slotTransform == null)
        {
            Debug.LogError($"Button {buttonName} not found in UI!");
            return;
        }

        Button slotButton = slotTransform.GetComponent<Button>();
        if (slotButton == null)
        {
            Debug.LogError($"Button component not found on {buttonName}!");
            return;
        }

        // Mostriamo le info dello slot
        MutagenSO equippedMutagen = GetEquippedMutagen(slotIndex);
        TextMeshProUGUI slotText = slotTransform.Find("SlotText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI slotInfoText = slotTransform.Find("SlotInfoText")?.GetComponent<TextMeshProUGUI>();

        if (slotText != null)
        {
            slotText.text = $"SLOT {slotIndex + 1}";
        }

        if (slotInfoText != null)
        {
            if (equippedMutagen != null)
            {
                slotInfoText.text = $"{equippedMutagen.mutagenName}\n{equippedMutagen.description} \nMana: {equippedMutagen.manaCost}";
            }
            else
            {
                slotInfoText.text = "No Mutagen Equipped";
            }
        }

        // Configuriamo il click
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => SelectSlot(slotIndex));

        // Divider + scale su hover/selezione
        ConfigureButtonVisual(slotButton);

        Debug.Log($"Button {buttonName} configured successfully");
    }

    private void SelectSlot(int slotIndex)
    {
        Debug.Log($"SelectSlot called: slotIndex={slotIndex}, pendingMutagen={pendingMutagen}, pendingMutagenItem={pendingMutagenItem}");

        if (pendingMutagen == null || pendingMutagenItem == null)
        {
            Debug.LogError("Cannot equip: pendingMutagen or pendingMutagenItem is null!");
            return;
        }

        Debug.Log($"Equipaggiando {pendingMutagen.mutagenName} nello slot {slotIndex}");
        // Equipaggiamo il mutagen nello slot scelto
        MutagenController controller = FindObjectOfType<MutagenController>();
        if (controller != null)
        {
            controller.EquipMutagen(pendingMutagen, slotIndex);
        }
            
        

        CloseSlotSelector();

    }

    private void CancelSelection()
    {
        CloseSlotSelector();
    }

    private void CloseSlotSelector()
    {
        isSelectingSlot = false;

        if (currentUI != null)
        {
            currentUI.SetActive(false);
        }

        if (pendingMutagenItem != null) {
            Destroy(pendingMutagenItem.gameObject);
        }

        // Riprendi il gioco
        Time.timeScale = previousTimeScale;

        // Riabilita gli input del player
        if (InputManager.Instance != null)
        {
            InputManager.Instance.inputEnabled = true;
        }


        pendingMutagen = null;
        pendingMutagenItem = null;

        Debug.Log("Slot Selector closed");
    }

    private MutagenSO GetEquippedMutagen(int slotIndex)
    {
        Player player = FindObjectOfType<Player>();
        if (player == null) return null;

        MutagenController controller = player.GetComponent<MutagenController>();
        if (controller == null) return null;

        return controller.GetEquippedMutagenBySlot(slotIndex);
    }

    public bool IsSelectingSlot()
    {
        return isSelectingSlot;
    }

    private void ConfigureButtonVisual(Button button) {
        if (button == null) return;

        Transform divider = button.transform.Find("Divider");

        // Nel caso nel prefab lo hai chiamato "divider" minuscolo
        if (divider == null)
            divider = button.transform.Find("divider");

        Vector3 originalScale = button.transform.localScale;

        bool isPointerOver = false;
        bool isSelected = false;

        if (divider != null)
            divider.gameObject.SetActive(false);

        button.transform.localScale = originalScale;

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            if (!button.interactable) return;

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
            if (!button.interactable) return;

            isSelected = true;
            RefreshVisual();
        });

        AddEventTrigger(trigger, EventTriggerType.Deselect, () =>
        {
            isSelected = false;
            RefreshVisual();
        });

        void RefreshVisual() {
            bool active = button.interactable && (isPointerOver || isSelected);

            if (divider != null)
                divider.gameObject.SetActive(active);
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

    public void ForceReset() {
        isSelectingSlot = false;

        if (currentUI != null) {
            Destroy(currentUI);
            currentUI = null;
        }

        pendingMutagen = null;
        pendingMutagenItem = null;

        Time.timeScale = 1f;

        if (InputManager.Instance != null) {
            InputManager.Instance.inputEnabled = true;
        }

        if (currentMutagenController != null) {
            currentMutagenController.OnRequestSlotSelection -= ShowSlotSelector;
            currentMutagenController = null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MutagenSlotSelectorManager : MonoBehaviour
{
    public static MutagenSlotSelectorManager Instance { get; private set; }

    [SerializeField] private GameObject slotSelectorPrefab;
    private GameObject currentUI;
    private MutagenSO pendingMutagen;
    private MutagenItem pendingMutagenItem;
    private float previousTimeScale = 1f;
    private bool isSelectingSlot = false;

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
        // Ascolta l'evento dal MutagenController
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            MutagenController mutagenController = player.GetComponent<MutagenController>();
            if (mutagenController != null)
            {
                mutagenController.OnRequestSlotSelection += ShowSlotSelector;
            }
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
        }

        // Configuriamo i bottoni degli slot
        ConfigureSlotButton(0, "Slot1Button");
        ConfigureSlotButton(1, "Slot2Button");

        // Configuriamo il bottone di annullamento
        Button cancelButton = currentUI.transform.Find("CancelButton")?.GetComponent<Button>();
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelSelection);
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
            
        Destroy(pendingMutagenItem.gameObject);

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
}

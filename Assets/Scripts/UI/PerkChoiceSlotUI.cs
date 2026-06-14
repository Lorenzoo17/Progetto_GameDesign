using UnityEngine;
using UnityEngine.UI;
using TMPro;  // ✅ Aggiungi questo

public class PerkChoiceSlotUI : MonoBehaviour
{
    [Header("Positive Perk")]
    [SerializeField] private Image positiveIcon;
    [SerializeField] private TextMeshProUGUI positiveDescription;  // ✅ Cambia in TMP

    [Header("Negative Perk")]
    [SerializeField] private Image negativeIcon;
    [SerializeField] private TextMeshProUGUI negativeDescription;  // ✅ Cambia in TMP

    [Header("Button")]
    [SerializeField] private Button selectButton;

    private PerkPair _currentPair;
    private int _slotIndex;
    private System.Action<int> _onSelected;

    public void SetUp(PerkPair pair, int slotIndex, System.Action<int> onSelected)
    {
        _currentPair = pair;
        _slotIndex = slotIndex;
        _onSelected = onSelected;

        if (pair == null) return;

        // Positive perk
        if (pair.positive != null)
        {
            positiveIcon.sprite = pair.positive.icon;
            positiveDescription.text = pair.positive.Description();  // ✅ Funziona uguale
        }

        // Negative perk
        if (pair.negative != null)
        {
            negativeIcon.sprite = pair.negative.icon;
            negativeDescription.text = pair.negative.Description();  // ✅ Funziona uguale
        }

        selectButton.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        _onSelected?.Invoke(_slotIndex);
    }

    private void OnDestroy()
    {
        selectButton.onClick.RemoveListener(OnSelect);
    }
}

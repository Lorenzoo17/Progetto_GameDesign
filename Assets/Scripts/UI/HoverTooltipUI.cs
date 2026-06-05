using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTooltipUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [TextArea] public string description;
    public string stats;

    public string itemName;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.Instance.Show(
            itemName,
            description,
            stats,
            GetComponent<RectTransform>()
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }
}
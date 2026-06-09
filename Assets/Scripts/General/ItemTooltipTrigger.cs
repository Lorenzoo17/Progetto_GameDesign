using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private IDescribable describable;

    public void SetSource(IDescribable source)
    {
        describable = source;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipUI.Instance == null)
        {
            Debug.LogError("TooltipUI instance not found in the scene.");
            return;
        }

        if (describable == null)
        {
            Debug.LogError("Describable source is null.");
            return;
        }

        TooltipUI.Instance.Show(describable.Description());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }

}
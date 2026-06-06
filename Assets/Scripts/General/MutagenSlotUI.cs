using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MutagenSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    [SerializeField] private TMP_Text keyText;

    [SerializeField] private GameObject activeBorder;

    public void SetSlot(MutagenSO mutagen,bool active)
    {

        if (mutagen != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = mutagen.icon;
            iconImage.preserveAspect = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        activeBorder.SetActive(active);
    }
}
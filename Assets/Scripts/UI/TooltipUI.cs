using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("UI")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Canvas canvas;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;

    private RectTransform canvasRect;

    [Header("Offset")]
    [SerializeField] private Vector2 offset = new Vector2(30, 0);

    private void Awake()
    {
        Instance = this;
        canvasRect = canvas.GetComponent<RectTransform>();
        Hide();
    }

    public void Show(string name, string description, string stats, RectTransform target)
    {
        root.gameObject.SetActive(true);

        nameText.text = name;
        descriptionText.text = description;
        statsText.text = stats;

        PositionTooltip(target);
    }

    public void Hide()
    {
        root.gameObject.SetActive(false);
    }

    private void PositionTooltip(RectTransform target)
    {
        if (target == null) return;

        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        // uso il lato destro dell’oggetto
        Vector3 worldPos = worldCorners[3]; // top-right corner

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera,
            worldPos
        );

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out anchoredPos
        );

        // offset base (a destra)
        anchoredPos += offset;

        // clamp ai bordi schermo
        Vector2 size = root.sizeDelta;

        Vector2 min = canvasRect.rect.min + size * 0.5f;
        Vector2 max = canvasRect.rect.max - size * 0.5f;

        anchoredPos.x = Mathf.Clamp(anchoredPos.x, min.x, max.x);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, min.y, max.y);

        root.anchoredPosition = anchoredPos;
    }
}
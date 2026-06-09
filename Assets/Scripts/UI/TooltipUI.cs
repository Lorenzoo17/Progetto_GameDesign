using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private Canvas canvas;

    private void Awake()
    {
        Instance = this;

        canvas = GetComponentInParent<Canvas>();

        Hide();
    }

    private void Update()
    {
        if (root.gameObject.activeSelf)
            FollowMouse();
    }

    public void Show(string description)
    {
        descriptionText.text = description;

        root.gameObject.SetActive(true);
    }

    public void Hide()
    {
        root.gameObject.SetActive(false);
    }

    private void FollowMouse()
    {
        root.position = Input.mousePosition;
    }
}
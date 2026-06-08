using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour,
    ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler,
    IPointerExitHandler {
    private RectTransform rectTransform;
    private Button button;

    [SerializeField] private GameObject selectionDivider;

    [Header("Selected Settings")]
    [SerializeField] private float selectedScale = 1.15f;

    private Vector3 originalScale;

    private bool isSelected = false;
    private bool isPointerOver = false;

    [SerializeField] private string hubSceneName;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        originalScale = rectTransform.localScale;

        if (selectionDivider != null)
            selectionDivider.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) {
        if (!CanInteract()) {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        isSelected = true;
        RefreshVisual();
    }

    public void OnDeselect(BaseEventData eventData) {
        isSelected = false;
        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!CanInteract()) return;

        isPointerOver = true;

        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        RefreshVisual();
    }

    public void OnPointerExit(PointerEventData eventData) {
        isPointerOver = false;
        RefreshVisual();
    }

    private bool CanInteract() {
        return button != null && button.interactable;
    }

    private bool wasHighlighted = false;

    private void RefreshVisual() {
        bool active = CanInteract() && (isSelected || isPointerOver);

        if (active && !wasHighlighted)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D(SoundID.UIHover, .08f);
            }
            
        }

        wasHighlighted = active;

        rectTransform.localScale = active
            ? originalScale * selectedScale
            : originalScale;

        if (selectionDivider != null) {
            selectionDivider.SetActive(active);
        }
    }

    public void LoadScene() {
        if(LevelLoader.Instance != null) {
            LevelLoader.Instance.LoadNextScene(hubSceneName);
        }
    }

    public void ExitGame() {
        Application.Quit();
    }
}
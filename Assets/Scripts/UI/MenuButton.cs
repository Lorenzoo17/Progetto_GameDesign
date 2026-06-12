using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour,
    ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler {
    private RectTransform rectTransform;
    private Button button;

    [SerializeField] private GameObject selectionDivider;

    [Header("Selected Settings")]
    [SerializeField] private float selectedScale = 1.15f;

    [SerializeField] private string hubSceneName;

    private Vector3 originalScale;

    private bool isSelected = false;
    private bool wasHighlighted = false;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();

        originalScale = rectTransform.localScale;

        ForceResetVisual();
    }

    private void OnDisable() {
        ForceResetVisual();
    }

    public void OnSelect(BaseEventData eventData) {
        if (!CanInteract()) {
            if (EventSystem.current != null)
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

        if (EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    private bool CanInteract() {
        return button != null && button.interactable;
    }

    public void ForceResetVisual() {
        isSelected = false;
        wasHighlighted = false;

        if (rectTransform != null)
            rectTransform.localScale = originalScale;

        if (selectionDivider != null)
            selectionDivider.SetActive(false);
    }

    private void RefreshVisual() {
        bool active = CanInteract() && isSelected;

        if (active && !wasHighlighted) {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.PlaySound2D(SoundID.UIHover, .08f);
            }
        }

        wasHighlighted = active;

        if (rectTransform != null) {
            rectTransform.localScale = active
                ? originalScale * selectedScale
                : originalScale;
        }

        if (selectionDivider != null) {
            selectionDivider.SetActive(active);
        }
    }

    public void LoadScene() {
        if (LevelLoader.Instance != null) {
            LevelLoader.Instance.LoadNextScene(hubSceneName);
        }
    }

    public void ExitGame() {
        Application.Quit();
    }
}
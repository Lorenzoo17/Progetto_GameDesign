using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenuUI : MonoBehaviour {
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Quality Toggles")]
    [SerializeField] private Toggle lowQualityToggle;
    [SerializeField] private Toggle mediumQualityToggle;
    [SerializeField] private Toggle highQualityToggle;

    [Header("First Selected")]
    [SerializeField] private GameObject firstMainMenuButton;
    [SerializeField] private GameObject firstOptionsButton;

    private const string MasterVolumeKey = "MasterVolume";
    private const string QualityLevelKey = "QualityLevel";

    private void Start() {
        SetupVolume();
        SetupQuality();

        ShowMainMenu();
    }

    private void SetupVolume() {
        float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        AudioListener.volume = savedVolume;

        if (masterVolumeSlider != null) {
            masterVolumeSlider.value = savedVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
    }

    private void SetupQuality() {
        int savedQuality = PlayerPrefs.GetInt(QualityLevelKey, 1);
        savedQuality = Mathf.Clamp(savedQuality, 0, 2);

        ApplyQuality(savedQuality, false);

        if (lowQualityToggle != null)
            lowQualityToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) ApplyQuality(0, true);
            });

        if (mediumQualityToggle != null)
            mediumQualityToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) ApplyQuality(1, true);
            });

        if (highQualityToggle != null)
            highQualityToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) ApplyQuality(2, true);
            });
    }

    private void ApplyQuality(int qualityIndex, bool save) {
        int unityQualityIndex = GetUnityQualityIndex(qualityIndex);

        QualitySettings.SetQualityLevel(unityQualityIndex, true);

        if (lowQualityToggle != null)
            lowQualityToggle.SetIsOnWithoutNotify(qualityIndex == 0);

        if (mediumQualityToggle != null)
            mediumQualityToggle.SetIsOnWithoutNotify(qualityIndex == 1);

        if (highQualityToggle != null)
            highQualityToggle.SetIsOnWithoutNotify(qualityIndex == 2);

        if (save) {
            PlayerPrefs.SetInt(QualityLevelKey, qualityIndex);
            PlayerPrefs.Save();
        }
    }

    private int GetUnityQualityIndex(int simpleQualityIndex) {
        int maxIndex = QualitySettings.names.Length - 1;

        if (maxIndex <= 0)
            return 0;

        switch (simpleQualityIndex) {
            case 0:
                return 0; // Low

            case 1:
                return Mathf.RoundToInt(maxIndex * 0.5f); // Medium

            case 2:
                return maxIndex; // High

            default:
                return Mathf.RoundToInt(maxIndex * 0.5f);
        }
    }

    public void SetMasterVolume(float volume) {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void OpenOptions() {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);

        SelectButton(firstOptionsButton);
    }

    public void ShowMainMenu() {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        SelectButton(firstMainMenuButton);
    }

    private void SelectButton(GameObject button) {
        if (EventSystem.current == null || button == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
}
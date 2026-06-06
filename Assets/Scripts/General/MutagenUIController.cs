using UnityEngine;
using UnityEngine.SceneManagement;

public class MutagenUIController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject gameplayUI;

    [Header("Slots")]
    [SerializeField] private MutagenSlotUI firstSlot;
    [SerializeField] private MutagenSlotUI secondSlot;

    private MutagenController mutagenController;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();
    }

    private void Start()
    {
        FindPlayer();
        RefreshUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        RefreshUI();
    }

    private void FindPlayer()
    {
        Unsubscribe();

        Player player = FindFirstObjectByType<Player>();

        if (player != null)
        {
            mutagenController = player.GetComponent<MutagenController>();
            Subscribe();
        }
    }

    private void Subscribe()
    {
        if (mutagenController != null)
        {
            mutagenController.OnMutagenStateChanged += RefreshUI;
        }
    }

    private void Unsubscribe()
    {
        if (mutagenController != null)
        {
            mutagenController.OnMutagenStateChanged -= RefreshUI;
        }
    }


    public void RefreshUI()
    {
        if (mutagenController == null)
            return;

        MutagenSO first = mutagenController.GetEquippedMutagenBySlot(0);
        MutagenSO second = mutagenController.GetEquippedMutagenBySlot(1);

        firstSlot.SetSlot(
            first,
            first != null && mutagenController.IsMutagenActive(first)
        );

        secondSlot.SetSlot(
            second,
            second != null && mutagenController.IsMutagenActive(second)
        );
    }
}   
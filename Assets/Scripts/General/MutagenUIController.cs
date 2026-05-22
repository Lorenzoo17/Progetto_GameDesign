using UnityEngine;
using UnityEngine.SceneManagement;

public class MutagenUIController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject gameplayUI;

    [Header("Slots")]
    [SerializeField] private MutagenSlotUI headSlot;

    [SerializeField] private MutagenSlotUI bodySlot;

    [SerializeField] private MutagenSlotUI pawsSlot;

    private MutagenController mutagenController;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshVisibility();
        FindPlayer();
        RefreshUI();
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        RefreshVisibility();

        FindPlayer();

        RefreshUI();
    }

    private void FindPlayer()
    {
        Player player =
            FindFirstObjectByType<Player>();

        if (player != null)
        {
            mutagenController =
                player.GetComponent<MutagenController>();
        }
    }

    private void RefreshVisibility()
    {
        bool showUI =
            SceneManager.GetActiveScene().name
            != "HubScene";

        gameplayUI.SetActive(showUI);
    }

    public void RefreshUI()
    {
        if (mutagenController == null)
            return;

        MutagenSO head =
            mutagenController.GetEquippedMutagenByPart(
                MutagenBodyPart.Head);

        MutagenSO body =
            mutagenController.GetEquippedMutagenByPart(
                MutagenBodyPart.Body);

        MutagenSO paws =
            mutagenController.GetEquippedMutagenByPart(
                MutagenBodyPart.Paws);

        headSlot.SetSlot(
            head,
            head != null &&
            mutagenController.IsMutagenActive(head));

        bodySlot.SetSlot(
            body,
            body != null &&
            mutagenController.IsMutagenActive(body));

        pawsSlot.SetSlot(
            paws,
            paws != null &&
            mutagenController.IsMutagenActive(paws));
    }
}
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

        MutagenSO head = mutagenController.GetEquippedMutagenByPart(MutagenBodyPart.Head);
        MutagenSO body = mutagenController.GetEquippedMutagenByPart(MutagenBodyPart.Body);
        MutagenSO paws = mutagenController.GetEquippedMutagenByPart(MutagenBodyPart.Paws);

        headSlot.SetSlot(
            head,
            head != null && mutagenController.IsMutagenActive(head)
        );

        bodySlot.SetSlot(
            body,
            body != null && mutagenController.IsMutagenActive(body)
        );

        pawsSlot.SetSlot(
            paws,
            paws != null && mutagenController.IsMutagenActive(paws)
        );
    }
}   
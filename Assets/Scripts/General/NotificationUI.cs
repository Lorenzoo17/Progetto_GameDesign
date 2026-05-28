using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 2f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
        messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(displayTime);

        messageText.gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;
    [Header("Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private List<Image> hearts = new List<Image>();

    private PlayerHealth playerHealth;

    private void Start() {
        if (Player.Instance == null) return;

        playerHealth = Player.Instance.playerHealth;

        if (playerHealth == null) return;

        CreateHearts();
        UpdateHearts();

        playerHealth.OnHealthChanged += PlayerHealth_OnHealthChanged;
    }

    private void OnDestroy() {
        if (playerHealth != null) {
            playerHealth.OnHealthChanged -= PlayerHealth_OnHealthChanged;
        }
    }

    private void PlayerHealth_OnHealthChanged(object sender, System.EventArgs e)
    {
        int requiredHearts = Mathf.CeilToInt(playerHealth.maxHealthUnits / 2f);

        if (requiredHearts != hearts.Count) {
            CreateHearts();
        }

        UpdateHearts();
    }

    private void CreateHearts()
    {
        // Clear old hearts if needed
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        hearts.Clear();

        int totalHearts = Mathf.CeilToInt(Player.Instance.playerHealth.maxHealthUnits / 2f);

        for (int i = 0; i < totalHearts; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartsContainer);

            Image heartImage = heartObj.GetComponent<Image>();

            hearts.Add(heartImage);

        }
    }

    private void createHeart()
    {
        if (Player.Instance.playerHealth.maxHealthUnits % 2 == 0)
        {
            // Aggiungi un cuore pieno
            GameObject heartObj = Instantiate(heartPrefab, heartsContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = fullHeartSprite;
            hearts.Add(heartImage);
        }
        else
        {
            // Aggiungi un cuore mezzo pieno
            GameObject heartObj = Instantiate(heartPrefab, heartsContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = halfHeartSprite;
            hearts.Add(heartImage);
        }
    }

    private void UpdateHearts()
    {
        int healthUnits = Player.Instance.playerHealth.currentHealthUnits;

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartHealth = healthUnits - (i * 2);

            if (heartHealth >= 2)
            {
                hearts[i].sprite = fullHeartSprite;
            }
            else if (heartHealth == 1)
            {
                hearts[i].sprite = halfHeartSprite;
            }
            else
            {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}

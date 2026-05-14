using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManaBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject manaPrefab;
    [SerializeField] private Transform manaContainer;

    [Header("Sprites")]
    [SerializeField] private Sprite fullManaSprite;
    [SerializeField] private Sprite emptyManaSprite;

    private List<Image> manaImages = new();

    private PlayerMana playerMana;

    private void Start()
    {
        if (Player.Instance == null)
            return;

        playerMana = Player.Instance.GetComponent<PlayerMana>();

        if (playerMana == null)
            return;

        CreateManaUI();

        UpdateManaUI();

        playerMana.OnManaChanged += PlayerMana_OnManaChanged;
    }

    private void OnDestroy()
    {
        if (playerMana != null)
        {
            playerMana.OnManaChanged -= PlayerMana_OnManaChanged;
        }
    }

    private void PlayerMana_OnManaChanged(object sender, System.EventArgs e)
    {
        if (manaImages.Count != playerMana.GetMaxMana())
        {
            CreateManaUI();
        }
        UpdateManaUI();
    }

    private void CreateManaUI()
    {
        foreach (Transform child in manaContainer)
        {
            Destroy(child.gameObject);
        }

        manaImages.Clear();

        int maxMana = playerMana.GetMaxMana();

        for (int i = 0; i < maxMana; i++)
        {
            GameObject manaObj =
                Instantiate(manaPrefab, manaContainer);

            Image manaImage =
                manaObj.GetComponent<Image>();

            manaImages.Add(manaImage);
        }
    }

    private void UpdateManaUI()
    {
        int currentMana = playerMana.GetCurrentMana();

        for (int i = 0; i < manaImages.Count; i++)
        {
            int reversedIndex = manaImages.Count - 1 - i;

            if (reversedIndex < currentMana)
            {
                manaImages[i].sprite = fullManaSprite;
            }
            else
            {
                manaImages[i].sprite = emptyManaSprite;
            }
        }
    }
}

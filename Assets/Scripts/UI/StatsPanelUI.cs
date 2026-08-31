using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StatsPanelUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Image playerImage;

    [Header("Weapon")]
    [SerializeField] private Image weaponImage;

    [Header("Mutagens")]
    [SerializeField] private Image mutagen1Image;
    [SerializeField] private Image mutagen2Image;

    [Header("Perks")]
    [SerializeField] private Transform perkContainer;
    [SerializeField] private Image perkIconPrefab;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI attackRateText;
    [SerializeField] private TextMeshProUGUI dodgeCooldownText;
    [SerializeField] private TextMeshProUGUI mutagenPowerText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        Player player = Player.Instance;

        if (player == null)
            return;

        RefreshPlayer(player);
        RefreshWeapon(player);
        RefreshMutagens(player);
        RefreshPerks(player);
        RefreshStats(player);
    }

    // =========================
    // PLAYER
    // =========================
    private void RefreshPlayer(Player player)
    {
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        if (sr == null)
            return;

        playerImage.sprite = sr.sprite;
        playerImage.enabled = true;

        playerImage.preserveAspect = true;
    }

    // =========================
    // WEAPON
    // =========================
    private void RefreshWeapon(Player player)
    {
        if (weaponImage == null)
            return;

        Weapon weapon =
            player.playerAttack.GetCurrentWeapon()
            ?.GetComponent<Weapon>();

        if (weapon == null)
        {
            weaponImage.enabled = false;
            return;
        }

        SpriteRenderer sr =
            weapon.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            weaponImage.sprite = sr.sprite;
            weaponImage.enabled = true;
            weaponImage.preserveAspect = true;

            SetupTooltip(weaponImage, weapon);
        }
        else
        {
            weaponImage.enabled = false;
        }
    }

    // =========================
    // MUTAGENS
    // =========================
    private void RefreshMutagens(Player player)
    {
        MutagenController mc = player.mutagenController;

        MutagenSO m1 = mc.GetEquippedMutagenBySlot(0);
        MutagenSO m2 = mc.GetEquippedMutagenBySlot(1);

        mutagen1Image.sprite = m1 != null ? m1.icon : null;
        mutagen2Image.sprite = m2 != null ? m2.icon : null;

        mutagen1Image.enabled = m1 != null;
        mutagen2Image.enabled = m2 != null;

        mutagen1Image.preserveAspect = true;
        mutagen2Image.preserveAspect = true;

        if (m1 != null)
            SetupTooltip(mutagen1Image, m1);

        if (m2 != null)
            SetupTooltip(mutagen2Image, m2);
    }

    // =========================
    // PERKS
    // =========================
    private void RefreshPerks(Player player)
    {
        if (perkContainer == null || perkIconPrefab == null)
        {
            Debug.LogError("Perk UI references missing!");
            return;
        }
        foreach (Transform child in perkContainer)
            Destroy(child.gameObject);

            if (player.perkController == null)
        {
            Debug.LogError("perkController NULL");
            return;
        }

        var perks = player.perkController.activePerks
                    .Concat(player.perkController.perksToRemove)
                    .Where(perk => !perk.isHidden)
                    .ToList();

        Debug.Log("Perks count: " + perks.Count);

        foreach (PerkBase perk in perks)
        {
            if (perk == null || perk.icon == null)
                continue;

            Image icon = Instantiate(perkIconPrefab, perkContainer);
            icon.sprite = perk.icon;
            icon.enabled = true;

            icon.preserveAspect = true;

            SetupTooltip(icon, perk);
        }
    }

    // =========================
    // STATS 
    // =========================
    private void RefreshStats(Player player)
    {
        CharacterStats stats = player.playerStats.playerCurrentStats;

        if (stats == null)
            return;

        if (attackText != null)
            attackText.text = $"Attack: {stats.GetAttack()}";

        if (speedText != null)
            speedText.text = $"Speed: {stats.GetMoveSpeed()}";

        if (attackRateText != null)
            attackRateText.text = $"Attack Rate: {1/stats.GetAttackRate()}";

        if (dodgeCooldownText != null)
            dodgeCooldownText.text = $"Dodge Cooldown: {stats.GetDodgeCooldown()}";
        if (mutagenPowerText != null)
            mutagenPowerText.text = $"Mutagen Power: {stats.GetMutagenPower()}";
    }

    // =========================
    // Tooltip setup
    // =========================

    private void SetupTooltip(Image image, IDescribable source)
    {
        if (image == null)
            return;

        ItemTooltipTrigger tooltip =
            image.GetComponent<ItemTooltipTrigger>();

        if (tooltip == null)
            tooltip = image.gameObject.AddComponent<ItemTooltipTrigger>();

        tooltip.SetSource(source);
    }
}
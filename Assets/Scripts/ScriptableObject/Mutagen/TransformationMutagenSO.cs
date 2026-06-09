using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TransformationMutagen",
    menuName = "Mutagens/Transformation")]
public class TransformationMutagenSO : MutagenSO
{
    [Header("Transformation")]
    public List<PerkBase> perks = new();

    [Header("Visual")]
    public GameObject auraPrefab;

    public Gradient auraGradient;
    public float gradientSpeed = 1f;
    public Color playerTintColor = Color.white;

    public override bool Activate(
        Player player,
        MutagenInstance instance)
    {
        PerkController perkController =
            player.GetComponent<PerkController>();

        if (perkController == null)
            return false;

        List<PerkBase> addedPerks = new();

        foreach (PerkBase perk in perks)
        {
            if (perk == null)
                continue;

            PerkBase runtimePerk = Instantiate(perk);
            runtimePerk.isHidden = true;
            GameObject aura =
                Instantiate(
                    auraPrefab,
                    player.transform);
            AuraVisual visual =
                aura.GetComponent<AuraVisual>();

            visual.Initialize(
                auraGradient,
                gradientSpeed);
            perkController.AddPerk(runtimePerk);
            addedPerks.Add(runtimePerk);
        }

        instance.runtimeData["perks"] = addedPerks;

        if (auraPrefab != null)
        {
            GameObject visual =
                Instantiate(
                    auraPrefab,
                    player.transform);

            instance.runtimeData["visual"] = visual;
        }

        return true;
    }

    public override void Tick(
        Player player,
        MutagenInstance instance,
        float deltaTime)
    {

    }

    public override void Deactivate(
        Player player,
        MutagenInstance instance)
    {
        PerkController perkController =
            player.GetComponent<PerkController>();

        if (perkController != null &&
            instance.runtimeData.TryGetValue(
                "perks",
                out object perksObj))
        {
            List<PerkBase> addedPerks =
                perksObj as List<PerkBase>;

            foreach (PerkBase perk in addedPerks)
            {
                if (perk == null)
                    continue;

                perkController.RemovePerk(perk);
            }
        }

        if (instance.runtimeData.TryGetValue(
            "visual",
            out object visualObj))
        {
            GameObject visual =
                visualObj as GameObject;

            if (visual != null)
                Destroy(visual);
        }
    }

    public override string Description()
    {
        string perkList = "";

        foreach (PerkBase perk in perks)
        {
            if (perk != null)
                perkList += $"\n- {perk.perkName}";
        }

        return $"Transform into a powerful form, gaining the following perks:{perkList}\nDuration: {duration} seconds.";

    }
}

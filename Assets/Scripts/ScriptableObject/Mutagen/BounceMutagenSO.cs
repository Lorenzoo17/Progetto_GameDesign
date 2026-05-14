using UnityEngine;

[CreateAssetMenu(menuName = "Mutagens/Bounce Mutagen")]
public class BounceMutagenSO : MutagenSO
{
    [Header("Bounce")]
    public float bounceDistance = 8f;

    public float bounceDuration = 0.25f;

    [Header("Movement")]
    public float speedMultiplier = 2f;

    public override void Activate(Player player, MutagenInstance instance)
    {
        Vector2 bounceDirection =
            player.playerMovement.GetLookingDirection();

        player.playerMovement.ApplyBounce(
            bounceDirection,
            bounceDistance,
            bounceDuration
        );

        player.playerMovement.SetSpeedMultiplier(speedMultiplier);

        player.playerHealth.SetInvincible(true);
        Instantiate(animationEffect, player.transform.position, Quaternion.identity);
        Debug.Log("Bounce mutagen activated");
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
        player.playerMovement.ResetSpeedMultiplier();

        player.playerHealth.SetInvincible(false);

        Debug.Log("Bounce mutagen ended");
    }
}

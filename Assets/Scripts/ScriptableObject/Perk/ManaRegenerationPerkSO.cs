using UnityEngine;

[CreateAssetMenu(fileName = "new mana regeneration perk", menuName = "ScriptableObject/ManaRegenerationPerk")]
public class ManaRegenerationPerkSO : PerkBase
{
    [SerializeField] public int roomsRequired = 3; // ogni quante stanze rigenera
    [SerializeField] public int manaToRestore = 2; // quanto mana rigenera

    private int roomsCleared = 0;
    private Player player;

    public override void OnApply(Player player)
    {
        this.player = player;
        roomsCleared = 0;

        // Registra il listener agli eventi della stanza
        RoomBehaviour.OnAnyRoomCleared += HandleRoomCleared;
    }

    public override void OnRemove(Player player)
    {
        // Deregistra il listener
        RoomBehaviour.OnAnyRoomCleared -= HandleRoomCleared;
        this.player = null;
        roomsCleared = 0;
    }

    private void HandleRoomCleared(RoomBehaviour room)
    {
        if (player == null || player.playerMana == null) return;

        roomsCleared++;

        if (roomsCleared >= roomsRequired)
        {
            if (manaToRestore <= 0) player.playerMana.UseMana(Mathf.Abs(manaToRestore)); // se manaToRestore è negativo, consuma mana invece di rigenerarlo
            else
                player.playerMana.RestoreMana(manaToRestore);


            if (NotificationUI.Instance != null)
            {
                NotificationUI.Instance.ShowMessage(
                    $"Mana regenerated! (+{manaToRestore})"
                );
            }

            roomsCleared = 0; // Reset del contatore
        }
    }
}

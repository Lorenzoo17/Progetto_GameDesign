using UnityEngine;

public class NoAttackingArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<Player>() == null) return;

        Player.Instance.playerAttack.StopPlayerFromAttacking = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.GetComponent<Player>() == null) return;

        Player.Instance.playerAttack.StopPlayerFromAttacking = false;
    }
}

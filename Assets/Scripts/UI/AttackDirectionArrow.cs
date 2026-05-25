using UnityEngine;

public class AttackDirectionArrow : MonoBehaviour {
    private void Start() {
        if(Player.Instance != null) {
            Player.Instance.playerAttack.attackDirectionUI = this.transform;
        }
    }
}

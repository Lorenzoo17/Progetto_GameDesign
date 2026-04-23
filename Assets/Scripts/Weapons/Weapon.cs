using UnityEngine;

public interface IWeapon {
    void Attack(Vector2 attackDirection);
    void HandleRotation(Transform weaponHolder, Vector2 dir);
}

public class Weapon : MonoBehaviour, IWeapon, ICollectible{

    public bool pickedUp;

    public virtual void Attack(Vector2 attackDirection) {
        Debug.Log("Base weapon attack");
    }
    public virtual void HandleRotation(Transform weaponHolder, Vector2 dir) {
        Debug.Log("Base weapon rotation");
    }

    public void Collect(Player player) {
        if (pickedUp) return;

        player.playerAttack.SetCurrentWeapon(this.gameObject);
        // Visual effect
        
        pickedUp = true;
        if(GetComponent<Collider>() != null) {
            GetComponent<Collider>().enabled = false; // disattivo collider
        }
    }

    public void DropWeapon() {
        pickedUp = false;

        if (GetComponent<Collider>() != null) {
            GetComponent<Collider>().enabled = true; // riattivo collider
        }
    }

}

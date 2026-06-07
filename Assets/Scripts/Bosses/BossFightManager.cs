using UnityEngine;
using UnityEngine.AI;

public class BossFightManager : MonoBehaviour {
    [Header("Scripts del boss da bloccare")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private RoomBehaviour room;
    private BossRoom bossRoom;
    private HealthSystem healthSystem;

    private void Awake() {
        healthSystem = GetComponent<HealthSystem>();

        healthSystem.OnDamageTaken += HealthSystem_OnDamageTaken;
    }

    public void SetRoom(RoomBehaviour room) {
        this.room = room;
        bossRoom = room.GetComponent<BossRoom>();
    }

    private void HealthSystem_OnDamageTaken(object sender, DamageEventArgs e) {
        if (healthSystem.CurrentHealth <= 0) {
            if(room != null)
                room.OpenDoors(); // apro le porte

            // abilito spawn per andare a piano successivo
            bossRoom.ShowNextBasementEntrance();
        }
    }

    public void FreezeBoss() {
        // blocca scripts AI / movimento / attacco
        foreach (MonoBehaviour script in scriptsToDisable) {
            if (script != null)
                script.enabled = false;
        }
    }

    public void ResumeBoss() {
        foreach (MonoBehaviour script in scriptsToDisable) {
            if (script != null)
                script.enabled = true;
        }
    }
}
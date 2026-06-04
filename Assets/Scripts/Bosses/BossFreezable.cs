using UnityEngine;
using UnityEngine.AI;

public class BossFreezable : MonoBehaviour {
    [Header("Scripts del boss da bloccare")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
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
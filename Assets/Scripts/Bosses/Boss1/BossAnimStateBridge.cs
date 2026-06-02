using UnityEngine;

public class BossAnimStateBridge : StateMachineBehaviour
{
    // Funzione nativa di Unity chiamata automaticamente all'ULTIMO frame dell'animazione
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Trova il BossCtrl partendo dall'oggetto che ha l'Animator
        BossCtrl boss = animator.GetComponentInParent<BossCtrl>();

        if (boss != null)
        {
            // Attiva il via libera per la macchina a stati C#
            boss.AnimActionComplete = true;
        }
    }
}
using UnityEngine;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Knockback")]
public class BossKnockbackState : State<BossCtrl>
{
    private float timer;

    public override void Enter() {
        timer = _runner.LastKnockbackDuration;
        
        if (_runner.Agent != null) _runner.Agent.enabled = false;
        
        _runner.Rb.linearVelocity = Vector2.zero;
        _runner.Rb.AddForce(_runner.LastKnockbackDirection.normalized * _runner.LastKnockbackForce, ForceMode2D.Impulse);
        
        // inserire animazione hit knockback
    }

    public override void Update() {
        timer -= Time.deltaTime;
    }

    public override void ChangeState() {
        
        if (timer <= 0) {
            _runner.SetState(typeof(BossIntroMovementState)); 
        }
    }

    public override void Exit() {
        if (_runner.Agent != null) _runner.Agent.enabled = true;
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}
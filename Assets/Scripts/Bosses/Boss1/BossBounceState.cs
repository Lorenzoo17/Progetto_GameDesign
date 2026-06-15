using UnityEngine;
using System.Collections;
using stateMachine;

[CreateAssetMenu(menuName = "States/Boss/Bounce Stun")]
public class BossBounceState : State<BossCtrl>
{
    [Header("Impostazioni Salto (Bounce)")]
    [SerializeField] private float bounceDuration = 0.6f;
    [SerializeField] private float bounceHeight = 1f;   

    [Header("Onda d'Urto all'Atterraggio")]
    [SerializeField] private float shockwaveRadius = 4f;   
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float shockwaveDamage = 0f;

    private bool isBounceComplete;

    public override void Enter()
    {
        // LOG 6: Siamo entrati nello stato?
        Debug.Log("[DEBUG BOUNCE] >>> SONO ENTRATO CORRETTAMENTE IN BOSSBOUNCESTATE! <<<");

        isBounceComplete = false;
        _runner.AnimActionComplete = false;

        _runner.isBounceActive = true;

        if (_runner.Agent != null && _runner.Agent.isOnNavMesh)
        {
            _runner.Agent.isStopped = true;
        }

        if (_runner.Anim != null)
        {
            Debug.Log("[DEBUG BOUNCE] Lancio il trigger animator: play_bounce");
            _runner.Anim.SetTrigger("play_bounce");
        }

        _runner.StartCoroutine(VerticalBounceRoutine());
    }

    private IEnumerator VerticalBounceRoutine()
    {
        float timePassed = 0f;
        Vector3 startPos = _runner.transform.position;

        while (timePassed < bounceDuration)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / bounceDuration;

            if (_runner.Visuals != null)
            {
              
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
                _runner.Visuals.localPosition = new Vector3(0, heightOffset, 0);
            }
            yield return null;
        }

        
        if (_runner.Visuals != null) _runner.Visuals.localPosition = Vector3.zero;

        
        TriggerLandingShockwave();

        isBounceComplete = true;
    }

    private void TriggerLandingShockwave()
    {

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.EnemySmash, .3f);
        }

        Player player = Object.FindFirstObjectByType<Player>();

        if (player != null)
        {
            // Calcola la distanza esatta
            float distance = Vector3.Distance(_runner.transform.position, player.transform.position);

            if (_runner.debug) Debug.Log($"[BOSS BOUNCE] Atterraggio! Distanza Player: {distance:F2} | Raggio Onda: {shockwaveRadius}");

            if (distance <= shockwaveRadius)
            {
                // Calcoliamo il vettore spinta (dal boss verso il player)
                Vector2 pushDirection = ((Vector2)player.transform.position - (Vector2)_runner.transform.position).normalized;

                if (_runner.debug) Debug.Log($"[BOSS BOUNCE] Player colpito! Lo spingo via con forza {pushForce}.");

                // 🎯 Bypassiamo IDamageable e parliamo direttamente con il Motore Fisico del Player
                if (player.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
                {
                    // Chiamiamo la tua funzione che blocca l'input e lancia il rigidbody!
                    pm.ApplyKnockback(pushDirection, pushForce);
                }
                else
                {
                    Debug.LogError("[BOSS BOUNCE] Impossibile trovare PlayerMovement sul Player!");
                }
            }
            else
            {
                if (_runner.debug) Debug.Log($"[BOSS BOUNCE] Il player era al sicuro fuori dal raggio ({distance} > {shockwaveRadius}).");
            }
        }
    }

    public override void Update() { }

    public override void ChangeState()
    {
        if (isBounceComplete)
        {
            _runner.SetState(typeof(BossIntroMovementState));
        }
    }

    public override void Exit()
    {
        if (_runner.Agent != null && _runner.Agent.isOnNavMesh)
        {
            _runner.Agent.isStopped = false;
        }
        _runner.isBounceActive = false;
    }

    public override void CaptureInput() { }
    public override void FixedUpdate() { }
}
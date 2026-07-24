using System;
using UnityEngine;

public class EnemyHealthSystem : HealthSystem
{
    
    private void Start()
    {
        this.OnDamageTaken += EnemyHealthSystem_OnDamageTaken;
    }

    private void OnDestroy()
    {
        this.OnDamageTaken -= EnemyHealthSystem_OnDamageTaken;
    }

    private void EnemyHealthSystem_OnDamageTaken(object sender, DamageEventArgs e)
    {
        
    }
}

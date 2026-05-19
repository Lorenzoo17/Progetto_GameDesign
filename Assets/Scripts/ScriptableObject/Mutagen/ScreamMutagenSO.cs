using UnityEngine;

[CreateAssetMenu(menuName = "Mutagens/Scream")]
public class ScreamSO : MutagenSO
{
    public override bool Activate(Player player, MutagenInstance instance)
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.SetStun(true);
        }

    
    if (animationEffect != null)
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mouseWorld - (Vector2)player.transform.position).normalized;

        float spawnDistance = 1f;

        Vector3 spawnPos =
            player.transform.position + (Vector3)(direction * spawnDistance);

        GameObject fx = Instantiate(animationEffect, spawnPos, Quaternion.identity);
        
        // orienta il VFX verso la direzione dello scream
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0f, 0f, angle -180f);
        Object.Destroy(fx, 2f);
    }

        return true;
    }

    public override void Tick(Player player, MutagenInstance instance, float deltaTime)
    {
        // gestito dal controller
    }

    public override void Deactivate(Player player, MutagenInstance instance)
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.SetStun(false);
        }
    }
}
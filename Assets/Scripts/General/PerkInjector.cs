using UnityEngine;

public class PerkInjector : MonoBehaviour
{
    [SerializeField] private PerkBase perkToInject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            player.perkController.AddPerk(perkToInject);
            Destroy(gameObject);
        }
    }
}
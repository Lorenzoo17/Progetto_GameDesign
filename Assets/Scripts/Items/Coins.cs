using UnityEngine;

public enum CoinType {
    Mutagen,
    Dungeon
}
public class Coins : MonoBehaviour, ICollectible {
    public CoinType coinType;
    [SerializeField] private int coinAmount;
    [SerializeField] private float distanceForFollow = 5f;
    [SerializeField] private float followSpeed = 2f;

    public bool towardsPlayer = true;

    public void Collect(Player player) {
        if (MetaProgressionManager.Instance != null) {
            switch (coinType) {
                case CoinType.Mutagen:
                    MetaProgressionManager.Instance.AddMutagenCoin(coinAmount);
                    break;
                case CoinType.Dungeon:
                    MetaProgressionManager.Instance.AddDungeonCoin(coinAmount);
                    break;
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(SoundID.CoinPickUp, .25f);
        }

        // sound
        Destroy(gameObject);
    }

    private void Update() {
        if (Player.Instance == null || !towardsPlayer) return;

        float distance = Vector2.Distance(Player.Instance.transform.position, this.transform.position);
        Vector2 direction = (Player.Instance.transform.position - transform.position).normalized;

        if (distance < distanceForFollow) {
            transform.position += (Vector3)direction * followSpeed * Time.deltaTime;
        }
    }
}

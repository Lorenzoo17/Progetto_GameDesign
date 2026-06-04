using UnityEngine;

public class BossRoom : MonoBehaviour {

    [SerializeField] private GameObject[] bossesToSpawn;
    private RoomBehaviour rb;
    [SerializeField] private Transform bossSpawnTransform;
    private bool spawned;

    private void Awake() {
        rb = GetComponent<RoomBehaviour>();

        if(bossSpawnTransform == null) {
            bossSpawnTransform = rb.roomCentre;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.GetComponent<Player>() || bossesToSpawn.Length == 0) return;
        if (spawned) return;
        int index = Random.Range(0, bossesToSpawn.Length);
        GameObject boss = Instantiate(bossesToSpawn[index], bossSpawnTransform.position, Quaternion.identity);

        spawned = true;

        // attivo splashScreen
        if (BossSplashScreen.Instance != null) {
            if(boss.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) {
                BossFreezable bf = boss.GetComponent<BossFreezable>();
                BossSplashScreen.Instance.SetBossSplashScreen(boss.name, sr.sprite, bf);
            }
        }
    }
}

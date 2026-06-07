using UnityEngine;

public class BossRoom : MonoBehaviour {

    [SerializeField] private GameObject[] bossesToSpawn;
    private RoomBehaviour rb;
    [SerializeField] private Transform bossSpawnTransform;
    private bool spawned;

    [SerializeField] private GameObject nextBasementEntrance;

    private void Awake() {
        rb = GetComponent<RoomBehaviour>();

        if(bossSpawnTransform == null) {
            bossSpawnTransform = rb.roomCentre;
        }
    }

    private void Start() {
        if (nextBasementEntrance == null) return;

        nextBasementEntrance.SetActive(false);
    }

    public void ShowNextBasementEntrance() {
        if (nextBasementEntrance == null) return;

        nextBasementEntrance.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.GetComponent<Player>() || bossesToSpawn.Length == 0) return;
        if (spawned) return;
        int index = Random.Range(0, bossesToSpawn.Length);
        GameObject boss = Instantiate(bossesToSpawn[index], bossSpawnTransform.position, Quaternion.identity);

        spawned = true;

        if (boss.TryGetComponent<BossFightManager>(out BossFightManager bf)) {
            bf.SetRoom(rb);

            // attivo splashScreen
            if (BossSplashScreen.Instance != null) {
                if (boss.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) {
                    BossSplashScreen.Instance.SetBossSplashScreen(boss.name.Replace("(Clone)", "").Trim(), sr.sprite, bf);
                }
            }
        }
    }
}

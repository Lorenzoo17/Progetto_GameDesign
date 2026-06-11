using UnityEngine;
using UnityEngine.AI;

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
        if (!other.GetComponent<Player>()) return;
        if (spawned) return;
        int index = Random.Range(0, bossesToSpawn.Length);

        Vector3 spawnPosition = bossSpawnTransform.position;
        spawnPosition.z = 0f;
        // per capire se sta spawnando in un punto lecito per la navmesh
        if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas)) {
            Debug.LogWarning($"Boss spawn point non vicino alla NavMesh: {spawnPosition}");
            return;
        }
        GameObject boss = Instantiate(bossesToSpawn[index], hit.position, Quaternion.identity);
        spawned = true;

        if (boss.TryGetComponent<BossFightManager>(out BossFightManager bf)) {
            bf.SetRoom(rb);

            // attivo splashScreen
            if (BossSplashScreen.Instance != null) {
                if (boss.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) {
                    BossSplashScreen.Instance.SetBossSplashScreen(boss.name.Replace("(Clone)", "").Trim(), sr.sprite, bf);
                }
                else {
                    if(boss.GetComponentInChildren<SpriteRenderer>() != null) {
                        Sprite sprite = boss.transform.Find("Visual").GetComponent<SpriteRenderer>().sprite;
                        if(sprite == null) {
                            Debug.LogWarning("Componente Visual non trovato nel transform");
                        }
                        BossSplashScreen.Instance.SetBossSplashScreen(boss.name.Replace("(Clone)", "").Trim(), sprite, bf);
                    }
                }
            }
        }
    }
}

using UnityEngine;

public class TrapShooter : MonoBehaviour {
    public ShooterType shooterType;
    public Vector2 direction;
    private ProjectileShooter projectileShooter;
    [SerializeField] private float projectileDamage = 1f;

    [SerializeField] private float minFireRate = 0.3f;
    [SerializeField] private float maxFireRate = 1.5f;
    private bool active;
    private float fireRate;
    private float timeBtwFire;
    private int projectileNumber;

    [SerializeField] private RoomBehaviour room;

    private void Awake() {
        projectileShooter = GetComponent<ProjectileShooter>();
    }

    private void Start() {
        fireRate = Random.Range(minFireRate, maxFireRate);
        direction = -transform.up;

        if(room != null) {
            room.OnRoomEnter += Room_OnRoomEnter;
            room.OnRoomExit += Room_OnRoomExit;
        }
    }

    private void Room_OnRoomExit(object sender, System.EventArgs e) {
        active = false;
        timeBtwFire = 0f;
    }

    private void Room_OnRoomEnter(object sender, System.EventArgs e) {
        active = true;
        timeBtwFire = 0f;
    }

    private void Update() {
        if (!active) return;

        if(timeBtwFire <= 0) {
            switch (shooterType) {
                case ShooterType.Linear:
                    direction = -transform.up;
                    projectileShooter.ShootLinear(gameObject, direction, projectileDamage);
                    break;
                case ShooterType.Curved: // in questo caso piu' che curved va verso il player e basta
                    direction = ((Vector2)Player.Instance.transform.position - (Vector2)projectileShooter.firePoint.position).normalized;
                    projectileShooter.ShootLinear(gameObject, direction, projectileDamage);
                    break;
                case ShooterType.Circle:
                    projectileNumber = Random.Range(3, 5);
                    projectileShooter.ShootMultipleProjectile(gameObject, projectileNumber, null, true, projectileDamage);
                    break;
                default:
                    direction = -transform.up;
                    projectileShooter.ShootLinear(gameObject, direction, projectileDamage);
                    break;
            }
            timeBtwFire = fireRate;
        }
        else {
            timeBtwFire -= Time.deltaTime;
        }
    }
}

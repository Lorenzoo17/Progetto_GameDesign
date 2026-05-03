using UnityEngine;


public class EntityOwner : MonoBehaviour {
    [SerializeField] private EntityType entityType;

    public EntityType GetEntityType => entityType;
}

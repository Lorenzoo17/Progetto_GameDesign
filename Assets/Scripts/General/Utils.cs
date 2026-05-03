using System.Collections;
using UnityEngine;

public enum EntityType {
    Player,
    Enemy,
    Neutral
}

public struct DamageInfo {
    public float Damage { get; private set; }
    public Vector2 Direction { get; private set; }
    public GameObject Source { get; private set; }
    public EntityType SourceFaction { get; private set; }

    public DamageInfo(float damage, Vector2 direction, GameObject source, EntityType sourceFaction) {
        Damage = damage;
        Direction = direction;
        Source = source;
        SourceFaction = sourceFaction;
    }
}

public static class Utils {
    public static IEnumerator FreezeFrame(float duration) {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }

    public static class CombatUtility {
        public static bool CanDamage(GameObject attacker, GameObject target) {
            if (attacker == null || target == null) return false;
            if (attacker == target) return false;

            EntityOwner attackerOwner = attacker.GetComponent<EntityOwner>();
            EntityOwner targetOwner = target.GetComponent<EntityOwner>();

            if (attackerOwner == null || targetOwner == null) return true;

            return attackerOwner.GetEntityType != targetOwner.GetEntityType;
        }
    }
}

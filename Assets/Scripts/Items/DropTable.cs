using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropEntry
{
    public string itemName;
    public GameObject prefab;

    [Min(0f)]
    public float weight = 1f;
}

public class DropTable : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.3f;
    [SerializeField] private List<DropEntry> dropEntries = new List<DropEntry>();

    public float DropChance => dropChance;
    public List<DropEntry> DropEntries => dropEntries;
}

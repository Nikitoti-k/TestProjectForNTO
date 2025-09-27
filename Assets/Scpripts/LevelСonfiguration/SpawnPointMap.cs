// Хранит точки спавна как детей объекта.
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointMap : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private void OnValidate()
    {
        spawnPoints.Clear();
        foreach (Transform child in transform)
        {
            if (child != null) spawnPoints.Add(child);
        }
    }

    public List<Transform> GetSpawnPoints()
    {
        return spawnPoints.FindAll(point => point != null);
    }
}
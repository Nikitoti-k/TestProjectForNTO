using System.Collections.Generic;
using UnityEngine;

public class SpawnPointMap : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>(); // Дети

    private void OnValidate()
    {
        spawnPoints.Clear();
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    public List<Transform> GetSpawnPoints()
    {
        return spawnPoints;
    }
}
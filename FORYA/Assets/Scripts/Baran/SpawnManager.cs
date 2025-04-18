using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField] private List<Transform> spawnPoints;
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }
    public Vector3 GetSpawnPointForClient(ulong clientID)
    {
        int index = (int)(clientID % (ulong)spawnPoints.Count);
        return spawnPoints[index].position;
    }
}

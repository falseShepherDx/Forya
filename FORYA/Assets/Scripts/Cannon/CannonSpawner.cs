using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;  // Linq, spawn noktalarını karıştırmak için

[RequireComponent(typeof(NetworkObject))]
public class CannonSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private GameObject cannonPrefab;

    [Header("Timing")]
    [SerializeField]
    private float spawnInterval = 2f;

    [Header("Difficulty Scaling")]
    [SerializeField]
    private float difficultyRampInterval = 30f;
    [SerializeField]
    private int maxSimultaneousSpawns = 3;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogError("CannonSpawner: No spawn points assigned!", this);
        if (centerPoint == null)
            Debug.LogError("CannonSpawner: centerPoint not assigned!", this);
        if (cannonPrefab == null)
            Debug.LogError("CannonSpawner: cannonPrefab not assigned!", this);

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
       // increased cannon amount based on time
        int waveCount = 1 + Mathf.FloorToInt(Time.timeSinceLevelLoad / difficultyRampInterval);
        int spawnCount = Mathf.Clamp(waveCount, 1, maxSimultaneousSpawns);

        
        var points = spawnPoints.OrderBy(_ => Random.value)
                                .Take(spawnCount);

        foreach (var point in points)
        {
            SpawnSingleCannon(point);
        }
    }

    private void SpawnSingleCannon(Transform spawnPoint)
    {
        //cannons look towards play area
        Vector3 dir = (centerPoint.position - spawnPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f);

        //spawning
        var cannon = Instantiate(cannonPrefab, spawnPoint.position, rot);
        cannon.GetComponent<NetworkObject>().Spawn(true);
    }
}

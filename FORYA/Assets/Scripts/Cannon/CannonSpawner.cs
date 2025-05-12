using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NetworkObject))]
public class CannonSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] centerPoints;
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
        if (centerPoints == null  || centerPoints.Length == 0)
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

        //choose a spawn point and assign so no
        List<Transform> available = new List<Transform>(spawnPoints);
        for (int i = 0; i < spawnCount; i++)
        {
            int spawnIndex = Random.Range(0, available.Count);
            Transform spawnPoint = available[spawnIndex];
            available.RemoveAt(spawnIndex);
            
            Transform center = centerPoints[Random.Range(0, centerPoints.Length)];
            SpawnSingleCannon(spawnPoint, center);
        }
    }

    private void SpawnSingleCannon(Transform spawnPoint,Transform centerPoint)
    {
        //look towards center pos
        Vector3 dir = (centerPoint.position - spawnPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f);

        //instantiation
        var cannon = Instantiate(cannonPrefab, spawnPoint.position, rot);
        cannon.GetComponent<NetworkObject>().Spawn(true);
    }
}

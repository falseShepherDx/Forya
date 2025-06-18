using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(NetworkObject))]
public class CannonSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject cannonPrefab;

    [Header("Timing")]
    [SerializeField]
    private float spawnInterval = 2f;

    [Header("Difficulty Scaling")]
    [SerializeField]
    private float difficultyRampInterval = 30f;
    [SerializeField]
    private int maxSimultaneousSpawns = 3;
    private bool isSpawning = false;
    private Coroutine spawnRoutine;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
    }
    public void StartSpawning()
    {
        if (!IsServer || spawnRoutine != null) return;
        isSpawning = true;
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
    

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
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
            
            
            Vector3 targetPos = GetRandomPlayerPosition();
            SpawnSingleCannon(spawnPoint, targetPos);
        }
    }

    private void SpawnSingleCannon(Transform spawnPoint, Vector3 targetPos)
    {
        Vector3 dir = (targetPos - spawnPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        var cannon = Instantiate(cannonPrefab, spawnPoint.position, rot);
        cannon.GetComponent<NetworkObject>().Spawn(true);
        cannon.GetComponent<Cannon>().SetTarget(targetPos);
    }

    private Vector3 GetRandomPlayerPosition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
            return Vector3.forward * 5f; // fallback

        GameObject chosen = players[Random.Range(0, players.Length)];
        return chosen.transform.position;
    }
    

   

}

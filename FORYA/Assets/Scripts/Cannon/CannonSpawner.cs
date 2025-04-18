using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CannonSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject cannonPf;
    [SerializeField] private float shootingDelay=2f;
    private float timer;

    private void Update()
    {
        if (!IsServer) return;
        timer += Time.deltaTime;
        if (timer >= shootingDelay)
        {
            timer = 0;
            SpawnCannon();
        }
    }

    private void SpawnCannon()
    {
        Transform point=spawnPoints[Random.Range(0,spawnPoints.Length)];
        GameObject cannon = Instantiate(cannonPf, point.position, point.rotation);
        cannon.GetComponent<NetworkObject>().Spawn();
        
    }
}

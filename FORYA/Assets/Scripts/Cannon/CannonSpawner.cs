using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CannonSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] cannonSpawnPoints;
    [SerializeField] private GameObject cannonPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(CannonSpawnRoutine());
    }

    IEnumerator CannonSpawnRoutine()
    {
        while (true)
        {
            Transform selectedPoint = cannonSpawnPoints[Random.Range(0, cannonSpawnPoints.Length)];
            GameObject cannonInstance = Instantiate(cannonPrefab, selectedPoint.position, selectedPoint.rotation);
            cannonInstance.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        }
    }
}
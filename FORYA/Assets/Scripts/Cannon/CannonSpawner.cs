using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CannonSpawner : NetworkBehaviour
{
    public Transform[] spawnPoints;
    public GameObject cannonPrefab;
    [SerializeField] private Transform centerPoint;
    

    private void Start()
    {
        if(IsServer)
            StartCoroutine(SpawnRoutine());
    }
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); 
            SpawnCannon();
        }
    }

    void SpawnCannon()
    {
        Transform selectedPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 direction = (centerPoint.position - selectedPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);

        GameObject cannonInstance = Instantiate(cannonPrefab, selectedPoint.position, rotation);
        cannonInstance.GetComponent<NetworkObject>().Spawn(true);
    }
}
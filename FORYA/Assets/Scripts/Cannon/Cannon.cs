using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Cannon : NetworkBehaviour
{
    public GameObject cannonBallPrefab;
    public Transform firePoint;
    public float shootForce = 20f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(FireAndDestroyRoutine());
    }

    IEnumerator FireAndDestroyRoutine()
    {
        yield return new WaitForSeconds(1f); // Cannon spawn olduktan 1 sn sonra ateş eder

        GameObject cannonBallInstance = Instantiate(cannonBallPrefab, firePoint.position, Quaternion.identity);
        NetworkObject ballNetworkObject = cannonBallInstance.GetComponent<NetworkObject>();
        ballNetworkObject.Spawn(true); 

        
        Rigidbody rb = cannonBallInstance.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.right * shootForce, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);
        NetworkObject.Despawn(true); 
    }
}
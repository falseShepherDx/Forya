using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Cannon : NetworkBehaviour
{
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 25f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        
        yield return new WaitForSeconds(1f);

        GameObject ball = Instantiate(cannonBallPrefab, shootPoint.position, shootPoint.rotation);
        ball.GetComponent<NetworkObject>().Spawn(true);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = shootPoint.forward * shootForce;

       
        yield return new WaitForSeconds(0.5f);
        NetworkObject.Despawn();
    }
}
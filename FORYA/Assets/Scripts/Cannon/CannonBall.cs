using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class CannonBall : NetworkBehaviour
{
    [SerializeField] private PlayerMovement_B movementScript;
    
    private void Awake()
    {
        int ballLayer = LayerMask.NameToLayer("CannonBall");
        gameObject.layer = ballLayer;
        Physics.IgnoreLayerCollision(ballLayer, ballLayer, true);
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(DespawnAfterTime(4f));
    }
    private IEnumerator DespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (IsSpawned)
            GetComponent<NetworkObject>().Despawn(true);
    }
    

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            var hitPoint = collision.GetContact(0).point;
            collision.gameObject.GetComponent<PlayerMovement_B>().DeadServerRpc();
            playerHealth.KillServerRpc();
        }
    }

    
}
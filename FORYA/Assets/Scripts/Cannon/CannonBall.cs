using System;
using UnityEngine;
using Unity.Netcode;

public class CannonBall : NetworkBehaviour
{
    
    private void Awake()
    {
        int ballLayer = LayerMask.NameToLayer("CannonBall");
        gameObject.layer = ballLayer;
        
        Physics.IgnoreLayerCollision(ballLayer, ballLayer, true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            var hitPoint = collision.GetContact(0).point;
            RoundManager.Instance.SpawnHitVFX(hitPoint);
            playerHealth.KillServerRpc();
        }
        
    }

    
}
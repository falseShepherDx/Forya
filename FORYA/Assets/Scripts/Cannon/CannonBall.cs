using System;
using UnityEngine;
using Unity.Netcode;

public class CannonBall : NetworkBehaviour
{
    void Start()
    {
            Invoke(nameof(DestroySelf), 5f); // 5 saniye içinde mutlaka yok olur
    }

    private void Awake()
    {
        int ballLayer = LayerMask.NameToLayer("CannonBall");
        gameObject.layer = ballLayer;
        
        Physics.IgnoreLayerCollision(ballLayer, ballLayer, true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.TryGetComponent<PlayerHealth>(out var player))
        {
            player.HandleDeathServerRpc();
        }

        
    }

    void DestroySelf()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
    }
}
using UnityEngine;
using Unity.Netcode;

public class CannonBall : NetworkBehaviour
{
    private void Start()
    {
        if (IsServer)
            Invoke(nameof(DestroySelf), 5f); 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.transform.TryGetComponent(out PlayerHealth player))
        {
            player.PlayerDiedServerRpc();
        }

        DestroySelf();
    }

    void DestroySelf()
    {
        NetworkObject.Despawn(true);
    }
}
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public override void OnNetworkSpawn()
    {
        currentHealth = maxHealth;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
            HandleDeathClientRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void KillServerRpc(ServerRpcParams rpcParams = default)
    {
        if (currentHealth <= 0) return;
        currentHealth = 0;
        HandleDeathClientRpc();
    }
    [ClientRpc]
    private void HandleDeathClientRpc(ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;
        var deathHandler = GetComponent<PlayerDeathHandler>();
        deathHandler?.OnDeath();
    }



}
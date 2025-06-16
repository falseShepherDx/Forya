using System;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
    private PlayerDeathHandler deathHandler;

    private void Awake()
    {
        deathHandler = GetComponent<PlayerDeathHandler>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
          
            CurrentHealth.Value = maxHealth;
        }
    }
    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        if (CurrentHealth.Value <= 0) return;

        CurrentHealth.Value -= amount;
        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            KillServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void KillServerRpc(ServerRpcParams rpcParams = default)
    {
        if (CurrentHealth.Value <= 0) return;
        CurrentHealth.Value = 0;
        HandleDeathClientRpc();
    }
    [ClientRpc]
    private void HandleDeathClientRpc()
    {
        deathHandler.OnDeath();
    }

}
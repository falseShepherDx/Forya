using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerHealth : NetworkBehaviour
{
    NetworkVariable<bool> alive = new NetworkVariable<bool>(true);

    [ServerRpc(RequireOwnership = false)]
    public void HandleDeathServerRpc()
    {
        if (!alive.Value) return;

        alive.Value = false;
        gameObject.SetActive(false);

        CheckWinner();
    }

    void CheckWinner()
    {
        var alivePlayers = FindObjectsOfType<PlayerHealth>().Where(x => x.alive.Value).ToList();
        if (alivePlayers.Count == 1)
        {
            Debug.Log($"Kazanan oyuncu: {alivePlayers[0].OwnerClientId}");
        }
    }
}
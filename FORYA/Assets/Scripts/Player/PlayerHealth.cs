using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PlayerHealth : NetworkBehaviour
{
    public NetworkVariable<bool> alive = new NetworkVariable<bool>(true);

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiedServerRpc()
    {
        if (!alive.Value) return;
        
        alive.Value = false;
        gameObject.SetActive(false);

        CheckWinner();
    }

    void CheckWinner()
    {
        var livingPlayers = FindObjectsOfType<PlayerHealth>().Count(player => player.alive.Value);

        if (livingPlayers == 1)
        {
            var winner = FindObjectsOfType<PlayerHealth>().First(player => player.alive.Value);
            Debug.Log($"Winner Client ID: {winner.OwnerClientId}");
        }
    }
}
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager_B : NetworkBehaviour
{
    public static GameManager_B instance;

    private HashSet<ulong> alivePlayers = new HashSet<ulong>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            alivePlayers.Clear();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                alivePlayers.Add(client.ClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveAlivePlayerServerRpc(ulong clientId)
    {
        if (!alivePlayers.Contains(clientId)) return;

        alivePlayers.Remove(clientId);

        if (alivePlayers.Count == 1)
        {
            ulong winnerId = 0;
            foreach (var id in alivePlayers) winnerId = id;

            string winnerName = LobbyManager.instance.GetNameByClientId(winnerId);
            WinScreen.instance.ShowWinScreenClientRpc(winnerName);
        }
    }
}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (IsServer && SceneManager.GetActiveScene().name !="LobbyScene")
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

    [ServerRpc(RequireOwnership = false)]
    public void ReturnSingleClientToMainMenuServerRpc(ulong clientId)
    {
        ClientRpcParams targetClient = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };

        ReturnToMainMenuClientRpc(targetClient);

        NetworkManager.DisconnectClient(clientId);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ReturnEveryoneToMainMenuServerRpc()
    {
        if (!IsServer) return;

        LobbyManager.instance.ClearAllPlayers();

        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
            Debug.Log("Host + tüm oyuncular LobbyScene’e gönderildi.");
        }
        else
        {
            Debug.LogWarning("SceneManager null!");
        }
    }


    [ClientRpc]
    void ReturnToMainMenuClientRpc(ClientRpcParams clientRpcParams)
    {
        SceneManager.LoadScene("LobbyScene");
    }
}

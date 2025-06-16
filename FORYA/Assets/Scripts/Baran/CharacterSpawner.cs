using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    int playerIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            return;
        }
      

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

     
            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            {
                NetworkObject oldPlayerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                oldPlayerObj.Despawn(true);
            }

            
            Vector3 spawnPos = SpawnManager.instance.GetSpawnPointForClient(clientId);
            GameObject player = Instantiate(playerPrefabs[playerIndex], spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            playerIndex++;
        }
    }
}

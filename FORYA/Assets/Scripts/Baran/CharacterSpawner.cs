using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // SADECE oyun sahnesinde �al��s�n
        if (SceneManager.GetActiveScene().name != "K�r�lanKarolar" || SceneManager.GetActiveScene().name!="Cannon Circle") return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            // Eski objeyi sil
            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            {
                NetworkObject oldPlayerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                oldPlayerObj.Despawn(true); // tamamen sil
            }

            // Yeni karakter do�ur
            Vector3 spawnPos = SpawnManager.instance.GetSpawnPointForClient(clientId);
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}

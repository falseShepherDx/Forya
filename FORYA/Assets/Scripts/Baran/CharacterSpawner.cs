using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSpawner : NetworkBehaviour
{
    public static CharacterSpawner instance;

    [SerializeField] private GameObject[] playerPrefabs;
    int playerIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            return;
        }   
    }

    public void SpawnPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;


            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
            {
                NetworkObject oldPlayerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                oldPlayerObj.Despawn(true);
            }

            Vector3 spawnPos = SpawnManager.instance.GetSpawnPointForClient(clientId);
            Debug.Log($"[SPAWN] Client {clientId} -> {spawnPos}");
            GameObject player = Instantiate(playerPrefabs[playerIndex], spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Rigidbody rb = player.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            string nick = LobbyManager.instance.GetNameByClientId(clientId);
            SetPlayerNameClientRpc(clientId, nick);
            playerIndex++;
        }
    }

    [ClientRpc]
    private void SetPlayerNameClientRpc(ulong clientId, string playerName)
    {
        foreach (var player in FindObjectsOfType<NetworkObject>())
        {
            if (player.OwnerClientId == clientId)
            {
                var nameText = player.transform.Find("NameText")?.GetComponent<TextMeshPro>();
                if (nameText != null)
                    nameText.text = playerName;
                else
                {
                    Debug.Log("TextMeshyok");
                }
            }
        }
    }



}

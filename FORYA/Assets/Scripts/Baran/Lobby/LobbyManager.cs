using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

[DefaultExecutionOrder(-100)]
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager instance;

    [SerializeField] Transform UIParent;
    [SerializeField] GameObject playerUIPrefab;
    public List<GameObject> panels;

    [SerializeField] GameObject startButton;

    private NetworkList<FixedString32Bytes> syncedPlayerNames;
    private Dictionary<ulong, int> clientToIndex = new Dictionary<ulong, int>();

    private NetworkList<bool> playerReadyStates;

    private void Awake()
    {
        syncedPlayerNames = new NetworkList<FixedString32Bytes>();
        playerReadyStates = new NetworkList<bool>();
    }

    public override void OnNetworkSpawn()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(instance);

        syncedPlayerNames.OnListChanged += OnPlayerListChanged;
        playerReadyStates.OnListChanged += OnReadyListChanged;

        if (IsServer)
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

        UpdateUI(); // herkes kendi UI’sini günceller
    }


    public override void OnNetworkDespawn()
    {
        if (syncedPlayerNames != null)
            syncedPlayerNames.OnListChanged -= OnPlayerListChanged;

        if (playerReadyStates != null)
            playerReadyStates.OnListChanged -= OnReadyListChanged;

        if (IsServer)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }



    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerPanelServerRPC(string playerName, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (clientToIndex.ContainsKey(senderId)) return;
        if (syncedPlayerNames.Count >= panels.Count) return;

        syncedPlayerNames.Add(playerName);
        clientToIndex[senderId] = syncedPlayerNames.Count - 1;

        playerReadyStates.Add(false);
        Debug.Log($"[Server] {playerName} added to panel {syncedPlayerNames.Count - 1}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientToIndex.TryGetValue(clientId, out int index))
        {
            if (index < syncedPlayerNames.Count)
            {
                syncedPlayerNames[index] = new FixedString32Bytes("Null");
            }

            clientToIndex.Remove(clientId);
            Debug.Log($"[Server] Client {clientId} disconnected, panel {index} cleared.");
        }
    }

  

    private void UpdateUI()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            var text = panels[i].GetComponentInChildren<TextMeshProUGUI>();
            var readyImage = panels[i].transform.Find("ReadyImage")?.GetComponent<UnityEngine.UI.Image>();

            if (i < syncedPlayerNames.Count)
            {
                text.text = syncedPlayerNames[i].ToString();
            }
            else
            {
                text.text = "Null";
            }

            // Ready durumu varsa sprite'ý renklendir
            if (readyImage != null)
            {
                if (i < playerReadyStates.Count && playerReadyStates[i])
                    readyImage.color = Color.green;
                else
                    readyImage.color = Color.red;
            }
            else Debug.Log("ýmage yok");
        }
    }

    public bool AreAllPlayersReady()
    {
        if (playerReadyStates.Count == 0) return false;

        foreach (var ready in playerReadyStates) 
        {
            if (!ready)
            {
                return false;
            }
        }
        return true;
    }

    private void OnPlayerListChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        UpdateUI();
    }
    private void OnReadyListChanged(NetworkListEvent<bool> changeEvent)
    {
        UpdateUI();
        if (IsHost && startButton != null)
        {
            startButton.SetActive(AreAllPlayersReady());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!clientToIndex.ContainsKey(senderId)) return;

        int index = clientToIndex[senderId];

        if (index >= 0 && index < playerReadyStates.Count)
        {
            playerReadyStates[index] = !playerReadyStates[index]; // toggle
            Debug.Log($"[Server] Player {senderId} ready state is now {playerReadyStates[index]}.");
        }
    }
}

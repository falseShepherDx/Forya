using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Hit Effect")] [SerializeField]
    private GameObject hitSplatPrefab;

    [Header("UI")] [SerializeField] private GameObject roundUIPrefab;

    private HashSet<ulong> alivePlayers = new HashSet<ulong>();
    private RoundUI roundUI;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                alivePlayers.Add(client.ClientId);
        }
        UpdateAliveCountClientRpc(alivePlayers.Count);
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            UpdateAliveCountClientRpc(alivePlayers.Count);
        };

        if (IsClient)
        {
            var uiGO = Instantiate(roundUIPrefab);
            roundUI = uiGO.GetComponent<RoundUI>();
        }
    }
    public void ReportDeath(ulong clientId)
    {
        if (!IsServer) return;
        if (alivePlayers.Remove(clientId))
        {
            UpdateAliveCountClientRpc(alivePlayers.Count);

            if (alivePlayers.Count == 1)
            {
                var winner = alivePlayers.First();
                AnnounceWinnerClientRpc(winner);
            }
        }
    }
    public void SpawnHitVFX(Vector3 position)
    {
        SpawnHitVFXClientRpc(position);
    }
    [ClientRpc]
    private void SpawnHitVFXClientRpc(Vector3 pos)
    {
        Instantiate(hitSplatPrefab, pos, Quaternion.identity);
    }
    [ClientRpc]
    private void UpdateAliveCountClientRpc(int aliveCount)
    {
        roundUI.UpdateAliveCount(aliveCount);
    }
    [ClientRpc]
    private void AnnounceWinnerClientRpc(ulong winnerClientId)
    {
        roundUI.ShowWinner(winnerClientId);
    }

}

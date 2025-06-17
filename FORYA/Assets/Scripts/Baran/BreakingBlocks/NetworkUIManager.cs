using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUIManager : NetworkBehaviour
{
    public static NetworkUIManager instance;

    [SerializeField] TextMeshProUGUI playerCountText;
    private NetworkVariable<int> playerCount = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

  

    public override void OnNetworkSpawn()
    {
        if (instance == null)
        {
            instance = this;
        }
        else { Destroy(this); }

        if (IsClient)
        {
            playerCount.OnValueChanged += OnPlayerCountChanged;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void IncreasePlayerCountServerRPC()
    {
        playerCount.Value++;

    }

    [ServerRpc(RequireOwnership = false)]
    public void DecreasePlayerCountServerRPC()
    {
        playerCount.Value--;

    }

    [Obsolete("Obsolete")]
    void OnPlayerCountChanged(int oldVal, int newVal)
    {
        playerCountText.text = playerCount.Value + " Player Alive!";

        if (playerCount.Value <= 0)
        {
            var spawner = FindObjectOfType<CannonSpawner>();
            if (spawner != null)
                spawner.StopSpawning();
        }
    }

}

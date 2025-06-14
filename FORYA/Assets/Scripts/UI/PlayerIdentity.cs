using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public class PlayerIdentity : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>();
    public string DisplayName => displayName.Value.ToString();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            displayName.Value = $"Player {OwnerClientId}";
        }
    }
}

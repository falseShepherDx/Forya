using UnityEngine;
using Unity.Netcode;

public class TickManager : NetworkBehaviour
{
    public static int CurrentTick { get; private set; }

    private void Update()
    {
        if (IsServer)
        {
            CurrentTick++;
            Debug.Log($"[SERVER TICK] Tick: {CurrentTick}");
            UpdateTickClientRpc(CurrentTick);
        }
        else
        {
            Debug.Log($"[CLIENT TICK] Tick: {CurrentTick}");
        }
    }

    [ClientRpc]
    private void UpdateTickClientRpc(int serverTick)
    {
        if (!IsServer)
        {
            CurrentTick = serverTick;
        }
    }
}

using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerUIHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (NetworkUIManager.instance == null)
            {
                StartCoroutine(WaitForManagerAndIncrease());
                return;
            }
            NetworkUIManager.instance.IncreasePlayerCountServerRPC();
        }


    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) NetworkUIManager.instance.DecreasePlayerCountServerRPC();
    }



    IEnumerator WaitForManagerAndIncrease()
    {
        while (NetworkUIManager.instance == null)
            yield return null;

        NetworkUIManager.instance.IncreasePlayerCountServerRPC();
    }
}

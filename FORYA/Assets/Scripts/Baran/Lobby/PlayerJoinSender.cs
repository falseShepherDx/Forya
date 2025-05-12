using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerJoinSender : NetworkBehaviour
{
   [SerializeField] GameObject ReadyCanvas;
    public override void OnNetworkSpawn()
    {    
       if (IsOwner && IsClient)
        {
            StartCoroutine(SendNickNameWithDelay());
            //ReadyCanvas.SetActive(true);
        }
    }

    IEnumerator SendNickNameWithDelay()
    {
        yield return new WaitForSeconds(0.5f);

        string nickName = PlayerPrefs.GetString("PlayerNickName", "Guest");

        if (LobbyManager.instance != null)
        {
            LobbyManager.instance.AddPlayerPanelServerRPC(nickName);

        }
        
    }
  
}

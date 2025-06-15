using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtons : NetworkBehaviour
{
    public void Leave()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Disconnected from session.");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Ready()
    {
        Debug.Log("pressed");
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            Debug.Log("ready!");
            LobbyManager.instance.SetPlayerReadyServerRpc();
        }
    }

    public void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("KýrýlanKarolar", UnityEngine.SceneManagement.LoadSceneMode.Single);


    }

    public void CopyJoinCode()
    {
       GUIUtility.systemCopyBuffer = RelayManager.instance.lastJoinCode;
        Debug.Log("Join Code Copied to Clipboard");
    }

}

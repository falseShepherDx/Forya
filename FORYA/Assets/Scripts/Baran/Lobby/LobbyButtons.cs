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
        

        int index = LobbyManager.instance.gameIndex.Value;

        if (index < 0 || index >= LobbyManager.instance.scenes.Length)
        {
            Debug.LogError("Scene index out of range!");
            return;
        }

        int buildIndex = LobbyManager.instance.scenes[index];

        string sceneName = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
        NetworkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);

    }

    public void CopyJoinCode()
    {
        GUIUtility.systemCopyBuffer = RelayManager.instance.lastJoinCode;
        Debug.Log("Join Code Copied to Clipboard");
    }


    public void ChangeGame(bool isNext)
    {
        Debug.Log("buton basýldý");
        if (!IsClient)
        {
            Debug.Log("Host Check");
            LobbyManager.instance.ChangeGameIndexServerRpc(isNext);
        }

 
    }
}

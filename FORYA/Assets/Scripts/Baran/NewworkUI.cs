using Unity.Netcode;
using UnityEngine;

public class NewworkUI : MonoBehaviour
{

    public Canvas networkCanvas;
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideCanvas();
        Debug.Log("Host Started");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideCanvas();
        Debug.Log("Client Started");
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        HideCanvas();
    }

    public void HideCanvas()
    {
        if (networkCanvas != null)
        {
            networkCanvas.gameObject.SetActive(false);
        }
    }
}

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUI_B : MonoBehaviour
{
    public Canvas networkCanvas,lobbyCanvas;
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TextMeshProUGUI warnerText;

    public void StartHost()
    {
        if(!TakeName()) return;
        NetworkManager.Singleton.StartHost();
        HideCanvas();
        Debug.Log("Host Started");
    }

    public void StartClient()
    {
        if (!TakeName()) return;

        NetworkManager.Singleton.StartClient();
        HideCanvas();
        Debug.Log("Client Started");
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        HideCanvas();
    }

  

    bool TakeName()
    {
        if (string.IsNullOrWhiteSpace(nameField.text))
        {
            warnerText.color = Color.red;
            return false;
        }

        string nickName = nameField.text.Trim();
        PlayerPrefs.SetString("PlayerNickName", nickName);

        return true;
    }
    public void HideCanvas()
    {
        if (networkCanvas != null)
        {
            lobbyCanvas.gameObject.SetActive(true);
            networkCanvas.gameObject.SetActive(false);
          
        }
    }
}

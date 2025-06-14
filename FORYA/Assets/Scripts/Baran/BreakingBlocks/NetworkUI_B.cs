using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUI_B : MonoBehaviour
{
    public Canvas networkCanvas,lobbyCanvas;
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TextMeshProUGUI warnerText;

    public TMP_InputField joinCodeInput;
    public async void StartHost()
    {
        if(!TakeName()) return;
        string code = await RelayManager.instance.CreateRelayAsync();
        Debug.Log("Oluþan Join Code: " + code);
        HideCanvas();
    }

    public async void StartClient()
    {
        if (!TakeName()) return;

        string joinCode = joinCodeInput.text;
        bool success = await RelayManager.instance.JoinRelayAsync(joinCode);
        if (success) HideCanvas();
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

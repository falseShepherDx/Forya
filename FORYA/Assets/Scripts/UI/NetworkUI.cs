using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    void Awake()
    {
        Debug.Log("Awake: NetworkManager Singleton => " + NetworkManager.Singleton);
        Debug.Log("Awake: hostButton => " + hostButton);


        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene");
            return;
        }

        NetworkManager.Singleton.StartHost();
        HideButtons();
    }

    private void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene");
            return;
        }

        NetworkManager.Singleton.StartClient();
        HideButtons();
    }
    private void HideButtons()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}

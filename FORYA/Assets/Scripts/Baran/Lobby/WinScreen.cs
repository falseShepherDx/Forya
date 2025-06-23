using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : NetworkBehaviour
{
   public static WinScreen instance;
    [SerializeField] GameObject winCanvas;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Button lobbyButton;

   
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }

    [ClientRpc]
    public void ShowWinScreenClientRpc(string winnerName)
    {
        winCanvas.SetActive(true);
        winnerText.text = winnerName;
    }

    public void LobbyButton(string nextGame)
    {
        if (IsServer)
        {
            DespawnAllPlayers();
            NetworkManager.SceneManager.LoadScene(nextGame, LoadSceneMode.Single);
        }
    }

    private void DespawnAllPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true);
            }
        }
    }

}

using TMPro;
using Unity.Netcode;
using UnityEngine;
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
            NetworkManager.SceneManager.LoadScene(nextGame, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
       
    }
}

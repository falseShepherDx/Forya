using TMPro;
using Unity.Netcode;
using UnityEngine;

public class WinScreen : NetworkBehaviour
{
   public static WinScreen instance;
    [SerializeField] GameObject winCanvas;
    [SerializeField] TextMeshProUGUI winnerText;

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
}

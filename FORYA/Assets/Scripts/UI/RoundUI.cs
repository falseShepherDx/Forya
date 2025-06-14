using UnityEngine;
using TMPro;
using Unity.Netcode;

public class RoundUI : MonoBehaviour
{
    [Header("Alive Count Display")]
    [SerializeField] private TMP_Text aliveCountText;
    [Header("Winner Panel")]
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerNameText;
    
    
    public void UpdateAliveCount(int aliveCount)
    {
        aliveCountText.text = $"Players Alive: {aliveCount}";
    }
    
    public void ShowWinner(ulong clientId)
    {
        winnerPanel.SetActive(true);
        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        var identity = client.PlayerObject.GetComponent<PlayerIdentity>();
        winnerNameText.text = $"{identity.DisplayName} Wins!";
    }
}

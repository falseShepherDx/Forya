using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager instance;

    [SerializeField] TextMeshProUGUI playerCountText;
    public int playerCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else { Destroy(this); }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("as");
    }


    public void AddPlayer()
    {
        playerCount++;
        playerCountText.text = playerCount + " Player Alive!";
        Debug.Log(playerCount);
    }
    public void RemovePlayer()
    {
        playerCount--;
        playerCountText.text = playerCount + " Player Alive!";
    }

}

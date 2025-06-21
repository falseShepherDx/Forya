using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class PingDisplay : NetworkBehaviour
{
    public TextMeshProUGUI pingText;

    void Update()
    {
        

        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        ulong ping = transport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);

        pingText.text = $"Ping: {ping} ms";
    }
}

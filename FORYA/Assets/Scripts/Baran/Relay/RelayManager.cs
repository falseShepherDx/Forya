using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager instance;
    public string lastJoinCode = "";
    [SerializeField] TextMeshProUGUI joinCodeText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    public async Task InitializeServicesAsync()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Unity Services initialized and authenticated.");
        }
    }

    public async Task<string> CreateRelayAsync(int maxPlayers = 4)
    {
        await InitializeServicesAsync();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        lastJoinCode = joinCode;
        Debug.Log("Relay Created. Join Code: " + joinCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
        joinCodeText.text ="Join Code :"+ lastJoinCode;
        return joinCode;
    }

    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        await InitializeServicesAsync();

        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(joinAlloc, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined Relay with code: " + joinCode);
            joinCodeText.text = "Join Code: " + joinCode.ToString();
            return true;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError($"Relay Join Failed: {ex.Message}");
            return false;
        }
    }
}

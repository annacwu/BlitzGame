using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Button testRelayButton;

    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        // testRelayButton.onClick.AddListener(OnTestRelayButtonClicked);
    }

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        Debug.Log("starting initialize async");
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("singing in anon");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Debug.Log("creating allocation async");
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log("host started with relay");
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }


    private async void JoinRelay(string joinCode) {
        try {
            Debug.Log("Joining relay with " + joinCode);
            await RelayService.Instance.JoinAllocationAsync(joinCode);
        } catch (RelayServiceException e){
            Debug.Log(e);
        }
        
    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
    // private async void OnTestRelayButtonClicked()
    // {
    //     await StartHostWithRelay(3, "dtls");
    // }
}

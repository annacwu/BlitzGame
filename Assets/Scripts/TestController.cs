using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class TestController : MonoBehaviour
{
    public string targetSceneName = "MainGameScene";
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private LobbyManager lobbyManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.F3))
        {
            _ = UpdateAsync();
        }
    }

    private async Task UpdateAsync()
    {
        // if (Input.GetKeyDown(KeyCode.F1))
        // {
        //     Debug.Log("Starting host and loading scene...");
        //     // int maxConnections = 4;
        //     // string connectionType = "dtls"; // or "udp" if you're not using encryption
        //     // await RelayManager.Instance.StartHostWithRelay(maxConnections, connectionType);

        //     if (NetworkManager.Singleton.IsHost)
        //     {
        //         NetworkManager.Singleton.SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.F2))
        // {
        //     Debug.Log("making lobby...");
        //     lobbyManager.CreateLobbyWithRelay("Test Lobby", 2);
        // }
    }
}


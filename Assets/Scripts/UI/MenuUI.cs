using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private RelayManager relayManager;

    void Awake()
    {
        // lamba to handle the async task
        createButton.onClick.AddListener(() =>
        {
            // _ = OnCreateButtonClicked();
            OnCreateNewGamePressed();
        });
        joinButton.onClick.AddListener(() =>
        {
            OnJoinGamePressed();
        });
    }

    // private async Task GoToLobbyScene() {
    //     int maxConnections = 4;
    //     string connectionType = "dtls";
    //     if (NetworkManager.Singleton != null) {
    //         Debug.Log("Starting relay host");
    //         string joinCode = await relayManager.StartHostWithRelay(maxConnections, connectionType);
    //         Debug.Log("host started with join code: " + joinCode);
    //         if (NetworkManager.Singleton.SceneManager != null) {
    //             Debug.Log("loading lobby scene");
    //             NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    //         } else {
    //             Debug.Log("scene manager issue");
    //         }
    //     } else {
    //         Debug.Log("network manager is null");
    //     }    
    // }
    private void OnJoinGamePressed()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OnCreateNewGamePressed(){
        {
            SceneStateManager.Instance.LobbyMode = LobbyStartupMode.ShowCreateLobby;
            SceneManager.LoadScene("Lobby");
        }
    }
}

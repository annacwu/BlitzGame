using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private PlayerSpawnSystem playerSpawnSystem;

    private void Awake(){

        serverButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
            playerSpawnSystem.RegisterSpawnSystemEvents();
        });

        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started manually"); // it was never starting without the following line
            playerSpawnSystem.OnServerStarted();
            playerSpawnSystem.RegisterSpawnSystemEvents();
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient(); // something i don't understand is why client stuff works SEEMS LIKE IT DOESNT ACTUALLY
        });
    }
}

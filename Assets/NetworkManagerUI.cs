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
            playerSpawnSystem.RegisterSpawnSystemEvents();
        });
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartUI : NetworkBehaviour
{
   public static GameStartUI Instance { get; private set; } 

   [SerializeField] private GameObject gameStartPanel;
   [SerializeField] private Button newGameButton;
   [SerializeField] private Button closeButton;

    private void Awake() {
        Instance = this;
        // FOR SOME REASON THIS BREAKS THE BUTTON WHEN UNCOMMENTED
        // if (NetworkManager.Singleton != null){
        //     // Hide();
        //     NetworkManagerUI.Instance.Show();
        // }
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        closeButton.onClick.AddListener(Hide);
    }

    public void Hide() {
        gameStartPanel.SetActive(false);
   }

   private void GoToLobbyScene() {
        NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
   }

   private void OnNewGameButtonClicked() {
        NetworkManager.Singleton.StartHost();
        GoToLobbyScene();
   }
}

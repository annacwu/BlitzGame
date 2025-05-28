using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
     [SerializeField] private RelayManager relayManager;

    private void Awake()
     {
          Instance = this;
          // FOR SOME REASON THIS BREAKS THE BUTTON WHEN UNCOMMENTED
          // if (NetworkManager.Singleton != null){
          //     // Hide();
          //     NetworkManagerUI.Instance.Show();
          // }

          // lamba to handle the async task
          newGameButton.onClick.AddListener(() =>
          {
               _ = OnNewGameButtonClicked();
          });
          closeButton.onClick.AddListener(Hide);
     }

    public void Hide() {
        gameStartPanel.SetActive(false);
   }

   private async Task GoToLobbyScene() {
          int maxConnections = 4;
          string connectionType = "dtls";
          if (NetworkManager.Singleton != null)
          {
               Debug.Log("Starting relay host");
               string joinCode = await relayManager.StartHostWithRelay(maxConnections, connectionType);
               Debug.Log("host started with join code: " + joinCode);
               if (NetworkManager.Singleton.SceneManager != null)
               {
                    Debug.Log("loading lobby scene");
                    NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
               }
               else
               {
                    Debug.Log("scene manager issue");
               }
          }
          else
          {
               Debug.Log("network manager is null");
          }    
   }

     private async Task OnNewGameButtonClicked()
     {
          Debug.Log("button clicked");
          await GoToLobbyScene();
          Debug.Log("Gone to lobby");
   }
}

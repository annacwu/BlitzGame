using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyUI : MonoBehaviour
{
    public static JoinedLobbyUI Instance { get; private set; } // REMINDER: look into this line bc what
    [SerializeField] private TMP_Text lobbyName; 
    [SerializeField] private TMP_Text lobbyID;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private GameObject playerListContainer;
    [SerializeField] private GameObject joinedLobbyPanel;

    private string currentLobbyId;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevent multiple instances
            return;
        }
        Instance = this;
        joinedLobbyPanel.SetActive(false);
        leaveButton.onClick.AddListener(LeaveButtonClicked);
        refreshButton.onClick.AddListener(UpdateJoinedPlayers);
        startButton.onClick.AddListener(StartButtonClicked);
    }

    public void Show(string lobId, string lobName) {
        currentLobbyId = lobId;
        joinedLobbyPanel.SetActive(true); // Show the panel
        lobbyName.text = lobName; 
        lobbyID.text = lobId;
    }

    public void Hide() {
        joinedLobbyPanel.SetActive(false); // Hide the panel
    }

    private void LeaveButtonClicked(){
        LobbyManager.Instance.LeaveLobby();
        Hide(); 
    }

    public async void UpdateJoinedPlayers() {
        if (string.IsNullOrEmpty(currentLobbyId)) {
            Debug.LogError("cannot refresh players, no lobby id");
        }
        try{
            var lobby = await Lobbies.Instance.GetLobbyAsync(currentLobbyId);
            UpdatePlayerList(lobby.Players);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private void UpdatePlayerList(List<Player> playerList) {
        foreach (Transform child in playerListContainer.transform) {
            Destroy(child.gameObject);
        }

        foreach (Player player in playerList) {
            GameObject playerItem = Instantiate(playerListPrefab, playerListContainer.transform);
            playerItem.GetComponentInChildren<TextMeshProUGUI>().text = $"{player.Id}";
        }
    }

    private void StartButtonClicked(){
        LobbyManager.Instance.MoveToGame();
    }
}

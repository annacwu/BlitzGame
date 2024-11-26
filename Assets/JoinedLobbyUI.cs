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
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private GameObject joinedLobbyPanel;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevent multiple instances
            return;
        }
        Instance = this;
        joinedLobbyPanel.SetActive(false);
        leaveButton.onClick.AddListener(LeaveButtonClicked);
        // startButton.onClick.AddListener(StartGame);
    }

    public void Show(string lobId, string lobName) {
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

}

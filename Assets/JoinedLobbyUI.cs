using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyUI : MonoBehaviour
{
    public static JoinedLobbyUI Instance { get; private set; } // REMINDER: look into this line bc what
    [SerializeField] private TMP_Text lobbyName; 
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
        // startButton.onClick.AddListener(StartGame);
        // leaveButton.onClick.AddListener(Hide);
    }

    public void Show() {
        joinedLobbyPanel.SetActive(true); // Show the panel
    }

    public void Hide() {
        joinedLobbyPanel.SetActive(false); // Hide the panel
    }

}

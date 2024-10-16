using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }

    [SerializeField] private GameObject createLobbyPanel; // The panel to show
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button createLobbyConfirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake() {
        Instance = this;
        createLobbyPanel.SetActive(false); // Hide the panel initially
        createLobbyConfirmButton.onClick.AddListener(OnCreateLobbyConfirm);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show() {
        createLobbyPanel.SetActive(true); // Show the panel
    }

    public void Hide() {
        createLobbyPanel.SetActive(false); // Hide the panel
    }

    private void OnCreateLobbyConfirm() {
        // Validate inputs (you could add further validation)
        string lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName)) {
            Debug.Log("Lobby name cannot be empty");
            return;
        }

        // Call the LobbyManager to create a lobby
        LobbyManager.Instance.CreateLobby(lobbyName);

        // Hide the panel after creating the lobby
        Hide();
    }
}


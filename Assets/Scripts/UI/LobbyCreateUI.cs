using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }

    [SerializeField] private GameObject createLobbyPanel; 
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_Dropdown numPlayersInput;
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

    // this is a wrapper so that the functionality of the method is public, but the logic remains internal (something about encapsulation??)
    // this method might be too simple to warrant that but here we are
    public void OnCreateLobbyConfirm() {
        OnCreateLobbyConfirm_Internal();
    }
    private void OnCreateLobbyConfirm_Internal() {

        string lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName)) {
            Debug.Log("Lobby name cannot be empty");
            return;
        }

        int numPlayers = int.Parse(numPlayersInput.options[numPlayersInput.value].text);
        // Call the LobbyManager to create a lobby had to make that one public
        LobbyManager.Instance.CreateLobbyWithRelay(lobbyName, numPlayers);

        // Hide the panel after creating the lobby
        Hide();
    }
}


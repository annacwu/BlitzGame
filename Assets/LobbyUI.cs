using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    // idk what this means tbh
    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject lobbyListContainer;
    [SerializeField] private GameObject lobbyEntryPrefab;

    private void Awake() {
        Instance = this;
        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateButtonClick);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateButtonClick() {
        Debug.Log("Create button clicked. tryping to show instance.");
        LobbyCreateUI.Instance.Show();
    }


    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {

        // Clear the previous lobby list UI
        foreach (Transform child in lobbyListContainer.transform) {
            Destroy(child.gameObject);
        }

        // Dynamically create UI elements for each lobby in the list
        foreach (Lobby lobby in lobbyList) {
            GameObject lobbyEntry = Instantiate(lobbyEntryPrefab, lobbyListContainer.transform);
            lobbyEntry.GetComponentInChildren<Text>().text = $"{lobby.Name} ({lobby.MaxPlayers} players)";
            // FIXME: double check this i think things might need to be initialized differently
        }
    }
}

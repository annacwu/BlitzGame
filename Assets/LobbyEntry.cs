using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    public Button joinButton;
    private string lobbyId;
    private string lobbyName;

    public void Setup(Lobby lobby) {
        lobbyId = lobby.Id;
        lobbyName = lobby.Name;
        joinButton.onClick.AddListener(JoinLobby);
    }

    public void JoinLobby() {
        LobbyManager.Instance.JoinLobby(lobbyId);
        JoinedLobbyUI.Instance.Show(lobbyId, lobbyName);
    }
}

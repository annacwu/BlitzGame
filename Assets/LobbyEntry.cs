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

    // this is prob the issue with the joining and it now showing anything here. 
    // this is never referenced so its never getting the id
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

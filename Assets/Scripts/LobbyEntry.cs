using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Services.Lobbies;

public class LobbyEntry : MonoBehaviour
{
    public Button joinButton;
    private string lobbyId;
    private string lobbyName;
    private Lobby lobby;

    // this is prob the issue with the joining and it now showing anything here. 
    // this is never referenced so its never getting the id
    public void Setup(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyId = lobby.Id;
        lobbyName = lobby.Name;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => {
            LobbyManager.Instance.JoinLobbyWithRelay(lobbyId, lobbyName);
        });
    }

    // public void JoinLobby()
    // {
    //     LobbyManager.Instance.JoinLobby(lobbyId);
    //     JoinedLobbyUI.Instance.Show(lobbyId, lobbyName);
    // }
  
}


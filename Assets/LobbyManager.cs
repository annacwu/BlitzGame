using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    // i just learned what making an event was so this is that i think
    public event System.EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : System.EventArgs {
        public List<Lobby> lobbyList;
    }

    private Lobby hostLobby ;
    private float heartbeatTimer;


    private void Awake() {
        Instance = this;
    }


    private async void Start() {
        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        // make it so user doesn't have to make an account to sign in through steam or something
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update() {
        HandleLobbyHeartbeat();
    }

    // lobby dies if inactive for 30 sec, so this keeps it open
    private async void HandleLobbyHeartbeat() {
        if (hostLobby != null) {
            heartbeatTimer -= Time.deltaTime;
            // if timer has lapsed
            if (heartbeatTimer < 0f) {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void CreateLobby(string lobbyName, int numPlayers) {
        try {
            int maxPlayers = numPlayers;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
            PrintPlayers(hostLobby);

            // Immediately switch to the joined lobby screen since the player is already in this lobby
            JoinedLobbyUI.Instance.Show(lobby.Id, lobby.Name);

        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    // find lobbies with unity's API
    private async void ListLobbies() {
        try {
            // lists ALL lobbies active ever
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results) {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void RefreshLobbyList() {
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // order by newest
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

     // private async void JoinLobby() {
    //     try {
    //         QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
    //         // currently joins first one it finds
    //         await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
    //     } catch (LobbyServiceException e) {
    //         Debug.Log(e);
    //     }
    // }

    // THIS IS THE ONE WE'RE USING
    public async void JoinLobby(string lobbyId) {
        try {
            var lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log("Joined lobby with ID: " + lobbyId);
        
            JoinedLobbyUI.Instance.UpdateJoinedPlayers();
        } catch (LobbyServiceException e) {
         Debug.LogError("Failed to join lobby: " + e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode) {
        try {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("Joined lobby by code " + lobbyCode);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby() {
        try {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private void PrintPlayers(Lobby lobby) {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players) {
            Debug.Log(player.Id);
        }
    }

    public async void LeaveLobby(){
        try {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync("lobbyId", playerId);
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    // i just learned what making an event was so this is that i think
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : System.EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby hostLobby;
    private float heartbeatTimer;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private async void Start()
    {
        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // make it so user doesn't have to make an account to sign in through steam or something
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    // lobby dies if inactive for 30 sec, so this keeps it open
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            // if timer has lapsed
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    // public async void CreateLobby(string lobbyName, int numPlayers) {
    //     try {
    //         int maxPlayers = numPlayers;
    //         Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

    //         hostLobby = lobby;
    //         Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
    //         PrintPlayers(hostLobby);

    //         // Immediately switch to the joined lobby screen since the player is already in this lobby
    //         JoinedLobbyUI.Instance.Show(lobby.Id, lobby.Name);

    //     } catch (LobbyServiceException e) {
    //         Debug.Log(e);
    //     }
    // }

    public async void CreateLobbyWithRelay(string lobbyName, int numPlayers)
    {
        try
        {
            int maxPlayers = numPlayers;

            // Step 1: Start host with Relay and get the join code
            string connectionType = "dtls"; // or "udp" if unencrypted
            string joinCode = await RelayManager.Instance.StartHostWithRelay(maxPlayers - 1, connectionType); // minus 1 for host
            Debug.Log("join code relay: " + joinCode);

            // Step 2: Create lobby with the Relay join code in metadata
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject> {
                    {
                        "joinCode", new DataObject(
                            DataObject.VisibilityOptions.Public,
                            joinCode
                        )
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Lobby metadata joinCode: " + lobby.Data["joinCode"].Value);

            hostLobby = lobby;
            Debug.Log($"Created Lobby '{lobby.Name}' with Relay. Max Players: {lobby.MaxPlayers}");
            PrintPlayers(hostLobby);

            // Immediately switch to the joined lobby screen since the player is already in this lobby
            JoinedLobbyUI.Instance.Show(lobby.Id, lobby.Name);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("LobbyServiceException: " + e);
        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected Exception: " + e);
        }
    }


    // find lobbies with unity's API
    private async void ListLobbies()
    {
        try
        {
            // lists ALL lobbies active ever
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
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

            QueryResponse lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
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

    // // THIS IS THE ONE WE'RE USING
    // public async void JoinLobby(string lobbyId) {
    //     try {
    //         var lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
    //         Debug.Log("Joined lobby with ID: " + lobbyId);

    //         // NetworkManager.Singleton.StartClient(); // THIS IS BREAKING IT BTW

    //         JoinedLobbyUI.Instance.UpdateJoinedPlayers();
    //     } catch (LobbyServiceException e) {
    //      Debug.LogError("Failed to join lobby: " + e);
    //     }
    // }

    // private async void JoinLobbyByCode(string lobbyCode) {
    //     try {
    //         await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
    //         Debug.Log("Joined lobby by code " + lobbyCode);
    //     } catch (LobbyServiceException e) {
    //         Debug.Log(e);
    //     }
    // }

    // private async void QuickJoinLobby() {
    //     try {
    //         await LobbyService.Instance.QuickJoinLobbyAsync();
    //     } catch (LobbyServiceException e) {
    //         Debug.Log(e);
    //     }
    // }

    public async void JoinLobbyWithRelay(string lobbyId, string lobbyName)
    {
        try
        {
            Debug.Log($"Joining lobby: {lobbyId}, {lobbyName}");

            if (JoinedLobbyUI.Instance == null)
                Debug.LogError("JoinedLobbyUI.Instance is null");


            var joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            Debug.Log("successfully joined lobbyService lobby");

            // Step 1: Get join code from lobby metadata
            if (!joinedLobby.Data.TryGetValue("joinCode", out var joinCodeData))
            {
                Debug.LogError("No joinCode found in lobby metadata!");
                return;
            }

            string joinCode = joinCodeData.Value;
            Debug.Log("Got joinCode from metadata: '" + joinCode + "'");
            string connectionType = "dtls"; // or "udp"

            // Step 2: Join Relay and start client
            Debug.Log("trying to start client with relay");
            bool success = await RelayManager.Instance.StartClientWithRelay(joinCode, connectionType);
            Debug.Log("ran that command");

            if (success)
            {
                Debug.Log("Client started via Relay, showing joined lobby UI");
                JoinedLobbyUI.Instance.Show(lobbyId, lobbyName);
                JoinedLobbyUI.Instance.UpdateJoinedPlayers();
            }
            else
            {
                Debug.LogError("Failed to start client via Relay");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error joining lobby: {ex}");
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync("lobbyId", playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //moves everyone from the lobby to the MainGameScene
    public void MoveToGame()
    {
        Debug.Log($"IsServer: {NetworkManager.Singleton.IsServer}, Transport: {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name}");
        Debug.Log($"Connection Approval: {NetworkManager.Singleton.NetworkConfig.ConnectionApproval}");
        Debug.Log($"Scene Management Enabled: {NetworkManager.Singleton.NetworkConfig.EnableSceneManagement}");
        Debug.Log($"Protocol Version: {NetworkManager.Singleton.NetworkConfig.ProtocolVersion}");

        Debug.Log("MovingOn");
        if (IsHost)
        {
            NetworkManager.SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
        }
    }

    public List<Player> GetPlayers()
    {
        return hostLobby.Players;
    }

}

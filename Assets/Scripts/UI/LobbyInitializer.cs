using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySceneInitializer : MonoBehaviour
{
    public GameObject createLobbyUI;
    public GameObject lobbyUI;
    public GameObject joinedLobbyUI;

    private void Start()
    {
        var mode = SceneStateManager.Instance?.LobbyMode ?? LobbyStartupMode.Default;

        createLobbyUI.SetActive(false);
        lobbyUI.SetActive(false);
        joinedLobbyUI.SetActive(false);

        switch (mode)
        {
            case LobbyStartupMode.ShowCreateLobby:
                createLobbyUI.SetActive(true);
                break;
            case LobbyStartupMode.Default:
            default:
                lobbyUI.SetActive(true);
                break;
        }
    }
}
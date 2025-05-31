using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbyStartupMode { Default, ShowCreateLobby }

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance { get; private set; }

    public LobbyStartupMode LobbyMode { get; set; } = LobbyStartupMode.Default;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

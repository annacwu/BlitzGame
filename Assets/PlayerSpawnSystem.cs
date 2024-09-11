using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Networking.Transport;

public class PlayerSpawnSystem : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab = null; 

    // create list of spawnPoints player can be put
    private static List<Transform> spawnPoints = new List<Transform>();

    // index to keep track of which spawn point we are on
    private int nextIndex = 0;

    // create two events so this happens when the server starts (listener?)
    private void OnEnable()
    {
        Debug.Log("OnEnable in PlayerSpawnSystem called");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server is active, calling OnServerStarted");
            // Register the method immediately if the server is already started
            OnServerStarted();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    // separate method so that it does the network manager stuff before it looks for the server
    public void RegisterSpawnSystemEvents()
    {
        Debug.Log("called this funciton but not if statement");
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Registering spawn system events");
            // so something is wrong with the line below this it doesn't actually acll that function idk why
            // NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Make sure the NetworkManager is properly initialized.");
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted; // also this might not work then if the other one didnt
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // when that event occurs, do the player spawn including if host player is spawing
    public void OnServerStarted()
    {
        Debug.Log("OnServerStarted in PlayerSpawnSystem called");
        if (NetworkManager.Singleton.IsHost)
        {
            // Spawn the host player
            Debug.Log("Spawning host player");
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("OnClientConnected in PlayerSpawnSystem called");
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    // spawn system doesn't exist until player joins, so need methods to add and remove it
    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
        Debug.Log($"spawn point at {transform.position} added");
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    // how to actually spawn the player
    public void SpawnPlayer(ulong clientId)
    {
        Debug.Log($"SpawnPlayer called for client {clientId}");
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

        if (spawnPoint == null){
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }

        Debug.Log($"Spawning player {clientId} at spawn point {nextIndex} at position {spawnPoint.position} and rotation {spawnPoint.rotation}");

        // the rest is all from an old yt video but this is from the unity docs so this should work
        var playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
        var playerInstanceNetworkObject = playerInstance.GetComponent<NetworkObject>();
        playerInstanceNetworkObject.SpawnAsPlayerObject(clientId);

        // rotate camera based on set spawn point rotation
        Camera playerCamera = playerInstance.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            Debug.Log($"setting player camera to {spawnPoint.rotation}");
            playerCamera.transform.rotation = spawnPoint.rotation;
            Debug.Log($"camera now set to {playerCamera.transform.rotation}");
        }

        nextIndex++;
    }
}

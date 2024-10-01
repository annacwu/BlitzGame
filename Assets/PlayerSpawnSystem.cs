using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Networking.Transport;

public class PlayerSpawnSystem : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab = null; 
    [SerializeField] private GameObject tableObject = null;

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

    public void OnClientConnected(ulong clientId)
    {
        Debug.Log($"OnClientConnected: clientId {clientId} connected.");
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

        var playerInfo = playerInstance.GetComponent<PlayerScript>();
        playerInfo.positionNumber = nextIndex; // assign position based on which spawn point they got

        Debug.Log($"Player position number saved as {nextIndex}");

        RotateTableForPosition(playerInfo.positionNumber);

        // Debug.Log($"Current LocalClientId: {NetworkManager.Singleton.LocalClientId}");

        
        // Debug.Log($"Local Client Id: {NetworkManager.Singleton.LocalClientId}, client Id: {clientId}");
        // if (NetworkManager.Singleton.LocalClientId == clientId) {
        //     Debug.Log($"Attempting to rotate table for client {clientId}");
        //     RotateTableTowardPlayer(spawnPoints[nextIndex].position, clientId);
        // }

        nextIndex++;
    }

    // making this a client rpc method makes the server execute it on the client's screen so it is synchronized
    // but does not make it universal
    // [ClientRpc]
    // private void RotateTableTowardPlayerClientRpc(ulong clientId) {
    //     // ensure it is the right client
    //     if (NetworkManager.Singleton.LocalClientId != clientId) {
    //         return;
    //     }

    //     var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
    //     if (localPlayer == null) {
    //         Debug.LogError("Local player object not found.");
    //         return;
    //     }

    //     Vector3 directionToPlayer = localPlayer.transform.position - tableObject.transform.position;

    //     float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
    //     tableObject.transform.rotation = Quaternion.Euler(0,0, angle);

    //     Debug.Log($"Table rotated for client {clientId} at angle {angle}");
    // }


    // // old rotate method before clientrpc implemented
    // [ClientRpc]
    // private void RotateTableTowardPlayer(Vector3 playerPosition, ulong clientId)
    // {
    //     Debug.Log($"RotateTableTowardPlayerClientRpc called for client {clientId}");
    //     if (NetworkManager.Singleton.LocalClientId != clientId) {
    //         return;
    //     }
    //     // Get the direction from the table to the player
    //     Vector3 directionToPlayer = playerPosition - tableObject.transform.position;
        
    //     // Calculate the angle and apply the rotation
    //     float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
    //     tableObject.transform.rotation = Quaternion.Euler(0, 0, angle); // offset problem might be here

    //     Debug.Log($"Table rotated at angle {angle}");
    // }

    private void RotateTableTowardPlayer(Vector3 playerPosition) {
        
        // Get the direction from the table to the player
        Vector3 directionToPlayer = playerPosition - tableObject.transform.position;
        
        // Calculate the angle and apply the rotation
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        tableObject.transform.rotation = Quaternion.Euler(0, 0, angle); // offset problem might be here

        Debug.Log($"Table rotated at angle {angle}");
    }

     private void RotateTableForPosition(int playerInfo){
        Vector3 playerPosition = spawnPoints[playerInfo].position; 
        Debug.Log($"Rotating table for player at {playerInfo} to position {playerPosition}");
        RotateTableTowardPlayer(playerPosition);
    }
}

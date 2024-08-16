using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

// the GameManager handles player spawning
public class GameManager : MonoBehaviour
{
    // this code doesn't work yet bc it's from the internet
    // and i'm in the middle of figuring out what it means
    // so that i can add the right variable names and such
    // but basically it is supposed to override the basic add player
    // method in the network manager class and instead add players
    // to specific available spawn points
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    
    private void OnEnable()
    {
        // Register to listen for the event when a client connects
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        // Unregister the event
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // Check if we are the server
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        // Transform spawnPoint = spawnPoints[playerCount - 0]; // Adjust if needed for 0 or 1-based index

        // GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        // playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}

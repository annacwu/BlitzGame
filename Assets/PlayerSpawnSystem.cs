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
        if (NetworkManager.Singleton != null)
        {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
        Debug.LogError("NetworkManager.Singleton is null. Make sure the NetworkManager is present in the scene and initialized before PlayerSpawnSystem.");
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // when that event occurs, do the player spawn including if host player is spawing
    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // Spawn the host player
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
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
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    // how to actually spawn the player
    public void SpawnPlayer(ulong clientId)
    {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

        if (spawnPoint == null){
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }

        // the rest is all from an old yt video but this is from the unity docs so this should work
        var playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
        var playerInstanceNetworkObject = playerInstance.GetComponent<NetworkObject>();
        playerInstanceNetworkObject.Spawn();

        nextIndex++;
    }
}

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

    // spawn system doesn't exist until player joins, so need methods to add and remove it
    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    // how to actually spawn the player
    public void SpawnPlayer(NetworkConnection conn)
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

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    // custom variable to check how to rotate the table for the player to see
    [SerializeField] private Camera playerCam;
    public int positionNumber;

    public void Start()
    {
        if (IsOwner)
        {
            Debug.Log("camera is being enabled rn");
            if (playerCam == null)
            {
                Debug.LogError("Player camera is NULL!");
            }
            else
            {
                playerCam.enabled = true;
            }
        }
    }
}

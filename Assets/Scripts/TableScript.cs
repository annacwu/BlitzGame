using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TableScript : NetworkBehaviour
{   

    private GameObject StackManager;
    [SerializeField] private GameObject stackPrefab;

    void Start() {
        StackManager = GameObject.FindGameObjectWithTag("StackManager");
    }

    void Update() {
        // Debug.Log($"[Client] Table Position: {transform.position}, Rotation: {transform.rotation}");
    }

    //should check to see if one can create a new stack in the middle
    void OnMouseDown() {

        Debug.Log("HelloThisIsTheTableWhyAreYouKnocking");

        StackManagerScript managerScript = StackManager.GetComponent<StackManagerScript>();
        
        //if nothing is selected, do nothing
        if (!managerScript.isAStackSelected()) {
            //Debug.Log("no stack is selected");
            return;
        } 

        GameObject currentStack = managerScript.returnCurrentSelection();
        StackScript.CardValues topCard = currentStack.GetComponent<StackScript>().getTopCard();

        if (topCard == null || currentStack.GetComponent<StackScript>().canTransfer.Value == false) 
        {
            managerScript.deselectStack();
            //return;
        } else if (topCard.value == 1) {
            
            
            if (Mouse.current == null){
                Debug.LogWarning("Mouse input not recognized. Ensure the Input System is set up correctly.");
                return;
            }

            //Vector3 mouseScreenLoc = Input.mousePosition; //old read, not sure why no workie
            Vector3 mouseScreenLoc = Mouse.current.position.ReadValue();
            mouseScreenLoc.z = 70;
            Vector2 mouseRealLoc = Camera.main.ScreenToWorldPoint(mouseScreenLoc);
            //mouseRealLoc.z = 0;

            //Debug.Log("Placed new card at X = " + mouseRealLoc.x + " and Y =" + mouseRealLoc.y);

            
            
            /*GameObject newStack = Instantiate(stackPrefab, mouseRealLoc, transform.rotation, transform);

            newStack.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face); //add new card to selected stack*/
            
            createNewStackRpc(mouseRealLoc, topCard.value, topCard.color, topCard.face);
            currentStack.GetComponent<StackScript>().removeTopCard();

            managerScript.deselectStack(); //ensures we deselect the stack we got the card from - otherwise leads to errors
        } else {
            managerScript.deselectStack();
        }
        
        
        //Debug.Log("Function Complete");
    }

    //Instantiates new stack with the card that needs to be added to it. 
    [Rpc(SendTo.Server)]
    void createNewStackRpc (Vector2 mouseRealLoc, int value, Color color, string face) {
        GameObject newStack = Instantiate(stackPrefab, mouseRealLoc, transform.rotation, transform);
        NetworkObject newStackNetObj = newStack.GetComponent<NetworkObject>();
        newStackNetObj.Spawn(true);

        newStack.GetComponent<StackScript>().addCard(value, color, face); //add new card to selected stack
        newStack.GetComponent<StackScript>().canTransfer.Value = false;
        newStack.GetComponent<StackScript>().isOnTable.Value = true;
        
        //currentStack.GetComponent<StackScript>().removeTopCard();
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TableScript : MonoBehaviour
{   

    private GameObject StackManager;
    [SerializeField] private GameObject stackPrefab;

    void Start() {
        StackManager = GameObject.FindGameObjectWithTag("StackManger");
    }

    //should check to see if one can create a new stack in the middle
    void OnMouseDown() {

        StackManagerScript managerScript = StackManager.GetComponent<StackManagerScript>();
        
        //if nothing is selected, do nothing
        if (!managerScript.isAStackSelected()) {
            //Debug.Log("no stack is selected");
            return;
        } 

        GameObject currentStack = managerScript.returnCurrentSelection();
        StackScript.CardValues topCard = currentStack.GetComponent<StackScript>().getTopCard();

        if (topCard == null || currentStack.GetComponent<StackScript>().canTransfer == false) 
        {
            //Debug.Log("Stack has no cards or you may not transfer from this type of stack.");
            return;
        }

        //if value is 1, then we can create a new stack
        if (topCard.value == 1) {
            
            
            if (Mouse.current == null){
                Debug.LogWarning("Mouse input not recognized. Ensure the Input System is set up correctly.");
                return;
            }

            //Vector3 mouseScreenLoc = Input.mousePosition; //old read, not sure why no workie
            Vector3 mouseScreenLoc = Mouse.current.position.ReadValue();
            mouseScreenLoc.z = 10;
            Vector2 mouseRealLoc = Camera.main.ScreenToWorldPoint(mouseScreenLoc);
            //mouseRealLoc.z = 0;

            //Debug.Log("Placed new card at X = " + mouseRealLoc.x + " and Y =" + mouseRealLoc.y);

            GameObject newStack = Instantiate(stackPrefab, mouseRealLoc, transform.rotation, transform);

            newStack.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face); //add new card to selected stack
            currentStack.GetComponent<StackScript>().removeTopCard();

            managerScript.deselectStack(); //ensures we deselect the stack we got the card from - otherwise leads to errors
        } else {
            //Debug.Log("Top Card Value = " + topCard.value);
        }
        
        
        //Debug.Log("Function Complete");
    }
}

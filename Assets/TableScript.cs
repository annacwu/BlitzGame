using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableScript : MonoBehaviour
{   

    [SerializeField] private GameObject StackManager;
    [SerializeField] private GameObject stackPrefab;
    

    //yes the table needs a script.

    //should check to see if one can create a new stack in the middle
    void OnMouseDown() {

        StackManagerScript managerScript = StackManager.GetComponent<StackManagerScript>();
        
        //if nothing is selected, do nothing
        if (!managerScript.isAStackSelected()) {
            return;
        } 

        GameObject currentStack = managerScript.returnCurrentSelection();
        StackScript.CardValues topCard = currentStack.GetComponent<StackScript>().getTopCard();

        //if value is 1, then we can create a new stack
        if (topCard.value == 1) {
            Vector3 mouseScreenLoc = Input.mousePosition;
            Vector3 mouseRealLoc = Camera.main.ScreenToWorldPoint(mouseScreenLoc);
            mouseRealLoc.z = 0;

            GameObject newStack = Instantiate(stackPrefab, mouseRealLoc, transform.rotation, transform);

            newStack.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face); //add new card to selected stack
            currentStack.GetComponent<StackScript>().removeTopCard();
        }
        
        
        Debug.Log("Cools");
    }
}

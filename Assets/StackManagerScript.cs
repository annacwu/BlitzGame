using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// this object handles the logic behind stacks & transferring cards between stacks
/// this object should NOT be a networkobject - it will only work if it is local
/// this works bc this only handles the logic of transferring 
/// not what is contained in each stack
/// so, this decides where to send cards
/// and then those cards are updated on the network
/// (i am not confident that this actually works in practice we shall see)
/// </summary>
public class StackManagerScript : MonoBehaviour
{

    private bool stackSelected = false;
    private GameObject currentStack; //the stack currently selected

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}


    //handles selecting a stack - if already have a stack selected, then either deselects or replaces.
    //currently the only way to know what stack is selected is to look at the console :)
    //returns stackSelected so that the stack knows whether it is selected or not
    //TO ADD: 
    //deselect if you click not on a stack?? mayshlaps
    //visual indicator of stack selection, so you know what stack is selected
    public bool selectStack (GameObject selectedStack) {
        if (!stackSelected) {
            stackSelected = true;
            currentStack = selectedStack;
            Debug.Log("Selected Stack: " + currentStack.name);
            return stackSelected;
        } else if (stackSelected && currentStack == selectedStack) {
            stackSelected = false;
            currentStack = null;
            Debug.Log("Selected Stack: NONE");
            return stackSelected;
        } else if (stackSelected && currentStack != selectedStack) {
            currentStack = selectedStack;
            Debug.Log("Selected Stack: " + currentStack.name);
            return stackSelected;
        }
        return false;
    }
}

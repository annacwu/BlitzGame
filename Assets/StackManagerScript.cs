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

    [SerializeField] private Color selectedColor;


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
    //get color working (I have no clue why it is not)
    public bool selectStack (GameObject selectedStack) {
        if (!stackSelected) {
            stackSelected = true;
            currentStack = selectedStack;
            selectedStack.GetComponent<SpriteRenderer>().color = selectedColor; //sets color of selected stack to whatever the color is
            //Debug.Log("Selected Stack: " + currentStack.name);
            return stackSelected;
        } else if (stackSelected && currentStack == selectedStack) {
            stackSelected = false;
            currentStack = null;
            selectedStack.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1); //resets color of deselected stack to white
            //Debug.Log("Selected Stack: NONE");
            return stackSelected;
        } else if (stackSelected && currentStack != selectedStack) { //should handle transferring cards
            
            //should handle deciding if one can transfer cards.
            //if tranfer is possible: transfer, but do not change selection. 
            //if not possible: do not transfer, change selection to new stack. 

            StackScript.CardValues currentTopCard = null;
            StackScript.CardValues newTopCard = null;

            if (currentStack.GetComponent<StackScript>().getTopCard() != null && selectedStack.GetComponent<StackScript>().getTopCard() != null) {
                currentTopCard = currentStack.GetComponent<StackScript>().getTopCard();
                newTopCard = selectedStack.GetComponent<StackScript>().getTopCard();
            }
            
            //currentTopCard = currentStack.GetComponent<StackScript>().getTopCard();
            //newTopCard = selectedStack.GetComponent<StackScript>().getTopCard();

            if (currentTopCard != null && newTopCard != null && currentTopCard.value + 1 == newTopCard.value && currentTopCard.color == newTopCard.color) {
                //transfer
                selectedStack.GetComponent<StackScript>().addCard(currentTopCard.value, currentTopCard.color, currentTopCard.face); //add new card to selected stack
                currentStack.GetComponent<StackScript>().removeTopCard(); //remove card from old stack
                Debug.Log("Card Transferred");
            } else {
                //do not transfer
                currentStack.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                currentStack = selectedStack;
                selectedStack.GetComponent<SpriteRenderer>().color = selectedColor;
                Debug.Log("You may not tranfer this card");
                return stackSelected;
            }
        }
        return false;
    }
}

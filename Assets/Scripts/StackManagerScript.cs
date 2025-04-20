using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// this object handles the logic behind stacks & transferring cards between stacks
/// this object should NOT be a networkobject - it will only work if it is local
/// this works bc this only handles the logic of transferring 
/// not what is contained in each stack
/// so, this decides where to send cards
/// and then those cards are updated on the network
/// </summary>
public class StackManagerScript : MonoBehaviour 
{

    private bool stackSelected = false;
    private GameObject currentStack;

    private Color selectedColor = Color.blue;
    [SerializeField] private GameObject stackPrefab;
    [SerializeField] private GameObject spawnSystem;
    [SerializeField] private GameObject tablePrefab;
    [SerializeField] private GameObject BButtonPrefab;
    [SerializeField] private int mult;

    //NUMBER OF DECKS TO SPAWN, SHOULD BE REPLACED BY AUTOMATIC DETERMINATION OF HOW MANY PLAYERS ARE PLAYING
    [SerializeField] private int numDecksTEMP;

    //handles selecting a stack - if already have a stack selected, then either deselects or replaces.
    //currently the only way to know what stack is selected is to look at the console :)
    //returns stackSelected so that the stack knows whether it is selected or not

    //TO ADD: pressing escape (or other key but i think escape is good) should DESELECT whatever stack u had selected
    public void selectStack (GameObject clickedStack) {
        
        //otherwise preScript would be null
        StackScript preScript = null;
        if (stackSelected) {
            preScript = currentStack.GetComponent<StackScript>();
        }
        StackScript postScript = clickedStack.GetComponent<StackScript>();

        //if same, just deselect
        if (clickedStack == currentStack) {
            Debug.Log("No transfer possible: same stack");
            deselectStack();
            return;
        }

        if (!stackSelected && postScript.canTransfer.Value && postScript.getTopCard() != null) {
            //if no stack selected, select the clicked stack, so long as one can transfer from it.
            Debug.Log("Selected new stack!");
            stackSelected = true;
            currentStack = clickedStack;
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
            currentStack.GetComponent<StackScript>().toggleOutline(true);
            postScript.selected = true;
            return;
        }

        
        if (stackSelected && !postScript.canTransfer.Value && !postScript.canAcceptCards) {
            Debug.Log("No transfer possible: new stack cannot accept or give cards!");
            deselectStack();
        } else if (stackSelected && !postScript.canAcceptCards) {
            Debug.Log("No transfer possible: new stack cannot accept cards!");
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            preScript.toggleOutline(false);
            preScript.selected = false;

            currentStack = clickedStack;
            postScript.selected = true;
            currentStack.GetComponent<StackScript>().toggleOutline(true);
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
            return;
        } else if (stackSelected) {
            //main meat of this whole thing
            StackScript.CardValues preCard = preScript.getTopCard();
            StackScript.CardValues postCard = postScript.getTopCard();
            if (postCard == null) {
                Debug.Log("Card transferred (to empty stack)");
                //transfer. empty stacks can accept anything
                postScript.addCard(preCard.value, preCard.color, preCard.face);
                preScript.removeTopCard();
                deselectStack();
            } else if (preCard.value == postCard.value + 1 && preCard.color == postCard.color) {
                Debug.Log("Card transferred (normal conditions)");
                postScript.addCard(preCard.value, preCard.color, preCard.face);
                preScript.removeTopCard();
                deselectStack();
                //transfer
            } else {
                if (postScript.canTransfer.Value) {
                    Debug.Log("No transfer possible: value or color conditions not met!");
                    currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                    currentStack.GetComponent<StackScript>().toggleOutline(false);
                    preScript.selected = false;
                    currentStack = clickedStack;
                    postScript.selected = true;
                    currentStack.GetComponent<StackScript>().toggleOutline(true);
                    currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
                } else {
                    deselectStack();
                }
                
            }
        }

    }

    //commented out previous selection function
    /*
    //NETWORK PROOF I THINK. I DON'T THINK ANY OF THIS CODE HAS TO BE ON THE NETWORK ACTUALLY SO THAT'S COOL
    public bool selectStack (GameObject selectedStack) {
        //ulong stackNetworkID = selectedStack.GetComponent<NetworkObject>().NetworkObjectId; //don't actually have to use this i think

        //if no stack is selected, selected stack that was clicked on
        if (!stackSelected) {
            stackSelected = true;
            currentStack = selectedStack;
            selectedStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor; //sets color of selected stack to whatever the color is

            return stackSelected;

        //if you click a stack that is already selected, deselect it
        } else if (stackSelected && currentStack == selectedStack) {

            stackSelected = false;
            currentStack = null;
            selectedStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            return stackSelected;

        } else if (stackSelected && currentStack != selectedStack) { //should handle transferring cards

            //should handle deciding if one can transfer cards.
            
            //if tranfer is possible: transfer, but do not change selection. 
            //if not possible: do not transfer, change selection to new stack. 
            StackScript.CardValues currentTopCard = null;
            StackScript.CardValues newTopCard = null;

            StackScript cScript = currentStack.GetComponent<StackScript>();
            StackScript sScript = selectedStack.GetComponent<StackScript>();;

            //if transferring from deck to acceptor pile, call function to transfer 3 at a time
            if (cScript.isDeck && sScript.isAcceptorPile) {
                transferThree(currentStack, selectedStack);
                return stackSelected;
            } else if (cScript.isAcceptorPile && sScript.isDeck && sScript.getTopCard() == null) {
                resetAcceptor(currentStack, selectedStack);
                return stackSelected;
            } else {
                currentTopCard = currentStack.GetComponent<StackScript>().getTopCard();
                newTopCard = selectedStack.GetComponent<StackScript>().getTopCard();
            }
            
            //normal card transferring
            if (cScript.getTopCard() != null && sScript.getTopCard() != null && sScript.canAcceptCards) {
                //Debug.Log("transferring from a deck: " + cScript.isDeck + "transferring to an acceptor pile: " + sScript.isAcceptorPile);
            } else if (cScript.getTopCard() != null && cScript.canTransfer.Value && sScript.canAcceptCards) {
                //empty stacks can accept any card
                currentTopCard = currentStack.GetComponent<StackScript>().getTopCard();
                selectedStack.GetComponent<StackScript>().addCard(currentTopCard.value, currentTopCard.color, currentTopCard.face);
                currentStack.GetComponent<StackScript>().removeTopCard();
                return stackSelected;
            } else {
                //if transferring stack is empty, simply deselect all. 
                deselectStack();
                return false;
            }

            if (currentTopCard.value - 1 == newTopCard.value && currentTopCard.color == newTopCard.color && cScript.canTransfer.Value == true && !sScript.isAcceptorPile) {
                //transfer
                //Debug.Log("Trying to Transfer!!");
                selectedStack.GetComponent<StackScript>().addCard(currentTopCard.value, currentTopCard.color, currentTopCard.face); //add new card to selected stack
                currentStack.GetComponent<StackScript>().removeTopCard(); //remove card from old stack
                //Debug.Log("Card Transferred");
            } else {
                //do not transfer
                currentStack.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                currentStack = selectedStack;
                selectedStack.GetComponent<SpriteRenderer>().color = selectedColor;
                //Debug.Log("You may not tranfer this card");
                return stackSelected;
            }
        }
        return false;
    } */

    //spawns cards in a configuration for the start of the game
    //CAN ONLY BE RUN ON THE SERVER. we can either fix this down the line or make it a feature (which i think makes sense anyways)
    public void startGame(/*GameObject[] players, int numPlayers*/ /*takes in players / position of players maybe?*/) {
        //  a. a deck consists of 40 cards, 4 of each number (1-10) in each of the 4 colors (red, blue, yellow (?), green)

        GameObject table = Instantiate(tablePrefab);
        var tableNetworkObject = table.GetComponent<NetworkObject>();
        tableNetworkObject.Spawn(true);
        Debug.Log($"[Server] Table Position: {table.transform.position}, Rotation: {table.transform.rotation}");

        for (int i = 0; i < numDecksTEMP; i++) {
            //computes where decks should go (TEMP)
            //Vector3 location = spawnSystem.transform.GetChild(i).transform.position;
            Vector3 zeroPos = new Vector3(0, -45 * mult, 0); //position of spawnpoint 1

            Quaternion rotation = spawnSystem.transform.GetChild(i).transform.rotation;
            Quaternion zeroRot = new Quaternion(0, 0, 0, 0);

            //rotates table to try and get it to work :3
            //Debug.Log(spawnSystem.transform.GetChild(i).name + "'s starting rotation: " + rotation.z);
            table.transform.rotation = rotation;
            table.transform.Rotate(0.0f, 0.0f, -90.0f);
            //table.transform.position = zeroPos;

            Vector3 deckPos = zeroPos; //position where we should spawn the deck (below the other cards, slightly left so u can flip cards to the right)
            deckPos.x -= 10 * mult; //horizontal offset
            deckPos.y -= 20 * mult; //vertical offset
            deckPos.z = -1; //closer to cam, so that collider triggers first
            Vector3 firstCardPos = zeroPos; //position where we should spawn the deck of 10 cards
            firstCardPos.x += 30 * mult;

            //spawns decks
            GameObject newDeck = Instantiate(stackPrefab, deckPos, zeroRot, table.transform); //this is a template position - ideally, we'd use the position + rotation of the player
            //createFullDeck(newDeck, "template face"); //also template for now :)
            var newDeckNetworkObject = newDeck.GetComponent<NetworkObject>();
            newDeckNetworkObject.Spawn(true);
            newDeckNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            createFullDeck(newDeck, "template face"); //moving this line of code down fixed a bunch of errors I was getting and I have no idea why :)
            newDeck.GetComponent<StackScript>().shuffle();
            newDeck.GetComponent<StackScript>().isDeck = true;
            newDeck.GetComponent<StackScript>().canTransfer.Value = false;
            newDeck.GetComponent<StackScript>().canAcceptCards = false;
            newDeck.GetComponent<StackScript>().faceOtherWay();

            //instantiates acceptor pile
            deckPos.x += 20 * mult;
            GameObject newAcceptorPile = Instantiate(stackPrefab, deckPos, zeroRot, table.transform);
            var newAcceptorPileNetworkObject = newAcceptorPile.GetComponent<NetworkObject>();
            newAcceptorPileNetworkObject.Spawn(true);
            newAcceptorPileNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            newAcceptorPile.GetComponent<StackScript>().isAcceptorPile = true;
            newAcceptorPile.GetComponent<StackScript>().canAcceptCards = false;
            newAcceptorPile.GetComponent<StackScript>().canTransfer.Value = true;

            //spawns blitz button
            deckPos.x += (20 * mult);
            GameObject blitzButton = Instantiate(BButtonPrefab, deckPos, zeroRot, table.transform);
            var blitzButtonNetworkObject = blitzButton.GetComponent<NetworkObject>();
            blitzButtonNetworkObject.Spawn(true);
            blitzButtonNetworkObject.transform.SetParent(table.transform);
            blitzButton.transform.GetChild(0).transform.GetChild(0).GetComponent<Button>().onClick.AddListener(doSomething);
            //sets up game

            //sets up the stack of 10
            //zeroPos.x -= 20;
            GameObject stackOf10 = Instantiate(stackPrefab, firstCardPos, zeroRot, table.transform);
            var stackOf10NetworkObject = stackOf10.GetComponent<NetworkObject>();
            stackOf10NetworkObject.Spawn(true);
            stackOf10NetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            for (int j = 0; j < 10; j++) {
                StackScript.CardValues topCard = newDeck.GetComponent<StackScript>().getTopCard();
                stackOf10.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face);
                newDeck.GetComponent<StackScript>().removeTopCard();
            }
            stackOf10.GetComponent<StackScript>().canAcceptCards = false;

            //sets up the three other stacks
            for (int j = 0; j < 3; j++) {
                firstCardPos.x -= 20 * mult;
                GameObject newStack = Instantiate(stackPrefab, firstCardPos, zeroRot, table.transform);
                var newStackNetworkObject = newStack.GetComponent<NetworkObject>();
                newStackNetworkObject.Spawn(true);
                newStackNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
                StackScript.CardValues topCard = newDeck.GetComponent<StackScript>().getTopCard();
                newStack.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face);
                newDeck.GetComponent<StackScript>().removeTopCard();
            }

            //rotates back
            table.transform.rotation = zeroRot;
            
            /*Vector3 tablePosition = table.transform.position;
            tablePosition.z -= 1;
            table.transform.SetPositionAndRotation(tablePosition, zeroRot);*/
        }

        

        //2. calls function to shuffle the deck
        //3. doles out cards to correct stacks
        //4. repeat for each player
    }

    private void doSomething () {
        Debug.Log("Did something");
    }

    //adds 1 card of each color / value combo to make a full deck. since faces aren't implemented yet that's mostly just a placeholder. 
    private void createFullDeck (GameObject deck, string face) {
        Color[] colors = {Color.blue, Color.green, Color.yellow, Color.red};
        StackScript currentDeckScript = deck.GetComponent<StackScript>();

        for (int i = 0; i < 10; i++) {
            for (int j = 0; j < 4; j++) {
                currentDeckScript.addCard(i + 1, colors[j], face);
            }
        }
    }

    //returns bool, if true a stack is currently selected. 
    public bool isAStackSelected() {
        return stackSelected;
    }

    //returns currently selected stack
    public GameObject returnCurrentSelection () {
        return currentStack;
    }

    //resets selection so that no stack is currently selected
    public void deselectStack () {
        stackSelected = false;
        if (currentStack != null) {
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            currentStack.GetComponent<StackScript>().toggleOutline(false);
            currentStack.GetComponent<StackScript>().selected = false;
            currentStack = null;
        }
    }

    //transfer 3 cards from the deck to the acceptor pile. if there are no cards left, transfer back to deck from acceptor. 
    private void transferThree (GameObject deck, GameObject acceptor) {
        StackScript dScript = deck.GetComponent<StackScript>();
        StackScript aScript = acceptor.GetComponent<StackScript>();

        StackScript.CardValues dTop = dScript.getTopCard();

        for (int i = 0; i < 3; i++) {
            if (dTop != null) {
                aScript.addCard(dTop.value, dTop.color, dTop.face);
                dScript.removeTopCard();
                dTop = dScript.getTopCard();
            } 
        }
    }

    //put all cards in the acceptor pile back in the right order in the deck
    private void resetAcceptor(GameObject acceptor, GameObject deck) {
        StackScript aScript = acceptor.GetComponent<StackScript>();
        StackScript dScript = deck.GetComponent<StackScript>();
        StackScript.CardValues aTop = aScript.getTopCard();

        while (aTop != null) {
            dScript.addCard(aTop.value, aTop.color, aTop.face);
            aScript.removeTopCard();
            aTop = aScript.getTopCard();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using TMPro;
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
public class StackManagerScript : NetworkBehaviour
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

    private LobbyManager lmanager;
    private NetworkManager networkManager;
    [SerializeField] private Canvas mainCanvas;

    private List<NetworkObject> acceptors = new List<NetworkObject>(); //list of acceptor piles, index = clientID of client who owns the stack
    private List<NetworkObject> decks = new List<NetworkObject>(); //list of decks, index = clientID of client who owns the stack


    //handles selecting a stack - if already have a stack selected, then either deselects or replaces.
    //currently the only way to know what stack is selected is to look at the console :)
    //returns stackSelected so that the stack knows whether it is selected or not

    //TO ADD: pressing escape (or other key but i think escape is good) should DESELECT whatever stack u had selected
    public void selectStack(GameObject clickedStack/*, int playerID*/)
    {

        //otherwise preScript would be null
        StackScript preScript = null;
        if (stackSelected)
        {
            preScript = currentStack.GetComponent<StackScript>();
        }
        StackScript postScript = clickedStack.GetComponent<StackScript>();

        //if same, just deselect
        if (clickedStack == currentStack)
        {
            //Debug.Log("No transfer possible: same stack");
            deselectStack();
            return;
        }


        if (!stackSelected && postScript.canTransfer.Value && postScript.getTopCard() != null && postScript.IsOwner)
        {
            //if no stack selected, select the clicked stack, so long as one can transfer from it.
            //Debug.Log("Selected new stack!");
            stackSelected = true;
            currentStack = clickedStack;
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
            currentStack.GetComponent<StackScript>().toggleOutline(true);
            postScript.selected = true;
            return;
        }


        if (stackSelected && !postScript.canTransfer.Value && !postScript.canAcceptCards)
        {
            //Debug.Log("No transfer possible: new stack cannot accept or give cards!");
            deselectStack();
        }
        else if (stackSelected && !postScript.IsOwner && !postScript.IsOwnedByServer)
        {
            //Debug.Log("No transfer possible: not owner of new stack!");
            deselectStack();
        }
        else if (stackSelected && !postScript.canAcceptCards)
        {
            //Debug.Log("No transfer possible: new stack cannot accept cards!");
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            preScript.toggleOutline(false);
            preScript.selected = false;

            currentStack = clickedStack;
            postScript.selected = true;
            currentStack.GetComponent<StackScript>().toggleOutline(true);
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
            return;
        }
        else if (stackSelected)
        {
            //main meat of this whole thing
            StackScript.CardValues preCard = preScript.getTopCard();
            StackScript.CardValues postCard = postScript.getTopCard();
            if (postCard == null)
            {
                //Debug.Log("Card transferred (to empty stack), new numCards: " + postScript.getNumCards() + "oldstack numCards: " + preScript.getNumCards());
                //transfer. empty stacks can accept anything
                postScript.addCard(preCard.value, preCard.color, preCard.face);
                preScript.removeTopCard();
                deselectStack();
            }
            else if (preCard.value == postCard.value + 1 && preCard.color == postCard.color)
            {
                //Debug.Log("Card transferred (normal conditions), new numCards: " + postScript.getNumCards() + "oldstack numCards: " + preScript.getNumCards());
                postScript.addCard(preCard.value, preCard.color, preCard.face);
                preScript.removeTopCard();
                deselectStack();
                //transfer
            }
            else
            {
                if (postScript.canTransfer.Value)
                {
                    //Debug.Log("No transfer possible: value or color conditions not met!");
                    currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                    currentStack.GetComponent<StackScript>().toggleOutline(false);
                    preScript.selected = false;
                    currentStack = clickedStack;
                    postScript.selected = true;
                    currentStack.GetComponent<StackScript>().toggleOutline(true);
                    currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = selectedColor;
                }
                else
                {
                    deselectStack();
                }

            }
        }

    }

    //spawns cards in a configuration for the start of the game
    //CAN ONLY BE RUN ON THE SERVER. we can either fix this down the line or make it a feature (which i think makes sense anyways)
    public void startGame()
    {

        /*
        lmanager = GameObject.FindGameObjectWithTag("LobbyManager").GetComponent<LobbyManager>();
        Debug.Log("players: " + lmanager.GetPlayers()); */

        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        foreach (ulong clientID in networkManager.ConnectedClientsIds)
        {
            Debug.Log("Client IDs: " + clientID);
            displayClientIDRpc(clientID, RpcTarget.Single(clientID, RpcTargetUse.Temp)); //should send command only to specific client
            //clientText.text = "ClientId = " + NetworkManager.ConnectedClientsIds[counter];
            //counter++;
        }

        //  a. a deck consists of 40 cards, 4 of each number (1-10) in each of the 4 colors (red, blue, yellow (?), green)

        GameObject table = Instantiate(tablePrefab);
        var tableNetworkObject = table.GetComponent<NetworkObject>();
        tableNetworkObject.Spawn(true);
        //Debug.Log($"[Server] Table Position: {table.transform.position}, Rotation: {table.transform.rotation}");

        int counter = 0;

        foreach (ulong clientID in networkManager.ConnectedClientsIds)
        {
            //computes where decks should go (TEMP)
            //Vector3 location = spawnSystem.transform.GetChild(i).transform.position;


            //int currentPlayer = players[i];
            //player that owns current decks


            Vector3 zeroPos = new Vector3(0, -45 * mult, 0); //position of spawnpoint 1

            Quaternion rotation = spawnSystem.transform.GetChild(counter).transform.rotation;
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
            newDeckNetworkObject.SpawnWithOwnership(clientID, true);
            newDeckNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            createFullDeck(newDeck, "template face"); //moving this line of code down fixed a bunch of errors I was getting and I have no idea why :)
            newDeck.GetComponent<StackScript>().shuffle();
            newDeck.GetComponent<StackScript>().isDeck = true;
            newDeck.GetComponent<StackScript>().canTransfer.Value = false;
            newDeck.GetComponent<StackScript>().canAcceptCards = false;
            //newDeck.GetComponent<StackScript>().setOwner(currentPlayer); //sets owner of stack
            newDeck.GetComponent<StackScript>().faceOtherWay();
            setStackOwnerTextRpc(newDeckNetworkObject.NetworkObjectId, clientID);

            //add deck to decks
            decks.Add(newDeckNetworkObject);


            //instantiates acceptor pile
            deckPos.x += 20 * mult;
            GameObject newAcceptorPile = Instantiate(stackPrefab, deckPos, zeroRot, table.transform);
            var newAcceptorPileNetworkObject = newAcceptorPile.GetComponent<NetworkObject>();
            newAcceptorPileNetworkObject.SpawnWithOwnership(clientID, true);
            newAcceptorPileNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            newAcceptorPile.GetComponent<StackScript>().isAcceptorPile = true;
            newAcceptorPile.GetComponent<StackScript>().canAcceptCards = false;
            newAcceptorPile.GetComponent<StackScript>().canTransfer.Value = true;
            setStackOwnerTextRpc(newAcceptorPileNetworkObject.NetworkObjectId, clientID);

            //add acceptor pile to acceptors
            acceptors.Add(newAcceptorPileNetworkObject);

            //sets up the stack of 10
            //zeroPos.x -= 20;
            GameObject stackOf10 = Instantiate(stackPrefab, firstCardPos, zeroRot, table.transform);
            var stackOf10NetworkObject = stackOf10.GetComponent<NetworkObject>();
            stackOf10NetworkObject.SpawnWithOwnership(clientID, true);
            stackOf10NetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
            for (int j = 0; j < 10; j++)
            {
                StackScript.CardValues topCard = newDeck.GetComponent<StackScript>().getTopCard();
                stackOf10.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face);
                newDeck.GetComponent<StackScript>().removeTopCard();
            }
            stackOf10.GetComponent<StackScript>().canAcceptCards = false;
            setStackOwnerTextRpc(stackOf10NetworkObject.NetworkObjectId, clientID);

            //sets up the three other stacks
            for (int j = 0; j < 3; j++)
            {
                firstCardPos.x -= 20 * mult;
                GameObject newStack = Instantiate(stackPrefab, firstCardPos, zeroRot, table.transform);
                var newStackNetworkObject = newStack.GetComponent<NetworkObject>();
                newStackNetworkObject.SpawnWithOwnership(clientID, true);
                newStackNetworkObject.transform.parent = table.transform; //fixes the parent issue >?>?>?
                StackScript.CardValues topCard = newDeck.GetComponent<StackScript>().getTopCard();
                newStack.GetComponent<StackScript>().addCard(topCard.value, topCard.color, topCard.face);
                newDeck.GetComponent<StackScript>().removeTopCard();
                setStackOwnerTextRpc(newStackNetworkObject.NetworkObjectId, clientID);
            }

            //rotates back
            table.transform.rotation = zeroRot;

            /*Vector3 tablePosition = table.transform.position;
            tablePosition.z -= 1;
            table.transform.SetPositionAndRotation(tablePosition, zeroRot);*/
            counter++;
        }



        //2. calls function to shuffle the deck
        //3. doles out cards to correct stacks
        //4. repeat for each player
    }

    //set display of clientid in all clients, theoretisch
    [Rpc(SendTo.SpecifiedInParams)]
    void displayClientIDRpc(ulong clientID, RpcParams rpcParams = default)
    {
        TMP_Text clientText = mainCanvas.GetComponentInChildren<TMP_Text>();
        clientText.text = "ClientID = " + clientID;
    }

    [Rpc(SendTo.Everyone)]
    void setStackOwnerTextRpc(ulong StackID, ulong clientID)
    {
        NetworkObject targetStack = GetNetworkObject(StackID);
        targetStack.GetComponentInChildren<Canvas>().GetComponentInChildren<TMP_Text>().text = "OwnerID = " + clientID;
    }

    //adds 1 card of each color / value combo to make a full deck. since faces aren't implemented yet that's mostly just a placeholder. 
    private void createFullDeck(GameObject deck, string face)
    {
        Color[] colors = { Color.blue, Color.green, Color.yellow, Color.red };
        StackScript currentDeckScript = deck.GetComponent<StackScript>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                currentDeckScript.addCard(i + 1, colors[j], face);
            }
        }
    }

    //returns bool, if true a stack is currently selected. 
    public bool isAStackSelected()
    {
        return stackSelected;
    }

    //returns currently selected stack
    public GameObject returnCurrentSelection()
    {
        return currentStack;
    }

    //resets selection so that no stack is currently selected
    public void deselectStack()
    {
        stackSelected = false;
        if (currentStack != null)
        {
            currentStack.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            currentStack.GetComponent<StackScript>().toggleOutline(false);
            currentStack.GetComponent<StackScript>().selected = false;
            currentStack = null;
        }
    }

    //transfer (up to) three cards from the current player's deck to the current player's acceptor pile
    //if there are no cards in the player's deck, instead transfer all cards back to the deck (in the correct order)
    public void transferThree()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        ulong currentClientID = networkManager.LocalClientId;
        //Debug.Log("Client who clicked the button: " + currentClientID);
        transferThreeRpc(currentClientID);
    }

    //only server keeps track of who owns what acceptor pile / deck, so need server to run the transfer 3 code methinks
    [Rpc(SendTo.Server)]
    private void transferThreeRpc(ulong clientID)
    {
        NetworkObject deckObj = decks[unchecked((int)clientID)]; //converts ulong to int i think
        NetworkObject acceptorObj = acceptors[unchecked((int)clientID)];

        if (deckObj.GetComponent<StackScript>().getTopCard() != null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (deckObj.GetComponent<StackScript>().getTopCard() != null)
                {
                    StackScript.CardValues transferCard = deckObj.GetComponent<StackScript>().getTopCard();
                    acceptorObj.GetComponent<StackScript>().addCard(transferCard.value, transferCard.color, transferCard.face);
                    deckObj.GetComponent<StackScript>().removeTopCard();
                }
            }
        }
        else
        {
            while (acceptorObj.GetComponent<StackScript>().getTopCard() != null)
            {
                StackScript.CardValues transferCard = acceptorObj.GetComponent<StackScript>().getTopCard();
                deckObj.GetComponent<StackScript>().addCard(transferCard.value, transferCard.color, transferCard.face);
                acceptorObj.GetComponent<StackScript>().removeTopCard();
            }
        }
        //transferring code goes here :)
    }

    //called when blitz button is pressed
    //checks whether conditions have been met, and if so, ends the game
    public void blitz()
    {
        /*
        if (stack of ten is empty) {
            end the game, display scores (in new screen or overlay of some kind)
        }
        */
    }
    
    [Rpc(SendTo.Server)]
    private void blitzRpc(ulong clientID)
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Mono.Cecil;
using TMPro;
using Unity.Netcode;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class StackScript : NetworkBehaviour
{   

    [SerializeField] private Sprite faceUpSprite;
    [SerializeField] private Sprite faceDownSprite;
    
    //track which way a stack is facing
    private NetworkVariable<bool> isFacingUp = new(true);

    //public bool isAcceptorPile = false; //acceptor pile denotes the stack that accepts 3 cards at a time from the deck

    private bool selected = false; //determines whether the stack has been clicked once (e.g to move a card)
    private StackManagerScript smanager;
    public GameObject cardPrefab;
    [SerializeField] public bool isDeck = false; //decks start with a full deck of cards, which is shuffled automatically. Decks are spawned in when the game starts, & handle the whole doling out cards thing.
    public bool isAcceptorPile = false;
    public bool canAcceptCards = true;

    public NetworkVariable<bool> canTransfer = new(true); //you cannot transfer from the main deck, or from stacks placed in the middle. 

   // private bool canAcceptCards; //decks and the stack of 10 cards cannot accept any cards. This variable will be set when a stack is created. 
    //a comment 

    private int numCards = 0; //counts # of cards in the linked list

    //private NetworkVariable<int> sortOrder = new(0); //stores what sorting order the next card should have

    public class CardValues {
        public bool destroyed;
        public int value; //number on the card, 2-10
        public Color color; //color of the card, determining where it can be placed
        public string face; //the symbol on the back, determining who owns the card, and gets points from it
        public ulong inGameObjectID; //reference to the actual Card object in the scene, so we can do stuff to it later
        //NetworkObject inGameObject; //not sure if we need this - if we do i'll reimplement it

        public CardValues (int v, Color c, string f, ulong g) {
            value = v;
            color = c;
            face = f;
            inGameObjectID = g;
            //inGameObject = GetNetworkObject(g);
            destroyed = false;
        }

        /*
        public void destroyObject(){
            Destroy(GetNetworkObject(inGameObjectID)); //not entirely sure this works. oh well. 
            Debug.Log("Object Destroyed"); 
        }*/
    }

    //when you load into the game, this should be filled according to what kind of stack it is
    //then, we can update it when the player moves cards around
    LinkedList<CardValues> cards = new LinkedList<CardValues>();
    // Start is called before the first frame update
    
    void Start()
    {
        smanager = GameObject.FindWithTag("StackManager").GetComponent<StackManagerScript>();
        //.GetComponent<StackManagerScript>();
        //instantiate cards in the stack (they should be networkobjects, but maybe we can deal with that later?)
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}


    //calls selectStack in the stackmanager.
    //doesn't need to be on network bc local. 
    public void selectThisStack () {
        //Debug.Log("Trying to select a stack - StackScript");
        selected = smanager.selectStack(gameObject);
    }

    //NOT NETWORK READY (but does it have to be network ready idk man)
    public void addBlankCard() {
        addCard(0, new Color(1, 1, 1, 1), null);
        numCards++;
    }

    //add card
    //at this point just a shell of a function that calls other functions. what even is its purpose. it is nothing. i have coded away its meaning
    //I THINK NETWORK READY
    public void addCard (int value, Color color, string face) {
        //sets sorting order networkvariable to ensure cards are displayed right
        spawnCardRpc(value, color, face);
        numCards++;
    }

    //take top card, put it somewhere else?
    //IN THEORY NETWORK READY
    public void removeTopCard () {
        //CardValues topCard = cards.First.Value;
        //Debug.Log(cards.First.Value);
        if (cards.First != null) {
            CardValues topCard = cards.First.Value;

            destroyCardRpc(topCard.inGameObjectID);
            
            /*
            NetworkObject realCard = GetNetworkObject(topCard.inGameObjectID);
            GameObject realerCard = realCard.gameObject;
            Destroy(realerCard);*/
            
            removeCardRpc(this.NetworkObjectId);
            numCards--;
        }
        
    }

    //getter for getting the top card in a stack
    public CardValues getTopCard() {
        if (cards.First != null) {
            return cards.First.Value;
        } else {
            return null;
        }
    }  

    //shuffle deck pershlaps
    //currently very unoptomized i think but might not matter since shuffling isn't something that happens during important gameplay
    // shuffles order of linked list, then calls reload to reset visual representation of cards.
    //On the network :3
    public void shuffle () {
        shuffleRpc();
        reload();
    }

    [Rpc(SendTo.Everyone)]
    public void shuffleRpc () {
        LinkedListNode<CardValues> firstCard;
        //Debug.Log("Shuffle Called");

        for (int i = 0; i < numCards; i++) {
            //get swap stuff
            firstCard = getIthNode(i);
            int swap = UnityEngine.Random.Range(i, numCards); //random card to swap with
            LinkedListNode<CardValues> toSwap = getIthNode(swap);
            
            //swap
            if (i + 1 == swap) {
                //cards.Remove(firstCard); 
                cards.Remove(toSwap);
                cards.AddBefore(firstCard, toSwap);  
                //cards.AddAfter(firstCard, firstCard);    
            } else if (i < swap) {
                LinkedListNode<CardValues> beforeSwap = toSwap.Previous;
                LinkedListNode<CardValues> afterFirst = firstCard.Next;

                cards.Remove(firstCard); 
                cards.Remove(toSwap);
                cards.AddBefore(afterFirst, toSwap);  
                cards.AddAfter(beforeSwap, firstCard);    
            }
        }

        reload();
    }

    //helper function, gets and returns i'th element in the linked list
    private LinkedListNode<CardValues> getIthNode (int n) {
        
        LinkedListNode<CardValues> currentNode = cards.First;
        for (int i = 0; i < n; i++) {
            currentNode = currentNode.Next;
        }
        return currentNode;
    }

    //helper function, resets all cards (removes them all, then spawns them all in in order)
    //SHOULD BE NETWORK PROOF
    private void reload() {

        CardValues[] destroyedCards = new CardValues[numCards];
        int originalNumCards = numCards;

        //destroy all card gameObjects
        for (int i = 0; i < originalNumCards; i++) {
            CardValues topCard = cards.First.Value;
            destroyedCards[i] = new CardValues(topCard.value, topCard.color, topCard.face, 0); //sets ulong to 0 bc null doesn't exist - hopefully this doesn't cause problems :)
            
            destroyCardRpc(topCard.inGameObjectID);
            /*NetworkObject realCard = GetNetworkObject(topCard.inGameObjectID);
            Destroy(realCard);*/
            //topCard.destroyObject();
            
            removeCardRpc(this.NetworkObjectId);
            //cards.RemoveFirst();
            numCards--;
        }

        //load all card gameObjects back into the scene
        for (int i = 0; i < originalNumCards; i++) {
            //sets sorting order networkvariable to ensure cards are displayed right
            spawnCardRpc(destroyedCards[i].value, destroyedCards[i].color, destroyedCards[i].face);
            numCards++;
           
        }
        
    }

    //getter for isDeck
    public bool checkIfDeck () {
        return isDeck;
    }

    //NOT ON NETWORK
    public void faceOtherWay () {

        //handles changing visuals so you can / can't see cards. 
        //eventually, flipped sprite should show face of cards flipped
        //not actually hard to do but im focusing on smth else today
        if (isFacingUp.Value) { 
            isFacingUp.Value = false;
            //canTransfer.Value = false; //facedown decks should not be able to transfer cards. 
            deckOrderRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, true);
        } else {
            isFacingUp.Value = true;
            //canTransfer.Value = true;
            deckOrderRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
        }

        LinkedListNode<CardValues> currentNode = cards.First;
        CardValues[] newOrdering = new CardValues[numCards];

        //creates new ordering
        for (int i = 0; i < numCards; i++) {
            newOrdering[i] = currentNode.Value;
            currentNode = currentNode.Next;
        }

        //reset cards
        cards = new LinkedList<CardValues>();
        for (int i = 0; i < numCards; i++) {
            cards.AddLast(newOrdering[i]);
        }

        //reload deck so order is right
        reload();

        Debug.Log("Reordered Cards! (deck is now facing the other way)");

    }

    //sets order of sprite of specific card 
    [Rpc(SendTo.Everyone)]
    void cardOrderRpc (ulong cardID, int multNum) {
        NetworkObject targetCard = GetNetworkObject(cardID);
        targetCard.GetComponent<SpriteRenderer>().sortingOrder = multNum*2 + 1;
        targetCard.GetComponentInChildren<Canvas>().sortingOrder = multNum*2 + 2;
    }

    //sets order of sprite and facedown sprite of flipped decks
    [Rpc(SendTo.Everyone)]
    void deckOrderRpc (ulong deckID, bool wasFacingUp) {
        NetworkObject targetDeck = GetNetworkObject(deckID);
        if (wasFacingUp) {
            targetDeck.GetComponent<SpriteRenderer>().sortingOrder = 100;
            targetDeck.GetComponent<SpriteRenderer>().sprite = faceDownSprite;    
        } else {
            targetDeck.GetComponent<SpriteRenderer>().sortingOrder = 0;
            targetDeck.GetComponent<SpriteRenderer>().sprite = faceUpSprite;
        }
    }

    //makes visual changes to card to reflect value, color, face, etc. 
    [Rpc(SendTo.Everyone)]
    void setCardValuesRpc (ulong cardID, int value, Color color, string face) {
        NetworkObject targetCard = GetNetworkObject(cardID);
        targetCard.GetComponent<CardScript>().setCard(value, color, face);
    }

    //adds card to linked list that stores all the cards. needed i think. helper for addCard
    [Rpc(SendTo.Everyone)]
    void addCardRpc (ulong stackID, int value, Color color, string face, ulong cardID) {
        NetworkObject targetStack = GetNetworkObject(stackID);
        CardValues cardToAdd = new CardValues(value, color, face, cardID);

        targetStack.GetComponent<StackScript>().cards.AddFirst(cardToAdd);
    }

    //spawns card in the game as a networkobject, since u can't do that as a client. helper for addCard
    [Rpc(SendTo.Server)]
    void spawnCardRpc (int value, Color color, string face) {
        GameObject newCard = Instantiate(cardPrefab, transform.position, transform.rotation, transform); //might want to instantiate in relation to stack, if we decide stacks can move around, rather than worldspace
        var newCardNetworkObject = newCard.GetComponent<NetworkObject>();
        newCardNetworkObject.Spawn(true);
        newCardNetworkObject.transform.parent = transform; //fixes the parent issue >?>?>?

        addCardRpc(this.NetworkObjectId, value, color, face, newCardNetworkObject.NetworkObjectId);
        setCardValuesRpc(newCardNetworkObject.NetworkObjectId, value, color, face);
        cardOrderRpc(newCardNetworkObject.NetworkObjectId, numCards);


        //return newCardNetworkObject.NetworkObjectId;
    }

    //in theory removes the card the same way that other rpc adds the card
    [Rpc(SendTo.Everyone)]
    void removeCardRpc (ulong stackID) {
        NetworkObject targetStack = GetNetworkObject(stackID);
        targetStack.GetComponent<StackScript>().cards.RemoveFirst();
    }

    //NetworkObjects can only be destroyed on the server, so this takes care of pesky cards we wanna get rid of!
    [Rpc(SendTo.Server)] 
    void destroyCardRpc (ulong cardID) {
        GameObject cardToDestroy = GetNetworkObject(cardID).gameObject;
        Destroy(cardToDestroy);
    }
    
}

//btw my comments are so professional and definitely would pass muster in a real work environment. 

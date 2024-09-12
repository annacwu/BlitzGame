using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class StackScript : MonoBehaviour
{   

    private bool selected = false; //determines whether the stack has been clicked once (e.g to move a card)
    private StackManagerScript smanager;
    public GameObject cardPrefab;
    [SerializeField] private bool isDeck; //decks start with a full deck of cards, which is shuffled automatically. Decks are spawned in when the game starts, & handle the whole doling out cards thing.
    [SerializeField] private bool isStackOfOne; //Stacks of one can only contain one card at a time (e.g. the set of 3 cards in front of the player)
    private bool canAcceptCards; //decks and the stack of 10 cards cannot accept any cards. This variable will be set when a stack is created. 
    //a comment 

    public class CardValues {
        public bool destroyed;
        public int value; //number on the card, 2-10
        public Color color; //color of the card, determining where it can be placed
        public string face; //the symbol on the back, determining who owns the card, and gets points from it
        GameObject inGameObject; //reference to the actual Card object in the scene, so we can do stuff to it later

        public CardValues (int v, Color c, string f, GameObject g) {
            value = v;
            color = c;
            face = f;
            inGameObject = g;
            destroyed = false;
        }

        public void destroyObject(){
            Destroy(inGameObject);
            Debug.Log("Object Destroyed");
        }
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
    public void selectThisStack () {
        if (selected) {
            //selected = false;
            selected = smanager.selectStack(gameObject);
            //Debug.Log("Stackscript says: " + selected);
        } else {
            //selected = true;
            selected = smanager.selectStack(gameObject);
            //Debug.Log("Stackscript says: " + selected);
        }
    }

    public void addBlankCard() {
        addCard(0, new Color(1, 1, 1, 1), null);
    }

    //add card
    public void addCard (int value, Color color, string face) {
        GameObject newCard = Instantiate(cardPrefab, transform.position, transform.rotation); //might want to instantiate in relation to stack, if we decide stacks can move around, rather than worldspace
        Vector3 cardPos = new Vector3(0, 0, -1);
        newCard.transform.position += cardPos; //i don't know if this is the best way to do this
        cards.AddFirst(new CardValues(value, color, face, newCard));
        newCard.GetComponent<CardScript>().setCard(cards.First.Value);
    }

    //take top card, put it somewhere else?
    public void removeTopCard () {
        //CardValues topCard = cards.First.Value;
        //Debug.Log(cards.First.Value);
        if (cards.First != null) {
            CardValues topCard = cards.First.Value;
            topCard.destroyObject();
            cards.RemoveFirst();
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
    public void shuffle () {

    }


}

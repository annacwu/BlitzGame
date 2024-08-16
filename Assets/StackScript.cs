using System.Collections;
using System.Collections.Generic;
// using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class StackScript : MonoBehaviour
{   

    private bool selected = false; //determines whether the stack has been clicked once (e.g to move a card)
    private StackManagerScript smanager;
    public GameObject cardPrefab;
    //a comment 

    class CardValues {
        bool destroyed;
        int value; //number on the card, 2-10
        string color; //color of the card, determining where it can be placed
        string face; //the symbol on the back, determining who owns the card, and gets points from it
        GameObject inGameObject; //reference to the actual Card object in the scene, so we can do stuff to it later

        public CardValues (int v, string c, string f, GameObject g) {
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
            Debug.Log("Stackscript says: " + selected);
        } else {
            //selected = true;
            selected = smanager.selectStack(gameObject);
            Debug.Log("Stackscript says: " + selected);
        }
    }

    public void addBlankCard() {
        addCard(0, null, null);
    }

    //add card
    public void addCard (int value, string color, string face) {
        GameObject newCard = Instantiate(cardPrefab, transform.position, transform.rotation); //might want to instantiate in relation to stack, if we decide stacks can move around, rather than worldspace
        Vector3 cardPos = new Vector3(0, 0, -1);
        newCard.transform.position += cardPos; //i don't know if this is the best way to do this
        cards.AddFirst(new CardValues(value, color, face, newCard));
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


    //shuffle deck pershlaps
    public void shuffle () {

    }


}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : NetworkBehaviour
{
    //love how cardscript has more commented out code than functional code. clearly my ideas are always good and never need fixing. 
    [SerializeField] private TMP_Text topValueText;
    [SerializeField] private TMP_Text bottomValueText;
    [SerializeField] private GameObject thisGameObject;
    private SpriteRenderer thisRenderer;

    public StackScript.CardValues thisCard;

    private int cardSortingOrder;
    
    
    // Start is called before the first frame update
    //set up sorting order when spawned on the network
    void Start()
    {   
        /*
        cardSortingOrder = gameObject.GetComponentInParent<StackScript>().returnSortingValue();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = cardSortingOrder;
        gameObject.GetComponentInChildren<Canvas>().sortingOrder = cardSortingOrder + 1; */
        //Debug.Log(cardSortingOrder);
    }
    
    /*
    // Update is called once per frame
    void Update()
    {
        
    }*/

    //sets visible color + value (we can modify this to do other things when we figure out how we're actually representing cards)
    /*public void setCard (StackScript.CardValues card) {
        thisRenderer = thisGameObject.GetComponent<SpriteRenderer>();
        thisCard = card;
        topValueText.text = thisCard.value.ToString();
        bottomValueText.text = thisCard.value.ToString();
        thisRenderer.color = thisCard.color;
    }*/
    
    public void setCard (int value, Color color, string face) {
        thisRenderer = thisGameObject.GetComponent<SpriteRenderer>();
        topValueText.text = value.ToString();
        bottomValueText.text = value.ToString();
        thisRenderer.color = color;
        //add visual change face

        //sets struct for future reference
        thisCard = new(value, color, face, this.NetworkObjectId);
    }

}

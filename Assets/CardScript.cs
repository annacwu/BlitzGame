using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    
    [SerializeField] private TMP_Text topValueText;
    [SerializeField] private TMP_Text bottomValueText;
    [SerializeField] private GameObject thisGameObject;
    private SpriteRenderer thisRenderer;

    public StackScript.CardValues thisCard;

    
    
    // Start is called before the first frame update
    //should set values of card correctly
    void Start()
    {
        
        //Debug.Log("hi");
    }

    /*
    // Update is called once per frame
    void Update()
    {
        
    }*/

    //sets visible color + value (we can modify this to do other things when we figure out how we're actually representing cards)
    public void setCard (StackScript.CardValues card) {
        thisRenderer = thisGameObject.GetComponent<SpriteRenderer>();
        thisCard = card;
        topValueText.text = thisCard.value.ToString();
        bottomValueText.text = thisCard.value.ToString();
        thisRenderer.color = thisCard.color;
    }

}

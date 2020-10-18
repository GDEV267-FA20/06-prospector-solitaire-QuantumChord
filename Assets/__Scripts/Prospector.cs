﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]

    public TextAsset deckXML;

    public TextAsset layoutXML;

    public float xOffset = 3;

    public float yOffSet = -2.5f;

    public Vector3 layoutCenter;

    [Header("Set Dynamically")]

    public Deck deck;

    public Layout layout;

    public List<CardProspector> drawPile;

    public Transform layoutAnchor;

    public CardProspector target;

    public List<CardProspector> tableau;

    public List<CardProspector> discardPile;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();

        deck.InitDeck(deckXML.text);

        Deck.Shuffle(ref deck.cards);

        //Card c;

        //for(int cNum=0; cNum<deck.cards.Count; cNum++)
        //{
        //c = deck.cards[cNum];

        //c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        layout = GetComponent<Layout>();

        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);

        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();

        CardProspector tCP;

        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardProspector;

            lCP.Add(tCP);
        }
        return (lCP);
    }

    //The Draw function will pull a single card from the drawPile and return it

    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];

        drawPile.RemoveAt(0);

        return (cd);
    }

    //LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"

    void LayoutGame()
    {
        //Create an empty GameObject to serve as an anchor for the tableau

        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");

            //^ Create an empty GameObject named _LayoutAnchor in the Hierarchy

            layoutAnchor = tGO.transform;

            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;

        //Follow the layout

        foreach(SlotDef tSD in layout.slotDefs)
        {
            //^ Iterate through all the SlotDefs in the layout.slotDefs as tSD

            cp = Draw();

            cp.faceUp = tSD.faceUp;

            cp.transform.parent = layoutAnchor;

            //This replaces the previous parent: deck.deckAnchor, which
            //appears as _Deck in the Hierarchy when the scene is playing.

            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);

            //^ Set the localPosition of the card based on slotDef

            cp.layoutID = tSD.id;

            cp.slotDef = tSD;

            //CardProspectors in the tableau have the state CardState.tableau

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        //Set up the initial target card

        MoveToTarget(Draw());

        //Set up the Draw pile

        UpdateDrawPile();
    }

    //Moves the current target to the discardPile

    void MoveToDiscard(CardProspector cd)
    {
        //Set the state of the card to discard

        cd.state = eCardState.discard;

        discardPile.Add(cd);

        cd.transform.parent = layoutAnchor;

        //Position this card on the discardPile

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);

        cd.faceUp = true;

        //Place it on top of the pile for depth sorting

        cd.SetSortingLayerName(layout.discardPile.layerName);

        cd.SetSortOrder(-100 + discardPile.Count);
    }

    //Make cd the new target card

    void MoveToTarget(CardProspector cd)
    {
        //If there is currently a target card, move it to discardPile

        if (target != null) MoveToDiscard(target);

        target = cd;

        cd.state = eCardState.target;

        cd.transform.parent = layoutAnchor;

        //Move to the target positon

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);

        cd.faceUp = true;

        //Set the depth sorting

        cd.SetSortingLayerName(layout.discardPile.layerName);

        cd.SetSortOrder(0);
    }

    //Arranges all the cards of the drawPile to show how many are left

    void UpdateDrawPile()
    {
        CardProspector cd;

        //Go through all the cards of the drawPile

        for(int i=0; i<drawPile.Count; i++)
        {
            cd = drawPile[i];

            cd.transform.parent = layoutAnchor;

            //Position it correctly with the layout.drawPile.stagger

            Vector2 dpStagger = layout.drawPile.stagger;

            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID+0.1f*i);

            cd.faceUp = false;

            cd.state = eCardState.drawpile;

            //Set depth sorting

            cd.SetSortingLayerName(layout.drawPile.layerName);

            cd.SetSortOrder(-10 * i);
        }
    }

    //CardClicked is called any time a card in the game is 

    public void CardClicked(CardProspector cd)
    {
        //The reaction is determined by the state of the clicked card

        switch (cd.state)
        {
            case eCardState.target:

                //Clicking the target card does nothing

                break;

            case eCardState.drawpile:

                //Clicking any card in the drawPile will draw the next card

                MoveToDiscard(target);

                MoveToTarget(Draw());

                UpdateDrawPile();

                break;

            case eCardState.tableau:

                //Clicking a card in the tableau will check if it's a valid play

                bool validMatch = true;

                if (!cd.faceUp)
                {
                    //If the card is face-down, it's not valid

                    validMatch = false;
                }

                if (!AdjacentRank(cd, target))
                {
                    //If it's not an adjacent rank, it's not valid

                    validMatch = false;
                }

                if (!validMatch) return;

                //If we got here, then: Yay! It's a valid card.

                tableau.Remove(cd);

                MoveToTarget(cd);

                break;
        }
    }

    //Return true if the two cards are adjacent in rank (A & K wrap around)

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        //If either card is face-down, it's not adjacent.

        if (!c0.faceUp || !c1.faceUp) return (false);

        //If they are 1 apart, they are adjacent

        if(Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }

        //If one is Ace and the other King, they are adjacent

        if (c0.rank == 11 && c1.rank == 13) return (true);

        if (c0.rank == 13 && c1.rank == 1) return (true);

        //otherwise, return false

        return (false);
    }

}

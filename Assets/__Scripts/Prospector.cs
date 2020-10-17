using System.Collections;
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

            cp.state = eCardState.tableau;

            tableau.Add(cp);
        }
    }
}

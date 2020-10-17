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

    [Header("Set Dynamically")]

    public Deck deck;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();

        deck.InitDeck(deckXML.text);

        Deck.Shuffle(ref deck.cards);

        Card c;

        for(int cNum=0; cNum<deck.cards.Count; cNum++)
        {
            c = deck.cards[cNum];

            c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]

    public string suit;

    public int rank;

    public Color color = Color.black;

    public string colS = "Black";

    //This List holds all of the Decorator GameObjects

    public List<GameObject> decoGOs = new List<GameObject>();

    //This List holds all of the Pip GameObjects

    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back;

    public CardDefinition def;

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }

        set
        {
            back.SetActive(!value);
        }
    }
}

[System.Serializable]

public class Decorator
{
    //This class stores information about deach decorator or pip from DeckXML
    public string type;

    public Vector3 loc;

    public bool flip = false;

    public float scale = 1f;
}

[System.Serializable]

public class CardDefinition
{
    //This class stores information for each rank of card
    public string face;

    public int rank;

    public List<Decorator> pips = new List<Decorator>();
}
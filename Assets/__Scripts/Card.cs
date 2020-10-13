using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    //Defined later
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
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

    //List of the SpriteRenderer Components of this GameObject and its children

    public SpriteRenderer[] spriteRenderers;

    void Start()
    {
        SetSortOrder(0);
    }

    //If spriteRenderers is not yet defined, this function defines it

    public void PopulateSpriteRenderers()
    {
        //If spriteRenders is null or empty

        if(spriteRenderers == null || spriteRenderers.Length == 0)
        {
            //Get SpriteRenderer Components of this GameObject and its children

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    //Sets the sortingLayerName on all SpriteRenderer Components

    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    //Sets the sortingOrder of all SpriteRenderer Components

    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        //Iterate through all the spriteRenderers as tSR

        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject == this.gameObject)
            {
                //If the gameObject is this.gameObject, it's the background

                tSR.sortingOrder = sOrd;

                continue;
            }

            //Each of the children of this GameObject are named
            //switch based on there name

        switch (tSR.gameObject.name)
            {
                case "back":

                    //Set it to the highest layer to cover the other sprites

                    tSR.sortingOrder = sOrd + 2;

                    break;

                case "face":
                default:

                    //Set it to the middle layer to be above the background

                    tSR.sortingOrder = sOrd + 1;

                    break;
            }
        }
    }

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
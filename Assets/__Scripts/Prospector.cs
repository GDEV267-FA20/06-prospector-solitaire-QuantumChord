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

    public TextAsset LayoutXMLNew;

    public float xOffset = 3;

    public float yOffSet = -2.5f;

    public Vector3 layoutCenter;

    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);

    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);

    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);

    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);

    public float reloadDelay = 2f;

    public Text gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]

    public Deck deck;

    public Layout layout;

    public List<CardProspector> drawPile;

    public Transform layoutAnchor;

    public CardProspector target;

    public List<CardProspector> tableau;

    public List<CardProspector> discardPile;

    public FloatingScore fsRun;

    void Awake()
    {
        S = this;

        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        //Set up the HighScore UI Text

        GameObject go = GameObject.Find("HighScore");

        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }

        int highScore = ScoreManager.HIGH_SCORE;

        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);

        go.GetComponent<Text>().text = hScore;

        //Set up the UI Texts that show at the end of the round

        go = GameObject.Find("GameOver");

        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");

        if (go != null)
        {

            roundResultText = go.GetComponent<Text>();

        }

        //Make the end of the round texts invisible

        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);

        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

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

        //layout.ReadLayout(layoutXML.text);

        layout.ReadLayout2(LayoutXMLNew.text);

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

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        //Set which cards are hiding each other

        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);

                tCP.hiddenBy.Add(cp);
            }
        }

        //Set up the initial target card

        MoveToTarget(Draw());

        //Set up the Draw pile

        UpdateDrawPile();
    }

    //Convert from the layoutID int to the CardProspector with that ID

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in tableau)
        {
            //Search through all cards in tableau List<>

            if(tCP.layoutID == layoutID)
            {
                //If the card has the same ID, return it

                return (tCP);
            }
        }

        //If it's not found, return null

        return (null);
    }

    //This turns cards in the Mine face-up or face-down

    void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool faceUp = true;

            foreach(CardProspector cover in cd.hiddenBy)
            {
                if (cover.state == eCardState.tableau)
                {
                    faceUp = false;
                }
            }

            cd.faceUp = faceUp;
        }
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

                ScoreManager.EVENT(eScoreEvent.draw);

                FloatingScoreHandler(eScoreEvent.draw);

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

                SetTableauFaces();

                ScoreManager.EVENT(eScoreEvent.mine);

                FloatingScoreHandler(eScoreEvent.mine);

                break;
        }

        //Checks to see whether the game is over or not

        CheckForGameOver();
    }

    //Test whether the game is over

    void CheckForGameOver()
    {
        //If the tableau is empty, the game is over

        if (tableau.Count == 0)
        {
            //Call GameOver() with a win

            GameOver(true);

            return;
        }

        //If there are still cards in the draw pile, the game's not over

        if (drawPile.Count > 0)
        {
            return;
        }

        //Checks for remaining valid plays

        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                //If there is a valid play, the game's not over

                return;
            }
        }

        //Since there are no valid plays, the game is over
        //Call gameOver with a loss

        GameOver(false);
    }

    //Called when the game is over. Simple for now, but expandable

    void GameOver(bool won)
    {

        int score = ScoreManager.SCORE;

        if (fsRun != null) score += fsRun.score;

        if (won)
        {
            gameOverText.text = "Round Over";

            roundResultText.text = "You won this round!\nRound Score: " + score;

            ShowResultsUI(true);

            //print("Game over. You won! :)");

            ScoreManager.EVENT(eScoreEvent.gameWin);

            FloatingScoreHandler(eScoreEvent.gameWin);
        }

        else
        {
            gameOverText.text = "Game Over";

            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;

                roundResultText.text = str;
            }

            else
            {
                roundResultText.text = "Your final score was: " + score;
            }

            ShowResultsUI(true);

            //print("Game Over. You Lost. :(");

            ScoreManager.EVENT(eScoreEvent.gameLoss);

            FloatingScoreHandler(eScoreEvent.gameLoss);
        }

        //Reload the scene, resetting the game

        //SceneManager.LoadScene("__Prospector_Scene_0");

        //Reload the scene in reloadDelay seconds
        //This will give the score a moment to travel

        Invoke("ReloadLevel", reloadDelay);
    }


    void ReloadLevel()
    {
        //reload the scene, resetting the game

        SceneManager.LoadScene("__Prospector_Scene_0");
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

        if (c0.rank == 1 && c1.rank == 13) return (true);

        if (c0.rank == 13 && c1.rank == 1) return (true);

        //otherwise, return false

        return (false);
    }

    //Handle FloatingScore movement

    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;

        switch (evt)
        {
            //Same things need to happen whether it's a draw, a win, or a loss

            case eScoreEvent.draw:

            case eScoreEvent.gameWin:

            case eScoreEvent.gameLoss:
                
                //Add fsRun to the Scoreboard score

                if (fsRun != null)
                {
                    // Create points for the Bézier curve

                    fsPts = new List<Vector2>();

                    fsPts.Add(fsPosRun);

                    fsPts.Add(fsPosMid2);

                    fsPts.Add(fsPosEnd);

                    fsRun.reportFinishTo = Scoreboard.S.gameObject;

                    fsRun.Init(fsPts, 0, 1);

                    //Also adjust the font size

                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });

                    fsRun = null;
                }

                break;

            case eScoreEvent.mine:

                //Create a FloatingScore for this score

                FloatingScore fs;

                //Move it from the mousePosition to fsPosRun

                Vector2 p0 = Input.mousePosition;

                p0.x /= Screen.width;

                p0.y /= Screen.height;

                fsPts = new List<Vector2>();

                fsPts.Add(p0);

                fsPts.Add(fsPosMid);

                fsPts.Add(fsPosRun);

                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);

                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });

                if (fsRun == null)
                {
                    fsRun = fs;

                    fsRun.reportFinishTo = null;
                }

                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }

                break;
        }
    }
}

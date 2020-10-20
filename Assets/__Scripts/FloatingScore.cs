using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//An enum to track the possible states of a FloatingScorew

public enum eFSState
{
    idle,

    pre,

    active,

    post
}

//FloatingScore can move itself on screen following a Bézier curve

public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]

    public eFSState state = eFSState.idle;

    [SerializeField]

    protected int _score = 0;

    public string scoreString;

    //The score property sets both _score and scoreString

    public int score
    {
        get
        {
            return (_score);
        }

        set
        {
            _score = value;

            scoreString = _score.ToString("N0");

            //Search "C# Standard Numeric Format Strings" for ToString formats

            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;

    public List<float> fontSizes;

    public float timeStart = -1f;

    public float timeDuration = 1f;

    public string easingCurve = Easing.InOut;

    //The GameObject that will receive the SendMessage when this is done moving

    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;

    private Text txt;

    //Set up the FloatingScore and movement
    //Note the use of parameter defaults for eTimeS & eTimeD

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();

        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1) //If there is only one point
        {
            //... then just go there.

            transform.position = ePts[0];

            return;
        }

        //If eTimeS is the default, just start at the current time

        if (eTimeS == 0) eTimeS = Time.time;

        timeStart = eTimeS;

        timeDuration = eTimeD;

        state = eFSState.pre;
    }

    public void FSCallback(FloatingScore fs)
    {
        //When this callback is called by SendMessage,
        //add the score from the calling FloatScore

        score += fs.score;
    }

    //Update is called once per frame

    void Update()
    {
        //If this is not moving, just return

        if (state == eFSState.idle) return;

        //Get u from the current time and duration
        //u ranges from 0 to 1 (usually)

        float u = (Time.time - timeStart) / timeDuration;

        //Use Easing class from utils to curve the u value

        float uC = Easing.Ease(u, easingCurve);

        if (u < 0)
        {
            state = eFSState.pre;

            txt.enabled = false;
        }

        else
        {
            if (u >= 1)
            {
                uC = 1;

                state = eFSState.post;

                if(reportFinishTo != null)// If there's a callback GameObject
                {
                    //Use SendMessage to call the FSCallback method
                    //with this as the parameter.

                    reportFinishTo.SendMessage("FSCallback", this);

                    //Now that the message has been sent,
                    //Destroy this gameObject

                    Destroy(gameObject);
                }

                else //If there is nothing to call back
                {
                    //... then don't destroy this. Just let it stay

                    state = eFSState.idle;
                }
            }

            else
            {
                //0<=u<1, which means that this is active and moving

                state = eFSState.active;

                txt.enabled = true;
            }

            //Use Bézier curve to move this to the right point

            Vector2 pos = Utils.Bezier(uC, bezierPts);

            //RectTransform anchors can be used to position UI objects relative
            //to total size of the scene

            rectTrans.anchorMin = rectTrans.anchorMax = pos;

            if (fontSizes !=null && fontSizes.Count > 0)
            {
                //If fontSizes has values in it
                //... then adjust the fontSize of this GUIText

                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));

                GetComponent<Text>().fontSize = size;
            }
        }
    }
}

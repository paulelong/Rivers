using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WordSpell;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public LetterProp lp;

    const float FALL_RATE = 1.5f;
    const float SPIN_RATE = 45f;

    // Use this for initialization
    void Start ()
    {
    }

    // Update is called once per frame
    void Update ()
    {
        // Tiles that fall off the screen are destroyed.
        if(gameObject.transform.position.y < -150)
        //if (gameObject.transform.GetChild(0).transform.position.y < -150)
        {
                Destroy(gameObject);
        }

        if(lp != null)
        {
            if (lp.LetterDCount > 0.01f)
            {
                lp.LetterDCount -= FALL_RATE * Time.deltaTime;
                gameObject.transform.position -= new Vector3(0, FALL_RATE * Time.deltaTime * WSGameState.GridScale, 0);
            }
            if (lp.LetterUCount > 0.01f)
            {
                lp.LetterUCount -= FALL_RATE * Time.deltaTime;
                gameObject.transform.position += new Vector3(0, FALL_RATE * Time.deltaTime * WSGameState.GridScale, 0);
            }
            if (lp.LetterLCount > 0.01f)
            {
                lp.LetterLCount -= FALL_RATE * Time.deltaTime;
                gameObject.transform.position -= new Vector3(FALL_RATE * Time.deltaTime * WSGameState.GridScale, 0, 0);
            }
            if (lp.LetterRCount > 0.01f)
            {
                lp.LetterRCount -= FALL_RATE * Time.deltaTime;
                gameObject.transform.position += new Vector3(FALL_RATE * Time.deltaTime * WSGameState.GridScale, 0, 0);
            }
            if (lp.LetterRotHL > 0.01f)
            {
                lp.LetterRotHL -= SPIN_RATE * Time.deltaTime;
                gameObject.transform.RotateAround(lp.LetterRotHLAxis, Vector3.up, SPIN_RATE * Time.deltaTime);
                gameObject.transform.Rotate(Vector3.up, SPIN_RATE * Time.deltaTime);
            }
            if (lp.LetterRotHR > 0.01f)
            {
                lp.LetterRotHR -= SPIN_RATE * Time.deltaTime;
                gameObject.transform.RotateAround(lp.LetterRotHRAxis, Vector3.up, -SPIN_RATE * Time.deltaTime);
                gameObject.transform.Rotate(Vector3.up, -SPIN_RATE * Time.deltaTime);
            }
            if (lp.LetterRotVU > 0.01f)
            {
                lp.LetterRotVU -= SPIN_RATE * Time.deltaTime;
                gameObject.transform.RotateAround(lp.LetterRotVUAxis, Vector3.right, SPIN_RATE * Time.deltaTime);
                gameObject.transform.Rotate(Vector3.right, SPIN_RATE * Time.deltaTime);
            }
            if (lp.LetterRotVD > 0.01f)
            {
                lp.LetterRotVD -= SPIN_RATE * Time.deltaTime;
                gameObject.transform.RotateAround(lp.LetterRotVDAxis, Vector3.right, -SPIN_RATE * Time.deltaTime);
                gameObject.transform.Rotate(Vector3.right, -SPIN_RATE * Time.deltaTime);
            }
        }
    }

    public void AttachLetterProp(LetterProp _lp)
    {
        lp = _lp;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        lp.PlaySelect();
        WSGameState.LetterClick(lp.I, lp.J);
    }

    // Called by animatitions so that spells can be completed, like turning tiles that have been flipped.
    public void FinishSpell()
    {
        Spells.CastSpell();
    }

}

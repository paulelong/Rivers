using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WordSpell;

public class Tile : MonoBehaviour, IPointerClickHandler {

    int i, j;
    LetterProp lp;
    float fallrate = 0.1f;

    public int I
    {
        get
        {
            return i;
        }

        set
        {
            i = value;
        }
    }

    public int J
    {
        get
        {
            return j;
        }

        set
        {
            j = value;
        }
    }

    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update () {
        if(gameObject.transform.position.y < -10)
        {
            Destroy(gameObject);
        }

        if(lp != null)
        {
            if (lp.LetterFallCount > 0.01f)
            {
                lp.LetterFallCount -= fallrate;
                gameObject.transform.position -= new Vector3(0, fallrate, 0);
            }
        }
    }

    public void SetPos(int _i, int _j, LetterProp  _lp)
    {
        I = _i;
        J = _j;
        lp = _lp;
    }

    public void SetPos(int _i, int _j)
    {
        I = _i;
        J = _j;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        WSGameState.LetterClick(I, J);
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }

    public void FinishSpell()
    {
        Spells.FinishSpellAnim();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WordSpell;

public class Tile : MonoBehaviour, IPointerClickHandler
{

    //static AudioClip[] AmbientSongs;

    System.Random r = new System.Random();

    //int i, j;
    public LetterProp lp;
    float fallrate = 0.1f;
    float spinrate = 5f;

    //public int I
    //{
    //    get
    //    {
    //        return i;
    //    }

    //    set
    //    {
    //        i = value;
    //    }
    //}

    //public int J
    //{
    //    get
    //    {
    //        return j;
    //    }

    //    set
    //    {
    //        j = value;
    //    }
    //}

    //public static void LoadMusic()
    //{
    //    AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
    //}

    // Use this for initialization
    void Start ()
    {
        //if(lp.TileType == LetterProp.TileTypes.Speaker)
        //{
        //    LoadMusic();
        //}
    }

    // Update is called once per frame
    void Update () {
        if(gameObject.transform.GetChild(0).transform.position.y < -150)
        {
            Destroy(gameObject);
        }

        if(lp != null)
        {
            if (lp.LetterDCount > 0.01f)
            {
                lp.LetterDCount -= fallrate;
                gameObject.transform.position -= new Vector3(0, fallrate, 0);
            }
            if (lp.LetterUCount > 0.01f)
            {
                lp.LetterUCount -= fallrate;
                gameObject.transform.position += new Vector3(0, fallrate, 0);
            }
            if (lp.LetterLCount > 0.01f)
            {
                lp.LetterLCount -= fallrate;
                gameObject.transform.position -= new Vector3(fallrate, 0, 0);
            }
            if (lp.LetterRCount > 0.01f)
            {
                lp.LetterRCount -= fallrate;
                gameObject.transform.position += new Vector3(fallrate, 0, 0);
            }
            if(lp.LetterRotHL > 0.01f)
            {
                lp.LetterRotHL -= spinrate;
                //gameObject.transform.Rotate(axis, Time.deltaTime);
                gameObject.transform.RotateAround(lp.LetterRotHLAxis, Vector3.up, spinrate);
                gameObject.transform.Rotate(Vector3.up, spinrate);
            }
            if (lp.LetterRotHR > 0.01f)
            {
                lp.LetterRotHR -= spinrate;
                //gameObject.transform.Rotate(axis, -spinrate);
                gameObject.transform.RotateAround(lp.LetterRotHRAxis, Vector3.up, -spinrate);
                gameObject.transform.Rotate(Vector3.up, -spinrate);
            }
            if (lp.LetterRotVU > 0.01f)
            {
                lp.LetterRotVU -= spinrate;
                //gameObject.transform.Rotate(axis, Time.deltaTime);
                gameObject.transform.RotateAround(lp.LetterRotVUAxis, Vector3.right, spinrate);
                gameObject.transform.Rotate(Vector3.right, spinrate);
            }
            if (lp.LetterRotVD > 0.01f)
            {
                lp.LetterRotVD -= spinrate;
                //gameObject.transform.Rotate(axis, -spinrate);
                gameObject.transform.RotateAround(lp.LetterRotVDAxis, Vector3.right, -spinrate);
                gameObject.transform.Rotate(Vector3.right, -spinrate);
            }
        }
    }

    public void AttachLetterProp(LetterProp _lp)
    {
        lp = _lp;
    }

    //public void SetPos(int _i, int _j, LetterProp  _lp)
    //{
    //    I = _i;
    //    J = _j;
    //    lp = _lp;
    //}

    //public void SetPos(int _i, int _j)
    //{
    //    I = _i;
    //    J = _j;
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        WSGameState.LetterClick(lp.I, lp.J);
        AudioSource audio = GetComponent<AudioSource>();
        lp.PlaySelect();
        //Debug.Log(lp.ASCIIString + " " + I.ToString() + "-" + J.ToString() + " " + lp.I + "-" + lp.J+  " " + lp.Tf.position);
    }

    public void FinishSpell()
    {
        Spells.CastSpell();
    }

    //public void NewSong()
    //{
    //    if(AmbientSongs != null)
    //    {
    //        int rs = r.Next(AmbientSongs.Length);
    //        AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
    //        asrc.clip = AmbientSongs[rs];
    //        asrc.PlayDelayed(4);
    //    }
    //}

    //public void StopSong()
    //{
    //    AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
    //    asrc.Stop();
    //}

}

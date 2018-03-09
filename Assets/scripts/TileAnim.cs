using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordSpell;

public class TileAnim : MonoBehaviour
{
    public AudioClip SelectSound;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NewSong()
    {
        AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
        if(asrc != null)
        {
            asrc.clip = Songs.GetNextSong();
            if(asrc.clip != null)
            {
                asrc.PlayDelayed(5);
                Logging.PlayDbg("ns_" + asrc.clip.ToString().Substring(0, 20), '\n');
            }
            else
            {
                Logging.PlayDbg("ns!no_songs");
            }
        }
        else
        {
            Logging.PlayDbg("ns!");
        }
    }

    public void StopSong()
    {
        AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
        asrc.Stop();
    }

    public void PlaySelect()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(SelectSound);
    }

    // Called by animatitions so that spells can be completed, like turning tiles that have been flipped.
    public void FinishSpell()
    {
        Spells.CastSpell2();
    }

}

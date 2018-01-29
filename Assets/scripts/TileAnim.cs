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

    public void FinishSpell()
    {
        Spells.CastSpell();
    }

    public void NewSong()
    {
        AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
        if(asrc != null)
        {
            asrc.clip = Songs.GetNextSong();
            asrc.PlayDelayed(5);
            WSGameState.boardScript.PlayDbg("ns_" + asrc.clip.ToString().Substring(0, 20), '\n');
        }
        else
        {
            WSGameState.boardScript.PlayDbg("ns!");
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

}

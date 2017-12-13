using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordSpell;

public class TileAnim : MonoBehaviour
{
    static AudioClip[] AmbientSongs;
    private System.Random r = new System.Random();
    public AudioClip SelectSound;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void LoadMusic()
    {
        AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
    }

    public void FinishSpell()
    {
        Spells.CastSpell();
    }

    public void NewSong()
    {
        if (AmbientSongs != null)
        {
            int rs = r.Next(AmbientSongs.Length);
            AudioSource asrc = (AudioSource)gameObject.GetComponent(typeof(AudioSource));
            asrc.clip = AmbientSongs[rs];
            asrc.PlayDelayed(4);
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

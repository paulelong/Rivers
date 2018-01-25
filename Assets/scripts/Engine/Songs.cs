using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WordSpell;

namespace WordSpell
{
    static public class Songs
    {
        static AudioClip[] AmbientSongs = null;
        static public AudioClip SelectSound;

        public static void LoadMusic()
        {
            AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
            if(AmbientSongs != null)
            {
                WSGameState.boardScript.PlayDbg("lm0_" + AmbientSongs.Length);
            }
            else
            {
                WSGameState.boardScript.PlayDbg("lm!" + AmbientSongs.Length);
            }

            WSGameState.boardScript.PlayDbg("lmx");
        }

        public static AudioClip GetNextSong()
        {
            if (AmbientSongs != null)
            {
                WSGameState.boardScript.PlayDbg("gns0_"+AmbientSongs.Length);
                if (AmbientSongs.Length > 0)
                {
                    int rs = WSGameState.Rnd.Next(AmbientSongs.Length - 1);
                    return AmbientSongs[rs];
                }
                else
                {
                    WSGameState.boardScript.PlayDbg("gns1");
                    return null;
                }
            }
            else
            {
                WSGameState.boardScript.PlayDbg("gns!");
                return null;
            }
        }
    }
}

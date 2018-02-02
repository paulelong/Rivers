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
                WSGameState.boardScript.StartDbg("lm0_" + AmbientSongs.Length);
            }
            else
            {
                WSGameState.boardScript.StartDbg("lm!");
            }

            WSGameState.boardScript.StartDbg("lmx");
        }

        public static AudioClip GetNextSong()
        {
            if (AmbientSongs != null)
            {
                WSGameState.boardScript.PlayDbg("gns0_"+AmbientSongs.Length);
                if (AmbientSongs.Length > 0)
                {
                    int rs = WSGameState.Rnd.Next(AmbientSongs.Length - 1);
                    WSGameState.boardScript.PlayDbg("gns1.1_" + rs);
                    return AmbientSongs[rs];
                }
                else
                {
                    WSGameState.boardScript.PlayDbg("gns1.2");
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

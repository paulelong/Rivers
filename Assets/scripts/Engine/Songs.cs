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
            Logging.StartDbg("lm0", timestamp: true);
            AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
            if(AmbientSongs != null)
            {
                Logging.StartDbg("lm1_" + AmbientSongs.Length);
            }
            else
            {
                Logging.StartDbg("lm!");
            }

            Logging.StartDbg("lmx", timestamp: true);
        }

        public static AudioClip GetNextSong()
        {
            if (AmbientSongs != null)
            {
                Logging.PlayDbg("gns0_"+AmbientSongs.Length);
                if (AmbientSongs.Length > 0)
                {
                    int rs = WSGameState.Rnd.Next(AmbientSongs.Length - 1);
                    Logging.PlayDbg("gns1.1_" + rs);
                    return AmbientSongs[rs];
                }
                else
                {
                    Logging.PlayDbg("gns1.2");
                    return null;
                }
            }
            else
            {
                Logging.PlayDbg("gns!");
                return null;
            }
        }
    }
}

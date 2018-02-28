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
        static List<AudioClip> BGMusic = new List<AudioClip>();

        static public AudioClip SelectSound;
        public static AudioClip x = null;

        public static string[] SongNames =
        {
            "MaHype2",
            "Prey4#3",
            "LetsGo49",
            "Acoustica_EQ",
            "idea5-Master",
            "idea6-Master",
            "iSong-Master",
            "KingArthorsPlight",
            "Stop Falling",
            "Theme1_MA",
            "WonderingAcc",
        };

//        public IEN

        public static void AddSong(AudioClip a)
        {
            BGMusic.Add(a);
        }

        public static int Count
        {
            get
            {
                return BGMusic.Count;
            }
        }

        public static void LoadMusic()
        {
            Logging.StartDbg("lm0", timestamp: true);

            AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
            if(AmbientSongs != null)
            {
                Logging.StartDbg("lm2_" + AmbientSongs.Length);
            }
            else
            {
                Logging.StartDbg("lm!");
            }

            Logging.StartDbg("lmx", timestamp: true);
        }

        public static AudioClip GetNextSong()
        {
            if(BGMusic.Count <= 0)
            {
                Logging.PlayDbg("gns!");
                return null;
            }
            else
            { 
                int rs = WSGameState.Rnd.Next(BGMusic.Count - 1);
                Logging.PlayDbg("gns1.1_" + rs);
                return BGMusic[rs];
            }

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

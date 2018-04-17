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
            "Acoustica_EQ",
            "idea5-Master",
            "idea6-Master",
            "iSong-Master",
            "KingArthorsPlight",
            "MaHype2",
            "Prey4#3",
            "LetsGo49",
            "Stop Falling",
            "Theme1_MA",
            "WonderingAcc",
        };
        private static int lastSong;

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
            if(Count <= 0)
            {
                Logging.PlayDbg("gns!");
                return null;
            }
            else
            {
                int rs;

                do
                {
                    rs = WSGameState.Rnd.Next(Count - 1);
                } while (lastSong == rs && Count >= 2);
                
                lastSong = rs;

                Logging.PlayDbg("gns1.1_" + rs);
                return BGMusic[rs];
            }
        }
    }
}

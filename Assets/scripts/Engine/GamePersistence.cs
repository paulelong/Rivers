using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using WordSpell;
using UnityEngine;

namespace WordSpell
{
    public class WordScoreItem
    {
        public string Word { get; set; }
        public string Wordscorestring { get; set; }
        public int Score { get; set; }
        public int Simplescore { get; set; }
    }

    public class GameStats
    {
        public int score = 0;
        public int level = 1;
        public int mana = 0;
        public List<WordScoreItem> history = new List<WordScoreItem>();
        public List<WordScoreItem> fortune = new List<WordScoreItem>();
        public List<SpellInfo> awarded = new List<SpellInfo>();
    }

    public class OverallStats
    {
        public List<WordScoreItem> BestWordScores = new List<WordScoreItem>();
        public List<WordScoreItem> BestWordScoresSimple = new List<WordScoreItem>();
        public List<WordScoreItem> LongestWords = new List<WordScoreItem>();
        public List<int> BestGameScores = new List<int>();
    }

    public class GamePersistence
    {
        private const string SaveGamePath = "WordSpellSave.xml";
        private const string OverallStatsPath = "WordSpellStats.xml";

        // [XmlRootAttribute("Letter")]
        public class SimpleLetter
        {
            public void Addletter(int _i, int _j, byte _letter, LetterProp.TileTypes _tt)
            {
                i = _i;
                j = _j;
                letter = _letter;
                tt = _tt;
            }

            public int i;
            public int j;
            public byte letter;
            public LetterProp.TileTypes tt;
        }

        //[XmlRootAttribute("StringList")]
        public class GameData
        {
            public List<SimpleLetter> grid = new List<SimpleLetter>();
            public GameStats gs = new GameStats();

            public void FillGameData(LetterProp[,] LetterPropGrid, GameStats _gs)
            {
                if (LetterPropGrid != null)
                {
                    gs = _gs;

                    for (int i = 0; i < WSGameState.gridsize; i++)
                    {
                        for (int j = 0; j < WSGameState.gridsize; j++)
                        {
                            SimpleLetter sl = new SimpleLetter();
                            sl.Addletter(i, j, LetterPropGrid[i, j].letter, LetterPropGrid[i, j].TileType);
                            grid.Add(sl);
                        }
                    }
                }
                else
                {
                    Debug.Log("Grid not intialized yet");
                }
            }

            internal GameStats ReplaceGameData(LetterProp[,] LetterPropGrid)
            {

                foreach (SimpleLetter sl in grid)
                {
                    LetterPropGrid[sl.i, sl.j].letter = sl.letter;
                    LetterPropGrid[sl.i, sl.j].TileType = sl.tt;
                    LetterPropGrid[sl.i, sl.j].UpdateLetterDisplay();
                }

                return (gs);
            }
        }

        internal static void SaveGame(LetterProp[,] LetterPropGrid, GameStats gs)
        {
            GameData gd = new GameData();
            gd.FillGameData(LetterPropGrid, gs);

            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(GameData));

                using(FileStream tw = new FileStream(filePath, FileMode.Create))
                {
                    xs.Serialize(tw, gd);
                }
            }
            catch(Exception e)
            {
                string s = e.Message;
            }
        }

        internal static GameStats LoadGame(LetterProp[,] LetterPropGrid)
        {
            GameData gd = null;
            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            if(File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(GameData));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        gd = (GameData)xs.Deserialize(fs);
                    }
  
                    return (gd.ReplaceGameData(LetterPropGrid));
                }
                catch (Exception e)
                {
                    string s = e.Message;
                }
            }


            return null;
        }

        internal static bool SavedGameExists()
        {
            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(GameData));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        xs.Deserialize(fs);
                    }
                    return true;
                }
                catch
                {
                    // We'll assume that the game save file is not there.
                    return false;
                }
            }

            return false;
        }

        internal static void SaveOverallStats(OverallStats os)
        {
            string filePath = Application.persistentDataPath + "/" + OverallStatsPath;

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(OverallStats));
                using (FileStream tw = new FileStream(filePath, FileMode.Create))
                {
                    xs.Serialize(tw, os);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal static OverallStats LoadOverallStats()
        {
            string filePath = Application.persistentDataPath + "/" + OverallStatsPath;

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(OverallStats));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        OverallStats os = (OverallStats)xs.Deserialize(fs);
                        return (os);
                    }
                }
                catch (InvalidOperationException)
                {
                    WSGameState.boardScript.MyDebug("los!");
                    return null;
                }
            }

            return null;
        }

        internal static void ResetGameData()
        {
            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch(Exception e)
                {
                    string s = e.Message;
                }
            }
        }

        internal static string TestPersistence()
        {
            string filePath = Application.persistentDataPath + "/testme.txt";
            string worked = "not yet";
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                worked = "It's opened";
            }

            return filePath + "\n" + worked;
        }

        internal static void ResetSavedData()
        {
            File.Delete(Application.persistentDataPath + "/" + SaveGamePath);

            File.Delete(Application.persistentDataPath + "/" + OverallStatsPath);
        }
    }
}

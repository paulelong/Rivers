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

    public class BestGameScore
    {
        public int score { get; set; }
        public int totalWords { get; set; }
        public int level { get; set; }
    }

    public class GameStats
    {
        public int score = 0;
        public int level = 1;
        public int mana = 0;
        public List<WordScoreItem> history = new List<WordScoreItem>();
        public List<WordScoreItem> fortune = new List<WordScoreItem>();
        public List<SpellInfo> awarded = new List<SpellInfo>();
        public int boardsize = 0;
    }

    public class OverallStats
    {
        public List<WordScoreItem> BestWordScores = new List<WordScoreItem>();
        public List<WordScoreItem> BestWordScoresSimple = new List<WordScoreItem>();
        public List<WordScoreItem> LongestWords = new List<WordScoreItem>();
        public List<BestGameScore> BestGameScores = new List<BestGameScore>();
    }

    public class GameData
    {
        public List<SimpleLetter> grid = new List<SimpleLetter>();
        public GameStats gs = new GameStats();
    }

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

    static public class GamePersistence
    {
        private const string SaveGamePath = "WordSpellSave.xml";
        private const string OverallStatsPath = "WordSpellStats.xml";

        static private GameData gd = new GameData();
        static private OverallStats os = new OverallStats();
        static private bool loaded = false;

        static public GameStats gs
        {
            get
            {
                return gd.gs;
            }
        }

        static public OverallStats Os
        {
            get
            {
                return os;
            }
        }

        static public void LoadSavedGameData()
        {
            LoadOverallStats();
            LoadGame();
        }

        static public string StatsText
        {
            get
            {
                string filePath = Application.persistentDataPath + "/" + OverallStatsPath;

                if (File.Exists(filePath))
                {
                    try
                    {

                        XmlSerializer xs = new XmlSerializer(typeof(GameData));
                        using (FileStream fs = new FileStream(filePath, FileMode.Open))
                        {
                            using (StreamReader reader = new StreamReader(fs))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WSGameState.boardScript.PlayDbg("gameFile!");
                        string s = e.Message;
                        return s;
                    }
                }
                else
                {
                    return "Couldn't open " + filePath;
                }
            }
        }

        static public string GameText
        {
            get
            {
                string filePath = Application.persistentDataPath + "/" + SaveGamePath;

                if(File.Exists(filePath))
                {
                    try
                    {

                        XmlSerializer xs = new XmlSerializer(typeof(GameData));
                        using (FileStream fs = new FileStream(filePath, FileMode.Open))
                        {
                            using (StreamReader reader = new StreamReader(fs))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WSGameState.boardScript.PlayDbg("gameFile!");
                        string s = e.Message;
                        return s;
                    }
                }
                else
                {
                    return "Couldn't open " + filePath;
                }
            }
        }

        // [XmlRootAttribute("Letter")]
        static public void SaveGameData(LetterProp[,] LetterPropGrid, GameStats _gs)
        {
            if (LetterPropGrid != null)
            {
                gd.gs = _gs;
                gd.grid.Clear();

                for (int i = 0; i < WSGameState.Gridsize; i++)
                {
                    for (int j = 0; j < WSGameState.Gridsize; j++)
                    {
                        SimpleLetter sl = new SimpleLetter();
                        sl.Addletter(i, j, LetterPropGrid[i, j].letter, LetterPropGrid[i, j].TileType);
                        gd.grid.Add(sl);
                    }
                }
            }
            else
            {
                Debug.Log("Grid not intialized yet");
            }
        }

        static internal void RestoreGameData(LetterProp[,] LetterPropGrid)
        {

            foreach (SimpleLetter sl in gd.grid)
            {
                LetterPropGrid[sl.i, sl.j].letter = sl.letter;
                LetterPropGrid[sl.i, sl.j].TileType = sl.tt;
                LetterPropGrid[sl.i, sl.j].UpdateLetterDisplay();
            }
        }

        static  int CalculateGridSize()
        {
            int largest = 0;

            foreach (SimpleLetter sl in gd.grid)
            {
                if(sl.i > largest)
                {
                    largest = sl.i;
                }
            }

            return largest + 1;
        }

        internal static void SaveGame(LetterProp[,] LetterPropGrid, GameStats gs)
        {
            //File.Delete(Application.persistentDataPath + "/" + SaveGamePath);

            SaveGameData(LetterPropGrid, gs);

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

        internal static void LoadGame()
        {
            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(GameData));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        gd = (GameData)xs.Deserialize(fs);
                        gd.gs.boardsize = CalculateGridSize();
                        loaded = true;
                    }
                }
                catch (Exception e)
                {
                    WSGameState.boardScript.PlayDbg(e.ToString());
                }
            }
        }

        internal static bool SavedGameExists()
        {
            if(loaded && gd.gs.boardsize > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void SaveOverallStats(OverallStats _os)
        {
            os = _os;

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

        internal static void LoadOverallStats()
        {
            string filePath = Application.persistentDataPath + "/" + OverallStatsPath;

            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(OverallStats));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        os = (OverallStats)xs.Deserialize(fs);
                    }
                }
                catch (InvalidOperationException)
                {
                    WSGameState.boardScript.StartDbg("los!");
                }
            }
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

        internal static void ResetSavedData()
        {
            File.Delete(Application.persistentDataPath + "/" + SaveGamePath);

            File.Delete(Application.persistentDataPath + "/" + OverallStatsPath);
        }
    }
}

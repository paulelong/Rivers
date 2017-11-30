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
    public class GamePersistence
    {
        private const string SaveGamePath = "WordSpellSave.xml";

       // [XmlRootAttribute("Letter")]
        public class SimpleLetter
        {
            public void addletter(int _i, int _j, char _letter, LetterProp.TileTypes _tt)
            {
                i = _i;
                j = _j;
                letter = _letter;
                tt = _tt;
            }

            public int i;
            public int j;
            public char letter;
            public LetterProp.TileTypes tt;
        }

        //[XmlRootAttribute("StringList")]
        public class GameData
        {
            public List<SimpleLetter> grid = new List<SimpleLetter>();

            public void FillGameData(LetterProp[,] LetterPropGrid)
            {
                for (int i = 0; i < WSGameState.gridsize; i++)
                {
                    for (int j = 0; j < WSGameState.gridsize; j++)
                    {
                        SimpleLetter sl = new SimpleLetter();
                        sl.addletter(i, j, LetterPropGrid[i, j].ASCIIChar, LetterPropGrid[i, j].TileType);
                        grid.Add(sl);
                    }
                }
            }
        }

        class BestScoreData
        {
        }

        internal static void SaveGame(LetterProp[,] LetterPropGrid)
        {
            GameData gd = new GameData();
            gd.FillGameData(LetterPropGrid);

            string filePath = Application.persistentDataPath + "/" + SaveGamePath;

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(GameData));
                TextWriter tw = new StreamWriter(filePath);
                xs.Serialize(tw, gd);
                tw.Close();
            }
            catch(Exception e)
            {
                string s = e.Message;
            }
        }

        //internal async static Task SaveGame()
        //{
        //    if(LetterPropGrid != null)
        //    {
        //        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //        // Create a simple setting
        //        for (int i = 0; i < gridsize; i++)
        //        {
        //            for (int j = 0; j < gridsize; j++)
        //            {
        //                string key = i.ToString() + "_" + j.ToString();
        //                localSettings.Values[key + "_letter"] = LetterPropGrid[i, j].letter;
        //                localSettings.Values[key + "_type"] = (int)LetterPropGrid[i, j].TileType;
        //            }
        //        }

        //        localSettings.Values["level"] = level;
        //        localSettings.Values["manna"] = Manna;
        //        localSettings.Values["score"] = totalScore;
        //        localSettings.Values["totalwords"] = totalwords;
        //        localSettings.Values["best"] = HighScoreWordTally;
        //        localSettings.Values["eff"] = Efficiency;

        //        await SaveList<List<WordScoreItem>>(BestWordScores, BestWordScoreFileName);
        //        await SaveList<List<WordScoreItem>>(BestWordScoresSimple, BestWordScoresSimpleFileName);
        //        await SaveList<List<WordScoreItem>>(LongestWords, BestLongestWordsFileName);
        //        //SaveList<List<WordScoreItem>>(AllWords, AllWordScoresFileName);
        //        await SaveList<List<WordScoreItem>>(FortuneWordScoreHistory, FortuneScoresFileName);
        //        await SaveList<List<WordScoreItem>>(HistoryWords, HistoryScoresFileName);
        //    }
        //}

        //internal async static Task<bool> LoadGame()
        //{
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //    int w = gridsize - 1;

        //    if(localSettings.Values.Keys.Contains(w.ToString() + "_" + w.ToString() + "_letter"))
        //    {
        //        // Create a simple setting
        //        for (int i = 0; i < gridsize; i++)
        //        {
        //            for (int j = 0; j < gridsize; j++)
        //            {
        //                string key = i.ToString() + "_" + j.ToString();
        //                byte letter = (byte)localSettings.Values[key + "_letter"];
        //                LetterProp.TileTypes tt = (LetterProp.TileTypes)localSettings.Values[key + "_type"];
        //                LetterPropGrid[i, j] = new LetterProp(letter, tt, i, j);

        //                LetterPropGrid[i, j].b.Click += LetterClick;

        //                LetterGrid.Children.Add(LetterPropGrid[i, j].b);
        //            }
        //        }

        //        level = (int)localSettings.Values["level"];
        //        Manna = (int)localSettings.Values["manna"];
        //        totalScore = (int)localSettings.Values["score"];
        //        totalwords = (int)localSettings.Values["totalwords"];
        //        HighScoreWordTally = (string)localSettings.Values["best"];
        //        Efficiency = (double)localSettings.Values["eff"];

        //        UpdateFortune();

        //        UpdateStats();
        //        Spells.UpdateSpellsForLevel(level);
        //        LetterProp.InitProbability(level);

        //        BestWordScores = await LoadList<List<WordScoreItem>>(BestWordScoreFileName);
        //        BestWordScoresSimple = await LoadList<List<WordScoreItem>>(BestWordScoresSimpleFileName);
        //        LongestWords = await LoadList<List<WordScoreItem>>(BestLongestWordsFileName);
        //        BestGameScores = await LoadList<List<int>>(BestOverallScoresFileName);
        //        FortuneWordScoreHistory = await LoadList<List<WordScoreItem>>(FortuneScoresFileName);
        //        HistoryWords = await LoadList<List<WordScoreItem>>(HistoryScoresFileName);

        //        if (HistoryWords != null)
        //        {
        //            foreach(WordScoreItem wsi in HistoryWords)
        //            {
        //                HistoryList.Items.Add(wsi.wordscorestring);
        //            }
        //        }
        //        else
        //        {
        //            HistoryWords = new List<WordScoreItem>();
        //        }

        //        if(FortuneWordScoreHistory == null)
        //        {
        //            FortuneWordScoreHistory = new List<WordScoreItem>();
        //        }

        //        if (BestWordScores == null)
        //        {
        //            BestWordScores = new List<WordScoreItem>();
        //        }

        //        if (BestWordScoresSimple == null)
        //        {
        //            BestWordScoresSimple = new List<WordScoreItem>();
        //        }

        //        if (LongestWords == null)
        //        {
        //            LongestWords = new List<WordScoreItem>();
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        
        //public static bool IsSavedGame()
        //{
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //    if (localSettings.Values.Keys.Contains("SavedGame"))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public static void SetSavedGame()
        //{
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //    localSettings.Values["SavedGame"] = 1;
        //}

        //public static void ResetSavedGame()
        //{
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //    if (localSettings.Values.Keys.Contains("SavedGame"))
        //    {
        //        localSettings.Values.Remove("SavedGame");
        //    }
        //}

        //internal async static Task SaveStats()
        //{
        //    // Add the latest high score
        //    if(BestGameScores == null)
        //    {
        //        BestGameScores = new List<int>();
        //    }

        //    int indx = BestGameScores.FindIndex(f => (f < totalScore));
        //    if (indx >= 0)
        //    {
        //        BestGameScores.Insert(indx, totalScore);
        //    }
        //    else
        //    {
        //        BestGameScores.Add(totalScore);
        //    }
        //    if (BestGameScores.Count > NumberOfTopScores)
        //    {
        //        BestGameScores.RemoveAt(NumberOfTopScores);
        //    }

        //    // Save Best Word, Best Game stats
        //    await SaveList<List<WordScoreItem>>(BestWordScores, BestWordScoreFileName);
        //    await SaveList<List<WordScoreItem>>(BestWordScoresSimple, BestWordScoresSimpleFileName);
        //    await SaveList<List<WordScoreItem>>(LongestWords, BestLongestWordsFileName);
        //    await SaveList<List<int>>(BestGameScores, BestOverallScoresFileName);
        //}

        //internal static async Task SaveList<T>(T list, string file)
        //{
        //        try
        //        {
        //            StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);

        //            using(IRandomAccessStream s = await sampleFile.OpenAsync(FileAccessMode.ReadWrite))
        //            {
        //                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
        //                dcs.WriteObject(s.AsStreamForWrite(), list);
        //                await s.FlushAsync();
        //            }
        //        }

        //        catch (Exception ex)
        //        {
        //            Announcment a = new Announcment(file + " " + ex.Message);
        //            await a.ShowAsync();
        //        }
        //}

        //internal static async Task LoadStats()
        //{
        //    // Save Best Word, Best Game stats
        //    BestWordScores = await LoadList<List<WordScoreItem>>(BestWordScoreFileName);
        //    BestWordScoresSimple = await LoadList<List<WordScoreItem>>(BestWordScoresSimpleFileName);
        //    LongestWords = await LoadList<List<WordScoreItem>>(BestLongestWordsFileName);
        //    BestGameScores = await LoadList<List<int>>(BestOverallScoresFileName);

        //    if(BestWordScores == null)
        //    {
        //        BestWordScores = new List<WordScoreItem>();
        //    }

        //    if (LongestWords == null)
        //    {
        //        LongestWords = new List<WordScoreItem>();
        //    }
        //    if (BestWordScoresSimple == null)
        //    {
        //        BestWordScoresSimple = new List<WordScoreItem>();
        //    }
        //    if (BestGameScores == null)
        //    {
        //        BestGameScores = new List<int>();
        //    }
        //}

        //internal static async Task<T> LoadList<T>(string file)
        //{

        //    StorageFile sampleFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
        //    try
        //    {
        //        if (sampleFile != null)
        //        {
        //            T list;
        //            using (IInputStream s = await sampleFile.OpenReadAsync())
        //            {
        //                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
        //                list = (T)dcs.ReadObject(s.AsStreamForRead());
        //            }

        //            return list;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if(!(ex is System.Xml.XmlException))
        //        {
        //            Announcment a = new Announcment(file + " " + ex.Message);
        //            await a.ShowAsync();
        //        }
        //    }

        //    return default(T);
        //}


        //internal static void FillBestWords(ListBox best_Words)
        //{
        //    best_Words.Items.Clear();

        //    foreach(WordScoreItem wsi in BestWordScores)
        //    {
        //        best_Words.Items.Add(wsi.wordscorestring);               
        //    }
        //}

        //internal static void FillBestWordsSimple(ListBox best_Words)
        //{
        //    best_Words.Items.Clear();

        //    foreach (WordScoreItem wsi in BestWordScoresSimple)
        //    {
        //        best_Words.Items.Add(wsi.word + " " + wsi.simplescore);
        //    }
        //}

        //internal static void FillTopScores(ListBox highScores)
        //{
        //    highScores.Items.Clear();

        //    foreach (int score in BestGameScores)
        //    {
        //        highScores.Items.Add(score.ToString());
        //    }
        //}

        //internal static void FillTopLongWords(ListBox highScores)
        //{
        //    highScores.Items.Clear();

        //    foreach (WordScoreItem wsi in LongestWords)
        //    {
        //        highScores.Items.Add(wsi.word);
        //    }
        //}

        //internal async static void ResetScores()
        //{
        //    StorageFile sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestWordScoreFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();
        //    sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestWordScoresSimpleFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();
        //    sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestLongestWordsFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();
        //    sf = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(BestOverallScoresFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();

        //    BestGameScores.Clear();
        //    BestWordScores.Clear();
        //    BestWordScoresSimple.Clear();
        //    LongestWords.Clear();

        //    Efficiency = 0;
        //    level = 0;
        //    totalScore = 0;
        //    Manna = 0;
        //}

        //internal async static Task EndGame()
        //{
        //    // Remove saved game setings.
        //    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        //    int w = gridsize - 1;

        //    if (localSettings.Values.Keys.Contains(w.ToString() + "_" + w.ToString() + "_letter"))
        //    {
        //        localSettings.Values.Remove(w.ToString() + "_" + w.ToString() + "_letter");
        //    }

        //    FortuneWordScoreHistory.Clear();
        //    AllWords.Clear();

        //    StorageFile sf = await ApplicationData.Current.RoamingFolder.CreateFileAsync(FortuneScoresFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();
        //    sf = await ApplicationData.Current.RoamingFolder.CreateFileAsync(AllWordScoresFileName, CreationCollisionOption.ReplaceExisting);
        //    await sf.DeleteAsync();

        //    Efficiency = 0;
        //    level = 0;
        //    totalScore = 0;
        //    Manna = 0;
        //}

    }
}

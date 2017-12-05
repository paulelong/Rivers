﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;


namespace WordSpell
{
    partial class WSGameState
    {
        #region Members
        static GameStats gs = new GameStats();
        static OverallStats os = new OverallStats();
        static List<WordScoreItem> TryWordList = new List<WordScoreItem>();

        #endregion Members


        #region Properties

        static public List<SpellInfo> AwardedSpells
        {
            get { return gs.awarded; }
        }

        static public List<string> LongestWords
        {
            get
            {
                List<string> longestWordStrings = new List<string>();
                foreach (WordScoreItem wsi in os.LongestWords)
                {
                    longestWordStrings.Add(wsi.word + " " + wsi.word.Length.ToString());
                }
                return longestWordStrings;
            }
        }

        static public List<string> BestGameScores
        {
            get 
            {
                List<string> bestGameScores = new List<string>();

                foreach (int i in os.BestGameScores)
                {
                    bestGameScores.Add(i.ToString());
                }
                return bestGameScores; 
            }
        }

        static public List<string> BestWordScores
        {
            get
            {
                List<string> bestWordStrings = new List<string>();
                foreach (WordScoreItem wsi in os.BestWordScores)
                {
                    bestWordStrings.Add(wsi.word + " " + wsi.wordscorestring);
                }
                return bestWordStrings;
            }
        }

        static public List<string> BestWordScoresSimple
        {
            get
            {
                List<string> bestWordScoresSimple = new List<string>();
                foreach (WordScoreItem wsi in os.BestWordScores)
                {
                    bestWordScoresSimple.Add(wsi.word + " " + wsi.simplescore);
                }
                return bestWordScoresSimple;
            }
        }

        #endregion Properties


        static public Board boardScript;

        static public LetterProp[,] LetterPropGrid = null;

        static List<LetterProp> SelLetterList = new List<LetterProp>();

        public const int gridsize = 9;

        static System.Random r;

        public static int CurrentLevel { get { return gs.level; } private set { } }

        public static double TotalEfficiency { get; internal set; }

#if UNITY_EDITOR
        private static int[] Levels = { 0, 20, 40, 60, 80, 100, 120, 140, 160, 180, 200, 220, 1300, 1600, 2000, 5000, 10000 };
#else
        private static int[] Levels = { 0, 25, 60, 100, 160, 230, 310, 400, 500, 650, 850, 1000, 1300, 1600, 2000, 2500, 3000, 4600, 5200, 10000, 20000, 30000  };
#endif

        private static bool levelup = false;

        //public static int totalScore;
        public static int HighScoreWordValue = 0;
        public static string HighScoreWord;
        public static string HighScoreWordTally;
        public static int totalwords = 0;
        public static double Efficiency;
        private const int EffWordCount = 3;
        private const int NumberOfTopScores = 20;

        static Material BadFortuneMaterial;
        static Material GoodFortuneMaterial;
        static Material GreatFortuneMaterial;

        private static int FortuneLevelCount;

        private static bool resume = false;
        public static bool Resume { get { return resume; } set { resume = value; } }

        internal static void DebugMode()
        {
            gs.mana = 100;
            gs.level = 20;
        }

        public enum FortuneLevel
        {
            Bad,
            Good,
            Great,
        }

        public static FortuneLevel GetFortune()
        {
            if (Efficiency > 12)
            {
                return FortuneLevel.Great;
            }
            else if (Efficiency >= 10)
            {
                return FortuneLevel.Good;
            }

            return FortuneLevel.Bad;
        }

        public static FortuneLevel GetFortune(int score)
        {
            if (score > 12)
            {
                return FortuneLevel.Great;
            }
            else if (score >= 10)
            {
                return FortuneLevel.Good;
            }

            return FortuneLevel.Bad;
        }

        internal static void NewMusicTile()
        {
            int ti = r.Next(gridsize);
            LetterPropGrid[ti, gridsize - 1].PlayBackgroundMusic();
        }

        #region Init

        #region Persistence
        internal static void Save()
        {
            GamePersistence.SaveGame(LetterPropGrid, gs);
        }

        internal static void Load()
        {
            GameStats _gs;
            _gs = GamePersistence.LoadGame(LetterPropGrid);
            if(_gs != null)
            {
                Deselect(null);
                gs = _gs;
                UpdateStats();
                UpdateFortune();

                foreach(WordScoreItem wsi in gs.history)
                {
                    boardScript.AddHistory(wsi.word + " " + wsi.wordscorestring);
                }

                // Activate any tile specific work, like music.
                for (int i = 0; i < gridsize; i++)
                {
                    for(int j = 0; j< gridsize; j++)
                    {
                        if(LetterPropGrid[i, j].TileType == LetterProp.TileTypes.Speaker)
                        {
                            LetterPropGrid[i, j].PlayBackgroundMusic();
                        }
                    }

                }
            }
            else
            {
                NewMusicTile();
            }
        }

        internal static void LoadStats()
        {
            OverallStats _os = GamePersistence.LoadOverallStats();
            if(_os != null)
            {
                os = _os;
            }
        }

        internal static void SaveStats()
        {
            GamePersistence.SaveOverallStats(os);
        }

        #endregion Persistence

        public static void InitGameGlobal()
        {
            GameObject go = GameObject.Find("BoardBackground");
            boardScript = (Board)go.GetComponent(typeof(Board));

            EngLetterScoring.LoadDictionary();
            LetterProp.InitLetterPropertyList(boardScript);

            BadFortuneMaterial = (Material)Resources.Load("Copper");
            GoodFortuneMaterial = (Material)Resources.Load("Silver");
            GreatFortuneMaterial = (Material)Resources.Load("Gold");
        }

        public static void InitNewGame()
        {

            // Reset all variables
            Replay();

            // Stuff useful for development, don't ship
#if UNITY_EDITOR
            AwardAllSpells();
#endif
            r = new System.Random();

            LetterProp.InitProbability(gs.level);

            LetterPropGrid = new LetterProp[gridsize, gridsize];

            Spells.UpdateSpellsForLevel(gs.level);

            //NewMusicTile();
        }

        #endregion init

        private static void UpdateManaScore()
        {
            boardScript.SetMana(gs.mana.ToString());
        }

        internal static void UpdateFortune()
        {
            //Sel
            //SolidColorBrush scb = GetFortuneColor();
            //SetGridColor(scb);
            //EffText.Foreground = scb;
        }

        internal static string GetWordTally()
        {
            return EngLetterScoring.GetWordTally(SelLetterList);
        }

        public static void NewLetter(int i, int j, Transform tf)
        {
            LetterPropGrid[i, j] = new LetterProp(gs.level, levelup, i, j, tf);

            if (levelup == true)
            {
                levelup = false;
            }
        } 

        public static void LetterClick(int i, int j)
        {
            LetterProp lp = LetterPropGrid[i, j];

            if(!Spells.EvalSpell(lp))
            {
                if (SelLetterList.Count >= 0)
                {
                    //string curword = GetCurrentWord().ToLower();
                    // Check if button is adject to the last
                    if (!(IsLetterAdjacentToLastButton(lp) && !SelLetterList.Contains(lp)))
                    {
                        // Deselect except for the one you just clicked.
                        if(!Deselect(lp))
                        {
                            lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                            //lp.SetSelected(true);
                        }
                    }
                    else
                    {
                        //lp.SetSelected(true);
                        lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                    }

                    SelLetterList.Add(lp);

                    boardScript.SetCurrentWord(GetCurrentWord() + "\n" + GetWordTally());

                    // add if for > 3 letters
                    //CurrentWord.Text = GetWordTally();

                    // if it's a word, update color to green
                    if (SelLetterList.Count > 2 && EngLetterScoring.IsWord(GetCurrentWord()))
                    {
                        boardScript.IndicateGoodWord(true);

                        // if it's a word, remember it.
                        AddToTryList();
                    }
                    else
                    {
                        boardScript.IndicateGoodWord(false);
                    }
                }
            }
        }

        private static void AddToTryList()
        {
            WordScoreItem wsi = new WordScoreItem() { word = GetCurrentWord(), score = ScoreWord(), wordscorestring = EngLetterScoring.GetWordTally(SelLetterList), simplescore = ScoreWordSimple() };

            if(TryWordList.FindIndex(f => (f.word == wsi.word)) >= 0)
            {
                return;
            }

            int indx = TryWordList.FindIndex(f => (f.score < wsi.score));
            if (indx >= 0)
            {
                TryWordList.Insert(indx, wsi);
            }
            else
            {
                TryWordList.Add(wsi);
            }

            boardScript.ClearTryList();
            foreach (WordScoreItem wsi_I in TryWordList)
            {
                boardScript.AddTryList(wsi_I.word + " " + wsi_I.score.ToString());
            }
        }


        internal static void TurnOver()
        {
            boardScript.ClearTryList();
            TryWordList.Clear();
        }

        internal static void Replay()
        {
            gs.score = 0;
            gs.level = 1;
            TotalEfficiency = 0;
            Efficiency = 0;
            HighScoreWordValue = 0;
            HighScoreWord = "";
            totalwords = 0;
            gs.mana = 0;

            UpdateStats();
        }

        internal static void SubmitWord()
        {
            string s = GetCurrentWord().ToLower();

            if (EngLetterScoring.IsWord(s))
            {
                if(gs.history.FindIndex(f => (f.word == GetCurrentWord())) >= 0)
                {
                    boardScript.ShowMsg("You've used that word already.");
                }
                else
                {
                    ScoreStats ss = RecordWordScore();
                    RemoveWordAndReplaceTiles();

                    Deselect(null);
                    boardScript.ResetSubmitButton();

                    bool gameOver = ProcessLetters();
                    if (gameOver)
                    {
                        GamePersistence.SaveOverallStats(os);
                        GamePersistence.ResetGameData();
                        RemoveGameBoard();

                        //SaveStats();
                        Resume = false;
                        boardScript.EndGanme();

                        //ResetSavedGame();
                    }
                    else
                    {
                        if (ss.MannaScore > 0)
                        {
                            //ScoreFlash.Foreground = WordWarLogic.GetFortuneColor(WordWarLogic.ScoreWord());
                            //ScoreFlash.Text = "Manna +" + ss.MannaScore;
                            //await BeginAsync(ScoreMotionSmall);
                        }

                        if (ss.bonus > 0)
                        {
                            //ScoreFlash.Foreground = WordWarLogic.GetFortuneColor(WordWarLogic.ScoreWord());
                            //ScoreFlash.Text = "Bonus +" + ss.bonus;
                            //await BeginAsync(ScoreMotionSmall);
                        }

                        if (ss.si != null)
                        {
                            boardScript.ShowMsg("Nice word, you've earned a " + ss.si.FriendlyName + " spell.");
                        }

                        TurnOver();

                        if (CheckNextLevel(gs.score))
                        {
                            boardScript.LevelSound();
                            string levelmsg = "Welcom to Level " + CurrentLevel.ToString() + "\n\n";
                            if (Spells.HasSpells())
                            {
                                levelmsg += "You have new spells";
                            }

                            boardScript.ShowMsg(levelmsg);
                        }

                        UpdateStats();
                        boardScript.ScoreWordSound();
                    }
                }
            }
            else
            {
                Deselect(null);

                //CurrentWord.Text = "Not a known word.  Try again";
            }

        }

        internal static bool EnoughMana(int mannaPoints)
        {
            return (mannaPoints > WSGameState.gs.mana);
        }

        private static void RemoveGameBoard()
        {
            for(int i = 0; i < gridsize; i++)
            {
                for(int j = 0; j < gridsize; j++)
                {
                    RemoveTile(LetterPropGrid[i, j]);
                }
            }

            boardScript.ClearHistory();
            boardScript.ClearTryList();
        }

        internal static bool CheckNextLevel(int totalScore)
        {
            if (gs.level < Levels.Length - 1 && totalScore >= Levels[gs.level])
            {
                gs.level++;
                levelup = true;

                //LevelText.Text = "L: " + CurrentLevel.ToString();

                if (gs.level >= 5)
                {
                    ChangeManna(6);
                }

                Spells.UpdateSpellsForLevel(gs.level);
                LetterProp.InitProbability(gs.level);
            }

            return levelup;
        }

        internal static int ScoreWord()
        {
            return EngLetterScoring.ScoreWord(SelLetterList);
        }

        internal static string ScoreWordString()
        {
            return EngLetterScoring.ScoreWordString(SelLetterList);
        }

        public struct ScoreStats
        {
            public int MannaScore;
            public SpellInfo si;
            public int bonus;
        }

        internal static ScoreStats RecordWordScore()
        {
            ScoreStats ss = new ScoreStats();

            int wordTotal = ScoreWord();

            boardScript.AddHistory(GetCurrentWord() + " " + GetWordTally());

            gs.score += wordTotal;
            if (wordTotal > HighScoreWordValue)
            {
                HighScoreWordValue = wordTotal;
                HighScoreWord = GetCurrentWord();
                HighScoreWordTally = EngLetterScoring.GetWordTally(SelLetterList);
            }

            WordScoreItem wsi = new WordScoreItem() { word = GetCurrentWord(), score = wordTotal, wordscorestring = EngLetterScoring.GetWordTally(SelLetterList), simplescore = ScoreWordSimple() };

            ss.bonus = EngLetterScoring.LengthBonus(wsi.word);

            gs.fortune.Add(wsi);
            if (gs.fortune.Count > EffWordCount)
            {
                gs.fortune.RemoveAt(0);
            }

            gs.history.Add(wsi);

            CheckTopBestWordScores(wsi);
            CheckTopBestWordScoresSimple(wsi);
            CheckTopLongestWordScores(wsi);

            totalwords++;

            TotalEfficiency = gs.score / totalwords;
            Efficiency = GetLatestEff();

            if (GetFortune() == FortuneLevel.Great)
            {
                FortuneLevelCount++;
            }
            else
            {
                FortuneLevelCount = 0;
            }

            if (FortuneLevelCount > 4)
            {
                gs.mana += (FortuneLevelCount - 4);
            }
            ss.MannaScore = ScoreManna();

            // If it's a big or price word, give them a spell based on the word.
            string curword = GetCurrentWord();
            if (wordTotal > 14 || curword.Length >= 8)
            {
                SpellInfo si = null;

                if (wordTotal > 70 || curword.Length > 16)
                {
                    si = Spells.GetSpell(6, gs.level);
                }
                else if (wordTotal > 55 || curword.Length > 15)
                {
                    si = Spells.GetSpell(5, gs.level);
                }
                else if (wordTotal > 45 || curword.Length > 14)
                {
                    si = Spells.GetSpell(4, gs.level);
                }
                else if (wordTotal > 35 || curword.Length > 13)
                {
                    si = Spells.GetSpell(3, gs.level);
                }
                else if (wordTotal > 25 || curword.Length > 11)
                {
                    si = Spells.GetSpell(2, gs.level);
                }
                else if (wordTotal > 17 || curword.Length > 9)
                {
                    si = Spells.GetSpell(1, gs.level);
                }
                else if (wordTotal > 14 || curword.Length >= 8)
                {
                    si = Spells.GetSpell(0, gs.level);
                }

                AwardedSpells.Add(si);
                ss.si = si;
            }

            UpdateFortune();

            UpdateStats();

            return ss;
        }

        internal static void AwardAllSpells()
        {
            foreach (SpellInfo si in Spells.AllSpells)
            {
                AwardedSpells.Add(si);
            }
        }

        internal static void RemoveAwardedSpells(SpellInfo selectedSpell)
        {
            foreach (SpellInfo si in AwardedSpells)
            {
                if (si.spellType == selectedSpell.spellType)
                {
                    AwardedSpells.Remove(si);
                    break;
                }
            }
        }

        private static void UpdateStats()
        {
            boardScript.SetScore(gs.score.ToString());

            if (totalwords > 0)
            {
                TotalEfficiency = gs.score / totalwords;
            }

            boardScript.SetLevel(gs.level.ToString());

            UpdateManaScore();
        }

        public static void ChangeManna(int manna)
        {
            gs.mana += manna;
            UpdateManaScore();
        }

        private static void CheckTopLongestWordScores(WordScoreItem wsi)
        {
            int indx = os.LongestWords.FindIndex(f => (f.word.Length < wsi.word.Length));
            if (indx >= 0)
            {
                os.LongestWords.Insert(indx, wsi);
            }
            else
            {
                os.LongestWords.Add(wsi);
            }
            if (os.LongestWords.Count > NumberOfTopScores)
            {
                os.LongestWords.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScoresSimple(WordScoreItem wsi)
        {
            int indx = os.BestWordScoresSimple.FindIndex(f => (f.score < wsi.simplescore));
            if (indx >= 0)
            {
                os.BestWordScoresSimple.Insert(indx, wsi);
            }
            else
            {
                os.BestWordScoresSimple.Add(wsi);
            }
            if (os.BestWordScoresSimple.Count > NumberOfTopScores)
            {
                os.BestWordScoresSimple.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScores(WordScoreItem wsi)
        {
            int indx = os.BestWordScores.FindIndex(f => (f.score < wsi.score));
            if (indx >= 0)
            {
                os.BestWordScores.Insert(indx, wsi);
            }
            else
            {
                os.BestWordScores.Add(wsi);
            }
            if (os.BestWordScores.Count > NumberOfTopScores)
            {
                os.BestWordScores.RemoveAt(NumberOfTopScores);
            }
        }

        private static int ScoreWordSimple()
        {
            return EngLetterScoring.ScoreWordSimple(SelLetterList);
        }

        private static double GetLatestEff()
        {
            int wordtotal = 0;
            foreach (WordScoreItem wsi in gs.fortune)
            {
                wordtotal += wsi.score;
            }

            return (double)wordtotal / (double)gs.fortune.Count;
        }

        internal static Material GetFortuneColor()
        {
            switch (GetFortune())
            {
                case FortuneLevel.Bad:
                    return (BadFortuneMaterial);
                case FortuneLevel.Good:
                    return (GoodFortuneMaterial);
                case FortuneLevel.Great:
                    return (GreatFortuneMaterial);
                default:
                    return (BadFortuneMaterial);
            }
        }

        public static int ScoreManna()
        {
            int addedManna = EngLetterScoring.ScoreManna(SelLetterList);
            gs.mana += addedManna;
            UpdateManaScore();

            return addedManna;
        }

        private static bool IsLetterAdjacentToLastButton(LetterProp lp1)
        {
            if (SelLetterList.Count <= 0)
            {
                return true;
            }

            LetterProp lp2 = SelLetterList.Last();

            if (Math.Abs(lp1.I - lp2.I) <= 1 && Math.Abs(lp1.J - lp2.J) <= 1)
            {
                return true;
            }

            return false;
        }

        public static bool Deselect(LetterProp lp_sel)
        {
            bool stillSelected = false;
            // Deselect everything and add this as the start
            foreach (LetterProp lp in SelLetterList)
            {
                if (lp != lp_sel)
                {
                    boardScript.DeselectLet(lp.SelectorObject);
                    lp.SelectorObject = null;
                    //lp.SetSelected(false);
                }
                else
                {
                    stillSelected = true;
                }
            }

            SelLetterList.Clear();
            return (stillSelected);
        }

        internal static bool ProcessLetters()
        {
            for (int i = 0; i < gridsize; i++)
            {
                for (int j = 0; j < gridsize; j++)
                {
                    LetterProp curlp = LetterPropGrid[i, j];
                    if (curlp.IsBurning())
                    {
                        if (curlp.J <= 0)
                        {
                            // GameOver
                            return true;
                        }

                        RemoveAndReplaceTile(curlp.I, curlp.J - 1);
                    }
                }
            }

            WordWarAI wwai = new WordWarAI(LetterPropGrid);
            return (!wwai.AnyWords());
        }

        public static void RemoveAndReplaceTile(int i, int j)
        {
            LetterProp toRemove = LetterPropGrid[i, j];

            Vector3 oldpos = LetterPropGrid[i, j].Tf.position;

            for (int jp = j; jp < gridsize - 1; jp++)
            {
                LetterProp LetterOntop = LetterPropGrid[i, jp + 1];
                //Tile t = (Tile)LetterOntop.Tf.GetChild(0).gameObject.GetComponent(typeof(Tile));
                Tile t = (Tile)LetterOntop.Tf.gameObject.GetComponent(typeof(Tile));
                t.SetPos(i, jp);

                LetterOntop.J = jp;

                LetterPropGrid[i, jp] = LetterPropGrid[i, jp + 1];

                LetterOntop.LetterDCount++;
            }

            float fallCount = LetterPropGrid[i, gridsize - 1].LetterDCount;
            Transform lbi = boardScript.NewTile(i, gridsize - 1, fallCount);
            NewLetter(i, gridsize - 1, lbi);
            LetterPropGrid[i, gridsize - 1].LetterDCount = fallCount;

            RemoveTile(toRemove);
        }


        public static void RemoveTile(LetterProp toRemove)
        {
            toRemove.Tf.Translate(0.0f, 0.0f, -1f);

            //Animator a = toRemove.Tf.GetComponent<Animator>();
            //a.enabled = false;
            toRemove.AnimationEnabled = false;

            Rigidbody rb = toRemove.Tf.GetComponent(typeof(Rigidbody)) as Rigidbody;
            rb.useGravity = true;
            rb.isKinematic = false;

            float xf = (r.Next(100) - 50f) / 150f;
            float yf = (r.Next(10) - 5f) / 1f;
            float zf = (r.Next(100) / 30f);
            rb.AddForce(new Vector3(xf, yf, -zf), ForceMode.VelocityChange);

            float xr = r.Next(100) / 10f;
            float yr =  r.Next(100) / 10f;
            float zr = r.Next(100) / 1f;
            rb.AddTorque(new Vector3(xr, yr, zr), ForceMode.VelocityChange);

            if(toRemove.MusicHolderRole)
            {
                LetterPropGrid[4, 8].PlayBackgroundMusic();
            }
        }

        public static void RemoveWordAndReplaceTiles()
        {
            foreach (LetterProp lp in SelLetterList)
            {
                RemoveAndReplaceTile(lp.I, lp.J);
            }
        }

        internal static string GetCurrentWord()
        {
            return EngLetterScoring.GetCurrentWord(SelLetterList);
        }

    }
}

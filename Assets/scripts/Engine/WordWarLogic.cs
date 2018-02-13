using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;


namespace WordSpell
{
    public static class WSGameState
    {
        #region Constants
        public const int MAX_GRID_SIZE = 9;

        private const int EFF_WORD_COUNT = 3;
        private const int TOP_SCORES_MAX = 20;

        const int EffHigh = 13;
        const int EffMed = 10;
        const int FortuneMaxOver = 10;
        const int glbseed = 0; //319266411;

        private const float LowestWordScore = 3f;

        public enum FortuneLevel
        {
            Bad,
            Good,
            Great,
        }

        public enum WordValidity
        {
            Garbage,
            Word,
            UsedWord,
        }

        #endregion Constants

        public struct ScoreStats
        {
            public int MannaScore;
            public SpellInfo si;
            public int bonus;
        }

        #region Privates
        private static GameObject selMagicTile;
        internal static bool dbg = false;

        static GameStats gs = new GameStats();
        static OverallStats os = new OverallStats();
        static List<WordScoreItem> TryWordList = new List<WordScoreItem>();
        static List<LetterProp> SelLetterList = new List<LetterProp>();

        static System.Random r = new System.Random();


#if UNITY_EDITOR
        private static int[] Levels = { 0, 20, 40, 60, 87, 100, 120, 140, 160, 180, 200, 220, 1300, 1600, 2000, 5000, 10000 };
#else
        private static int[] Levels = { 0, 25, 60, 100, 160, 230, 310, 400, 500, 650, 850, 1000, 1300, 1600, 2000, 2500, 3000, 4600, 5200, 10000, 20000, 30000  };
#endif

        private static bool levelup = false;
        private static int totalwords = 0;
        private static bool backdoor = false;

        static Material BadFortuneMaterial;
        static Material GoodFortuneMaterial;
        static Material GreatFortuneMaterial;
        static Material ManaMaterial;

        private static int FortuneLevelCount;

        private static bool resume = false;
        private static bool gameOver = false;
        #endregion Privates

        #region Properties

        public static System.Random Rnd
        { 
            get
            {
                return r;
            } 
        } 

        public static int CurrentLevel { get { return gs.level; } private set { } }

        public static double TotalEfficiency { get; internal set; }

        static public List<SpellInfo> AwardedSpells
        {
            get { return gs.awarded; }
        }

        static public List<string> LongestWords
        {
            get
            {
                List<string> longestWordStrings = new List<string>();
                if (os.LongestWords != null)
                {
                    foreach (WordScoreItem wsi in os.LongestWords)
                    {
                        longestWordStrings.Add(wsi.Word + " " + wsi.Word.Length.ToString());
                    }
                }
                else
                {
                    Logging.StartDbg("lwp!");
                }

                return longestWordStrings;
            }
        }

        static public List<string> BestGameScores
        {
            get 
            {
                List<string> bestGameScores = new List<string>();

                if (os.BestGameScores != null)
                {
                    foreach (BestGameScore bgs in os.BestGameScores)
                    {
                        float ratio = (float)bgs.score / (float)bgs.totalWords;
                        
                        bestGameScores.Add(string.Format("{0} L{2} {1:0.0} ", bgs.score, ratio, bgs.level));
                    }
                }
                else
                {
                    Logging.StartDbg("bgsp!");
                }

                return bestGameScores; 
            }
        }

        static public List<string> BestWordScores
        {
            get
            {
                List<string> bestWordStrings = new List<string>();

                if (os.BestWordScores != null)
                {
                    foreach (WordScoreItem wsi in os.BestWordScores)
                    {
                        bestWordStrings.Add(wsi.Score + " " + wsi.Word + " " + wsi.Wordscorestring);
                    }
                }
                else
                {
                    Logging.StartDbg("bwsp!");
                }

                return bestWordStrings;
            }
        }

        static public List<string> BestWordScoresSimple
        {
            get
            {
                List<string> bestWordScoresSimple = new List<string>();

                if (os.BestWordScores != null)
                {
                    foreach (WordScoreItem wsi in os.BestWordScores)
                    {
                        bestWordScoresSimple.Add(wsi.Simplescore + " " + wsi.Word);
                    }
                }
                else
                {
                    Logging.StartDbg("bwss!");
                }

                return bestWordScoresSimple;
            }
        }

        public static bool Resume { get { return resume; } set { resume = value; } }

        public static bool IsGameOver
        {
            get
            {
                return gameOver;
            }

            set
            {
                gameOver = value;
            }
        }

        public static bool GameInProgress
        {
            get
            {
                if (!IsGameOver && LetterPropGrid != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        static public float GridScale
        {
            get
            {
                return scaleSize;
            }
        }

        public static int Gridsize
        {
            get
            {
                return gridsize;
            }

            set
            {
                gridsize = value;
                scaleSize = (float)MAX_GRID_SIZE / (float)gridsize;
                halfOffset = (float)(gridsize - 1) / 2f;
            }
        }

        public static float HalfOffset
        {
            get
            {
                return halfOffset;
            }
        }

        #endregion Properties

        #region Fields
        static public Board boardScript;

        static public LetterProp[,] LetterPropGrid = null;

        static float scaleSize = 0;

        static int gridsize = 7;
        static float halfOffset = 0;
        #endregion Fields

        internal static void DebugMode()
        {
            gs.mana = 100;
            gs.level = 20;
        }

        #region Init

        #region Persistence
        internal static void Save()
        {
            GamePersistence.SaveGame(LetterPropGrid, gs);
        }

        internal static void Load()
        {
            gs = GamePersistence.gs;

            Deselect(null);

            UpdateStats();

            CreateNewBoard(gs.boardsize);

            GamePersistence.RestoreGameData(WSGameState.LetterPropGrid);

            foreach (WordScoreItem wsi in gs.history)
            {
                boardScript.AddHistory(wsi.Score + " " + wsi.Word + " " + wsi.Wordscorestring);
            }

            bool FoundMusic = false; // Work around bug so multiple speaker tiles don't show after reload

            // Activate any tile specific work, like music.
            for (int i = 0; i < Gridsize; i++)
            {
                for(int j = 0; j< Gridsize; j++)
                {
                    if(LetterPropGrid[i, j].TileType == LetterProp.TileTypes.Speaker && !FoundMusic)
                    {
                        LetterPropGrid[i, j].PlayBackgroundMusic();
                        FoundMusic = true;
                    }
                }
            }

            Spells.UpdateSpellsForLevel(gs.level);

        }

        internal static void LoadStats()
        {
            OverallStats _os = GamePersistence.Os;
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

            LetterProp.InitLetterPropertyList(boardScript);

            BadFortuneMaterial = Resources.Load("Copper") as Material;
            GoodFortuneMaterial = Resources.Load("Silver") as Material;
            GreatFortuneMaterial = Resources.Load("Gold") as Material;
            ManaMaterial = Resources.Load("Mana") as Material;

            // Load the music for the speaker tiles just once.
            Songs.LoadMusic();

            boardScript.ShowMsg("Loading dictionary...");
            EngLetterScoring.LoadDictionary();
            boardScript.HideMsg();
        }

        public static void InitNewGame()
        {

            Logging.StartDbg("Ing0");
            // Reset all variables
            Replay();

            // Stuff useful for development, don't ship
#if UNITY_EDITOR
            AwardAllSpells();
            if (glbseed == 0)
            {
                int seed = (int)DateTime.Now.Ticks;
                Logging.StartDbg("sd=" + seed.ToString());
                r = new System.Random(seed);
            }
            else
            {
                Logging.StartDbg("sd=" + glbseed.ToString());
                r = new System.Random(glbseed);
            }
#else
            int seed = (int)DateTime.Now.Ticks;
            Logging.StartDbg("sd=" + seed.ToString());
            r = new System.Random(seed);
#endif

            LetterProp.InitProbability(gs.level);

            IsGameOver = false;

            UpdateStats();
            Logging.StartDbg("Ingx");
        }

        public static void LoadGame()
        {
            WSGameState.Load();
        }

        public static void CreateNewBoard(int size)
        {
            Gridsize = size;
            LetterPropGrid = new LetterProp[Gridsize, Gridsize];

            for (int i = 0; i < WSGameState.Gridsize; i++)
            {
                for (int j = 0; j < WSGameState.Gridsize; j++)
                {
                    WSGameState.NewLetter(i, j);
                }
            }
        }

        public static void NewLetter(int i, int j, float fallcount = 0.0f)
        {
            LetterPropGrid[i, j] = new LetterProp(gs.level, levelup, i, j, fallcount);

            if (levelup == true)
            {
                levelup = false;
            }

        }

        //public static LetterProp NewLetter(int i, int j)
        //{
        //    LetterPropGrid[i, j] = new LetterProp(gs.level, levelup, i, j);

        //    if (levelup == true)
        //    {
        //        levelup = false;
        //    }

        //    return (LetterPropGrid[i, j]);
        //}

        internal static void NewMusicTile()
        {
            int ti = WSGameState.Rnd.Next(Gridsize - 1);
            LetterPropGrid[ti, Gridsize - 1].PlayBackgroundMusic();
            Logging.PlayDbg("mt(" + LetterPropGrid[ti, Gridsize - 1].ASCIIString + "," + ti + ")");
        }

        internal static void Replay()
        {
            gs.score = 0;
            gs.level = 1;
            gs.mana = 0;

            gs.awarded.Clear();
            gs.history.Clear();

            TotalEfficiency = 0;
            //HighScoreWordValue = 0;
            //HighScoreWord = "";
            totalwords = 0;

            Logging.StartDbg("r1");
            UpdateStats();
        }

        #endregion init

        #region Main
        public static void LetterClick(int i, int j)
        {
            LetterProp lp = LetterPropGrid[i, j];

            boardScript.ResetTimer();

            if (!Spells.EvalSpell(lp))
            {
                if (SelLetterList.Count >= 0)
                {
                    // Check if button is adject to the last
                    if (!(IsLetterAdjacentToLastButton(lp) && !SelLetterList.Contains(lp)))
                    {
                        // Deselect except for the one you just clicked.
                        if (!Deselect(lp))
                        {
                            lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                        }
                    }
                    else
                    {
                        lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                    }

                    SelLetterList.Add(lp);


                    // add if for > 3 letters

                    // if it's a word, update color to green, unless we used it before
                    if (SelLetterList.Count > 2 && EngLetterScoring.IsWord(GetCurrentWord()))
                    {
                        if (gs.history.FindIndex(f => (f.Word == GetCurrentWord())) >= 0)
                        {
                            boardScript.SetCurrentWord(GetCurrentWord() + " = " + ScoreWord() + ", already used\n" + GetWordTally());
                            boardScript.IndicateGoodWord(WSGameState.WordValidity.UsedWord);
                        }
                        else
                        {
                            boardScript.SetCurrentWord(GetCurrentWord() + " = " + ScoreWord() + ". submit?\n" + GetWordTally());
                            boardScript.IndicateGoodWord(WSGameState.WordValidity.Word);
                        }

                        // if it's a word, remember it.
                        AddToTryList();
                    }
                    else
                    {
                        boardScript.SetCurrentWord(GetCurrentWord() + " = " + ScoreWord() + "\n" + GetWordTally());
                        boardScript.IndicateGoodWord(WSGameState.WordValidity.Garbage);
                    }
                }
            }
        }

        internal static void SubmitWord()
        {
            Logging.PlayDbg("Subx");
            string s = GetCurrentWord().ToLower();
            Logging.PlayDbg("Sub00");

            if (EngLetterScoring.IsWord(s))
            {
                if (gs.history.FindIndex(f => (f.Word == GetCurrentWord())) >= 0)
                {
                    boardScript.ShowMsg("You've used that word already.");
                    Deselect(null);
                }
                else
                {
                    Logging.PlayDbg("Sub0");
                    ScoreStats ss = RecordWordScore();

#if UNITY_EDITOR
                    Logging.PlayDbg("Sub0.1");
                    Save();
#endif
                    bool GainedNextLevel = false;
                    if (GainedNextLevel = CheckNextLevel(gs.score))
                    {
                        levelup = true;
                    }

                    Logging.PlayDbg("Sub1");
                    RemoveWordAndReplaceTiles();
                    IsGameOver = ProcessLetters();
                    Logging.PlayDbg("Sub2");

                    Deselect(null);

                    boardScript.ResetSubmitButton();

                    if (IsGameOver)
                    {
                        GameOver();
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
                            boardScript.RefreshSpells();
                        }

                        TurnOver();
                        boardScript.ScoreWordSound();

                        if (GainedNextLevel)
                        {
                            boardScript.LevelSound();
                            string levelmsg = "Welcome to Level " + CurrentLevel.ToString() + "\n\n";
                            if (Spells.HasSpells())
                            {
                                levelmsg += "You have new spells.  Spells require Mana which you collect by spelling words using purple Mana tiles.\n\n";
                                boardScript.RefreshSpells();
                            }
                            levelmsg += EngLetterScoring.GetLevelMsg(CurrentLevel);

                            boardScript.ShowMsg(levelmsg);
                        }

                        UpdateStats();
                    }
                }
            }
            else
            {
                if (s.Length < 3)
                {
                    if (s.Length == 0)
                    {
                        boardScript.ShowMsg("Select adjacent tiles in any direction to spells words.  When the word is valid, submit button will turn green.  Words must be 3 or more letters long.");
                    }
                    else
                    {
                        boardScript.ShowMsg("Only words greater than 3 letters are accepted.");
                    }
                }
                else
                {
                    boardScript.ShowMsg(EngLetterScoring.GetIncorrectWordPhrase());
                }

                if (s == "x")
                {
                    backdoor = true;
                }
                else
                {
                    if (s == "y" && backdoor)
                    {
                        boardScript.ShowMsg("You've found the backdoor.");
                        AwardAllSpells();
                    }
                    else
                    {
                        backdoor = false;
                    }
                }

                Deselect(null);
            }

            Logging.PlayDbg("SubX", last: true);
        }

        internal static Material GetMagicMat()
        {
            return ManaMaterial;
        }

        static void RecoreGameScore(int totalScore)
        {
            if (totalScore >= 0)
            {
                BestGameScore bsc = new BestGameScore();

                bsc.totalWords = gs.history.Count;
                bsc.score = totalScore;
                bsc.level = CurrentLevel;

                int indx = os.BestGameScores.FindIndex(f => (f.score < totalScore));
                if (indx >= 0)
                {
                    os.BestGameScores.Insert(indx, bsc);
                }
                else
                {
                    os.BestGameScores.Add(bsc);
                }

                if (os.BestGameScores.Count > TOP_SCORES_MAX)
                {
                    os.BestGameScores.RemoveAt(TOP_SCORES_MAX);
                }
            }
        }

        public static void RemoveAndReplaceTile(int i, int j)
        {
            LetterProp toRemove = LetterPropGrid[i, j];

            Vector3 oldpos = LetterPropGrid[i, j].LetTF.position;

            for (int jp = j; jp < Gridsize - 1; jp++)
            {
                LetterProp LetterOntop = LetterPropGrid[i, jp + 1];

                LetterOntop.J = jp;

                LetterPropGrid[i, jp] = LetterPropGrid[i, jp + 1];

                LetterOntop.LetterDCount++;
            }

            if (i == 6 && LetterPropGrid[i, Gridsize - 1].ASCIIChar == 'O')
            {
                dbg = true;
            }

            float fallCount = LetterPropGrid[i, Gridsize - 1].LetterDCount;

            NewLetter(i, Gridsize - 1, fallCount);

            LetterPropGrid[i, Gridsize - 1].LetterDCount = fallCount;

            RemoveTile(toRemove);
        }

        public static void RemoveTile(LetterProp toRemove)
        {
            toRemove.LetTF.Translate(0.0f, 0.0f, -1f);

            if (toRemove.MusicHolderRole && !IsGameOver)
            {
                NewMusicTile();
            }

            Rigidbody rb = toRemove.rigidbody; // toRemove.LetterBlockObj.GetComponent(typeof(Rigidbody)) as Rigidbody;
            rb.useGravity = true;
            rb.isKinematic = false;

            float xf = (WSGameState.Rnd.Next(100) - 50f) / 150f;
            float yf = (WSGameState.Rnd.Next(10) - 5f) / 1f;
            float zf = (WSGameState.Rnd.Next(100) / 30f);
            rb.AddForce(new Vector3(xf, yf, -zf), ForceMode.VelocityChange);

            float xr = (WSGameState.Rnd.Next(200) - 100f); // / 10f;
            float yr = WSGameState.Rnd.Next(100) / 10f;
            float zr = WSGameState.Rnd.Next(100) / 1f;
            rb.AddTorque(new Vector3(xr, yr, zr), ForceMode.VelocityChange);
        }

        #endregion Main

        #region StatUpdate

        public static FortuneLevel GetFortune()
        {
            if (GetLatestEff() >= EffHigh)
            {
                return FortuneLevel.Great;
            }
            else if (GetLatestEff() >= EffMed)
            {
                return FortuneLevel.Good;
            }

            return FortuneLevel.Bad;
        }

        private static void UpdateManaScore()
        {
            boardScript.SetMana(gs.mana.ToString());
        }

        internal static void UpdateFortune()
        {
            float eff = (float)GetLatestEff();

            // Scale goes from 3, the smallest score to EffHigh (which is the max efficiency) +20.
            float scale = (eff - LowestWordScore) / (EffHigh + FortuneMaxOver);

            // No bigger than 1, but zero makes it invisible, so start at .5
            if(scale > 1.0f)
            {
                scale = 1.0f;
            }
            if(scale <= 0.1f)
            {
                scale = 1f / (EffHigh + FortuneMaxOver - LowestWordScore);
            }
            Material fc = GetFortuneColor();
            boardScript.SetFortune(scale, fc);
        }

        private static void AddToTryList()
        {
            WordScoreItem wsi = new WordScoreItem() { Word = GetCurrentWord(), Score = ScoreWord(), Wordscorestring = EngLetterScoring.GetWordTally(SelLetterList), Simplescore = ScoreWordSimple() };

            if (TryWordList.FindIndex(f => (f.Word == wsi.Word)) >= 0)
            {
                return;
            }

            int indx = TryWordList.FindIndex(f => (f.Score < wsi.Score));
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
                boardScript.AddTryList(wsi_I.Word + " " + wsi_I.Score.ToString());
            }
        }

        private static void UpdateStats()
        {
            boardScript.SetScore(gs.score.ToString());

            if (totalwords > 0)
            {
                TotalEfficiency = gs.score / totalwords;
            }
            else
            {
                TotalEfficiency = 0;
            }

            boardScript.SetLevel(gs.level.ToString());

            UpdateManaScore();
            UpdateFortune();
            UpdateEff();
        }

        private static void UpdateEff()
        {
            boardScript.SetEff((float)gs.score / (float)gs.history.Count);
        }

        public static void ChangeManna(int manna)
        {
            gs.mana += manna;
            UpdateManaScore();
        }

        #endregion StatUpdate

        internal static string GetWordTally()
        {
            return EngLetterScoring.GetWordTally(SelLetterList);
        }

        internal static void TurnOver()
        {
            boardScript.ClearTryList();
            TryWordList.Clear();
        }

        public static void GameOver()
        {
            IsGameOver = true;
            Deselect(null);

            RecoreGameScore(gs.score);

            GamePersistence.SaveOverallStats(os);
            GamePersistence.ResetGameData();

            RemoveGameBoard();

            Resume = false;
            boardScript.EndGameAction();
        }

        internal static bool EnoughMana(int mannaPoints)
        {
            return (mannaPoints > WSGameState.gs.mana);
        }

        private static void RemoveGameBoard()
        {
            for(int i = 0; i < Gridsize; i++)
            {
                for(int j = 0; j < Gridsize; j++)
                {
                    //LetterPropGrid[i, j].AnimationEnabled = false;
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

        internal static ScoreStats RecordWordScore()
        {
            ScoreStats ss = new ScoreStats();

            int wordTotal = ScoreWord();

            boardScript.AddHistory(wordTotal + " " + GetCurrentWord() + " " + GetWordTally());

            gs.score += wordTotal;

            WordScoreItem wsi = new WordScoreItem() { Word = GetCurrentWord(), Score = wordTotal, Wordscorestring = EngLetterScoring.GetWordTally(SelLetterList), Simplescore = ScoreWordSimple() };

            ss.bonus = EngLetterScoring.LengthBonus(wsi.Word);

            gs.fortune.Add(wsi);
            if (gs.fortune.Count > EFF_WORD_COUNT)
            {
                gs.fortune.RemoveAt(0);
            }

            Logging.PlayDbg("rws4");
            gs.history.Add(wsi);
            Logging.PlayDbg("rws4.1");

            CheckTopBestWordScores(wsi);
            Logging.PlayDbg("rws4.2");
            CheckTopBestWordScoresSimple(wsi);
            Logging.PlayDbg("rws4.3");
            CheckTopLongestWordScores(wsi);
            Logging.PlayDbg("rws5");

            totalwords++;

            TotalEfficiency = gs.score / totalwords;

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
            Logging.PlayDbg("rws6");

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
            Logging.PlayDbg("rwsX");

            return ss;
        }

        internal static void AwardAllSpells()
        {
            foreach (SpellInfo si in Spells.AllSpells)
            {
                AwardedSpells.Add(si);
            }

            boardScript.ShowSpellStuff();
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

        private static void CheckTopLongestWordScores(WordScoreItem wsi)
        {
            Logging.PlayDbg("ctl0");
            if (os.LongestWords.FindIndex(f => (f.Word == wsi.Word)) >= 0)
            {
                return;
            }
            Logging.PlayDbg("ctl1("+wsi.Word.Length.ToString()+" "+os.LongestWords.Count.ToString()+")", '\n');
            foreach(WordScoreItem w in os.LongestWords)
            {
                Logging.PlayDbg("clt1.1(" + w.Word + "_" + w.Score + ")", '\n');
            }

            int indx = os.LongestWords.FindIndex(f => (f.Word.Length < wsi.Word.Length));
            Logging.PlayDbg("ctl2");
            if (indx >= 0)
            {
                os.LongestWords.Insert(indx, wsi);
            }
            else
            {
                os.LongestWords.Add(wsi);
            }

            Logging.PlayDbg("ctl3");

            if (os.LongestWords.Count > TOP_SCORES_MAX)
            {
                os.LongestWords.RemoveAt(TOP_SCORES_MAX);
            }

            Logging.PlayDbg("ctl4");

        }

        private static void CheckTopBestWordScoresSimple(WordScoreItem wsi)
        {
            if (os.BestWordScoresSimple.FindIndex(f => (f.Word == wsi.Word)) >= 0)
            {
                return;
            }

            int indx = os.BestWordScoresSimple.FindIndex(f => (f.Score < wsi.Simplescore));
            if (indx >= 0)
            {
                os.BestWordScoresSimple.Insert(indx, wsi);
            }
            else
            {
                os.BestWordScoresSimple.Add(wsi);
            }
            if (os.BestWordScoresSimple.Count > TOP_SCORES_MAX)
            {
                os.BestWordScoresSimple.RemoveAt(TOP_SCORES_MAX);
            }
        }

        private static void CheckTopBestWordScores(WordScoreItem wsi)
        {
            int idx = os.BestWordScores.FindIndex(f => (f.Word == wsi.Word));
            if (idx >= 0)
            {
                if(os.BestWordScores[idx].Simplescore <= wsi.Simplescore)
                {
                    return;
                }
            }

            int indx = os.BestWordScores.FindIndex(f => (f.Score < wsi.Simplescore));
            if (indx >= 0)
            {
                os.BestWordScores.Insert(indx, wsi);
            }
            else
            {
                os.BestWordScores.Add(wsi);
            }
            if (os.BestWordScores.Count > TOP_SCORES_MAX)
            {
                os.BestWordScores.RemoveAt(TOP_SCORES_MAX);
            }
        }

        private static int ScoreWordSimple()
        {
            return EngLetterScoring.ScoreWordSimple(SelLetterList);
        }

        private static double GetLatestEff()
        {
            if (gs.fortune.Count <= 0)
            {
                return 0;
            }

            int wordtotal = 0;
            foreach (WordScoreItem wsi in gs.fortune)
            {
                wordtotal += wsi.Score;
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

        public static void MagicSelect(LetterProp lp_sel)
        {
            selMagicTile = boardScript.SelectLet(lp_sel.I, lp_sel.J, true);
        }

        public static void MagicDeselect()
        {
            if(selMagicTile != null)
            {
                boardScript.DeselectLet(selMagicTile);
            }
            selMagicTile = null;                
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
            List<LetterProp> removeList = new List<LetterProp>() ;

            for (int i = 0; i < Gridsize; i++)
            {
                for (int j = 0; j < Gridsize; j++)
                {
                    LetterProp curlp = LetterPropGrid[i, j];
                    if (curlp.IsBurning())
                    {
                        if (curlp.J <= 0)
                        {
                            // GameOver
                            return true;
                        }

                        if(LetterPropGrid[i, j -1].TileType != LetterProp.TileTypes.Burning)
                        {
                            boardScript.PlayLavaSound();

                            removeList.Add(LetterPropGrid[i, j - 1]);
                        }
//                        RemoveAndReplaceTile(curlp.I, curlp.J - 1);
                    }
                }
            }

            foreach(LetterProp lp in removeList)
            {
                RemoveAndReplaceTile(lp.I, lp.J);
            }

            WordWarAI wwai = new WordWarAI(LetterPropGrid);
            return (!wwai.AnyWords());
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

        public static string PrintGameBoard()
        {
            string ret = "";

            // Activate any tile specific work, like music.
            for (int j = Gridsize - 1; j >= 0; j--)
            {
                for (int i = 0; i < Gridsize; i++)
                {
                    ret += LetterPropGrid[i, j].ASCIIChar + " ";
                }

                ret += "\n";
            }

            return ret;
        }
    }
}

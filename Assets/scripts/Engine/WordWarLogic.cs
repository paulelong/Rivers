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
            None,
        }

        public enum WordValidity
        {
            Garbage,
            Word,
            UsedWord,
        }

        public enum GameEndReasons
        {
            NOT_OVER,
            USER_ENDED,
            BURNING_TILE,
            NO_WORDS,
        }

        #endregion Constants


        #region Privates
        public struct ScoreStats
        {
            public int MannaScore;
            public SpellInfo si;
            public int bonus;
        }

        private static List<GameObject> selMagicTileList = new List<GameObject>();
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
        static Material NoWordMaterial;

        private static int FortuneLevelCount;

        private static bool resume = false;
        private static bool gameOver = false;

        // simple stats
        private static int spellsCasted;
        private static int spellsAwarded;

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

        public static bool SnipeGiven
        {
            get { return gs.SnipeGiven;  }
            set { gs.SnipeGiven = value; }
        }

        public static int CheckMusicState
        {
            get { return gs.CheckMusicState; }
            set { gs.CheckMusicState = value; }
        }

        public static float MusicWaitTime
        {
            get { return gs.MusicWaitTime; }
            set { gs.MusicWaitTime = value;  }
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
                        if(wsi.Word != null)
                        {
                            longestWordStrings.Add(wsi.Word + " " + wsi.Word.Length.ToString());
                        }
                        else
                        {
                            Logging.StartDbg("StatXMLCorrupt!!!");
                            GamePersistence.ResetHistoryData();
                            return longestWordStrings;
                        }
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

        public static int NumAttempted
        {
            get { return gs.spellsAttempted; }
            set { gs.spellsAttempted = value; }
        }

        public static int NumCasted
        {
            get { return gs.spellsCasted; }
            set { gs.spellsCasted = value; }
        }

        public static int NumAborted
        {
            get { return gs.spellsAborted; }
            set { gs.spellsAborted = value; }
        }

        #endregion Properties

        #region Fields
        static public Board boardScript;

        static public LetterProp[,] LetterPropGrid = null;

        static float scaleSize = 0;

        static int gridsize = 7;
        static float halfOffset = 0;
        #endregion Fields


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

        #endregion Persistence

        public static void InitGameGlobal()
        {
            GameObject go = GameObject.Find("BoardBackground");
            boardScript = (Board)go.GetComponent(typeof(Board));

            LetterProp.InitLetterPropertyList(boardScript);

            BadFortuneMaterial = Resources.Load("Copper") as Material;
            GoodFortuneMaterial = Resources.Load("Silver") as Material;
            GreatFortuneMaterial = Resources.Load("Gold") as Material;
            NoWordMaterial = Resources.Load("NormalSelect") as Material;
            ManaMaterial = Resources.Load("Mana") as Material;

            // Load the music for the speaker tiles just once.
//            Songs.LoadMusic();

            //boardScript.ShowMsg("Loading dictionary...");
            //EngLetterScoring.LoadDictionary();
           // boardScript.HideMsg();
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

            WSAnalytics.RecordAnalyticsStartGame(gs, glbseed);
        }

        internal static void Replay()
        {
            gs.score = 0;
            gs.level = 1;
            gs.mana = 0;

            gs.awarded.Clear();
            gs.history.Clear();

            TotalEfficiency = 0;
            totalwords = 0;

            AwardedSpells.Clear();
            Spells.AvailableSpells.Clear();

            Logging.StartDbg("r1");
            UpdateStats();
        }

        public static void NewLetter(int i, int j, float fallcount = 0.0f)
        {
            LetterPropGrid[i, j] = new LetterProp(gs.level, levelup, i, j, fallcount);

            if (levelup == true)
            {
                levelup = false;
            }

        }

        internal static void NewMusicTile()
        {
            boardScript.ResetCheckMusic();

            int ti;
            ti = WSGameState.Rnd.Next(Gridsize - 1);

            LetterPropGrid[ti, Gridsize - 1].PlayBackgroundMusic();
            Logging.PlayDbg("mt(" + LetterPropGrid[ti, Gridsize - 1].ASCIIString + "," + ti + ")");
        }

        internal static void SetPanelSize(GameObject contrastPanel)
        {
            RectTransform r = contrastPanel.transform.GetComponent<RectTransform>();

            Vector3 minp = LetterPropGrid[0, 0].LetterBlockObj.transform.position;
            Vector3 maxp = LetterPropGrid[gridsize - 1, gridsize - 1].LetterBlockObj.transform.position;

            Debug.Log("0,0 = " + minp);
            Debug.Log("x,x = " + maxp);

            Bounds b = LetterPropGrid[0, 0].LetterBlockObj.GetComponent<Renderer>().bounds;

            Vector3 p1 = Camera.main.WorldToScreenPoint(minp - new Vector3(b.extents.x + .1f, b.extents.y + .1f, b.extents.z));
            Vector3 p2 = Camera.main.WorldToScreenPoint(maxp + new Vector3(b.extents.x + .1f, b.extents.y + .1f, b.extents.z));

            Debug.Log("Set1 to " + p1.x + " " + p1.y);
            Debug.Log("Set2 to " + p2.x + " " + p2.y);

            SetSize(r, p1, p2);
        }

        internal static void SetSize(RectTransform trans, Vector3 ul, Vector3 lr)
        {
            Vector3 d = lr - ul;

            Vector2 min = new Vector2(ul.x / (float)Screen.width, ul.y / (float)Screen.height);
            Vector2 max = new Vector2(lr.x / (float)Screen.width, lr.y / (float)Screen.height);

            Debug.Log("min = " + min);
            Debug.Log("max = " + max);

            trans.anchorMin = min;
            trans.anchorMax = max;
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
                            SelLetterList.Add(lp);
                            lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                        }
                        else
                        {
                            SelLetterList.Add(lp);
                        }
                    }
                    else
                    {
                        SelLetterList.Add(lp);
                        lp.SelectorObject = boardScript.SelectLet(lp.I, lp.J);
                    }

                    //Deselect(null);
                    foreach (LetterProp lpi in SelLetterList)
                    {
                        boardScript.SelectLet(lpi);
                    }

                    //SelLetterList.Add(lp);

                    // add if for > 3 letters

                    // if it's a word, update color to green, unless we used it before
                    if (SelLetterList.Count > 2 && EngLetterScoring.IsWord(GetCurrentWord()))
                    {
                        if (gs.history.FindIndex(f => (f.Word == GetCurrentWord())) >= 0)
                        {
                            boardScript.SetCurrentWord(GetCurrentWord() + " = " + ScoreWord() + LocalizationManager.instance.GetLocalizedValue(Board.SubmitButtonAlreadyUsedKey) + GetWordTally());
                            boardScript.IndicateGoodWord(WSGameState.WordValidity.UsedWord);
                        }
                        else
                        {
                            boardScript.SetCurrentWord(GetCurrentWord() + " = " + ScoreWord() + LocalizationManager.instance.GetLocalizedValue(Board.SubmitButtonSumbitKey) + GetWordTally());
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
                    boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("UsedAlready"));
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
                    GameEndReasons ger = ProcessLetters();
                    Logging.PlayDbg("Sub2");

                    Deselect(null);

                    boardScript.ResetSubmitButton();

                    if (ger != GameEndReasons.NOT_OVER)
                    {
                        GameOver(ger);
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
                            boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("NiceWord") + ss.si.FriendlyName + LocalizationManager.instance.GetLocalizedValue("Spell"));
                            boardScript.RefreshSpells();
                        }

                        TurnOver();
                        boardScript.ScoreWordSound();

                        if (GainedNextLevel)
                        {
                            WSAnalytics.RecordAnalyticsLevelReached(gs);

                            boardScript.LevelSound();
                            string levelmsg = LocalizationManager.instance.GetLocalizedValue("NewLevel") + CurrentLevel.ToString() + "\n\n";
                            if (Spells.HasSpells())
                            {
                                levelmsg += LocalizationManager.instance.GetLocalizedValue("NewSpell");
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
                        boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("NoWordSpelledMsg"));
                    }
                    else
                    {
                        boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("LessThanThreeMsg"));
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
        #endregion Main

        internal static Material GetMagicMat()
        {
            return ManaMaterial;
        }

        #region LetterTileManagment
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

        #endregion

        #region StatUpdate

        public static FortuneLevel GetFortune()
        {
            return GetFortuneLevel(GetLatestEff());
        }

        public static FortuneLevel GetFortuneLevel(float value)
        {
            if (value >= EffHigh)
            {
                return FortuneLevel.Great;
            }
            else if (value >= EffMed)
            {
                return FortuneLevel.Good;
            }
            else if (value == 0)
            {
                return FortuneLevel.None;
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

            if(eff <= 0f)
            {
                eff = .1f;
            }

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

        public static void GameOver(GameEndReasons ger)
        {
            // Send anylitics on how the game went
            WSAnalytics.RecordAnalyticsGameOver(gs);

            boardScript.ContrastPanel.SetActive(false);

            IsGameOver = true;
            Deselect(null);

            RecoreGameScore(gs.score);

            GamePersistence.SaveOverallStats(os);
            GamePersistence.ResetGameData();

            RemoveGameBoard();

            Resume = false;

            int index = (int)ger - 1;


            int wordlength = 0;
            int bestscore = 0;
            string longestword = "";
            WordScoreItem bestword = null;

            foreach(WordScoreItem wsi in gs.history)
            {
                if(wsi.Word.Length > wordlength)
                {
                    wordlength = wsi.Word.Length;
                    longestword = wsi.Word;
                }

                if (wsi.Score > bestscore)
                {
                    bestscore = wsi.Score;
                    bestword = wsi;
                }
            }

            string bestWordResponse = LocalizationManager.instance.GetLocalizedValue("BestWordIs") + " " + bestword.Word + "=" + bestword.Score.ToString() + ". ";
            bestWordResponse += LocalizationManager.instance.GetLocalizedValue("LongestWordIs") + " " + longestword + ".\n";

            boardScript.EndGameAction(bestWordResponse + LocalizationManager.instance.GetLocalizedValuesByindex("EndGameReasons", index));
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

        public static int ScoreWord()
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

        public static void AwardSpell(string spellname)
        {
            SpellInfo si = Spells.FindSpell(spellname);
            AwardedSpells.Add(si);
        }

        internal static void AwardAllSpells()
        {
            foreach (SpellInfo si in Spells.AllSpells)
            {
                //if(si.FriendlyName.Contains("Swap"))
                //{
                    AwardedSpells.Add(si);
                //}
            }

            boardScript.ShowSpellStuff();
        }

        public static void ChangeSong()
        {
            GameObject go = null;

            for (int i = 0; i < Gridsize; i++)
            {
                for (int j = 0; j < Gridsize; j++)
                {
                    if (LetterPropGrid[i, j].TileType == LetterProp.TileTypes.Speaker)
                    {
                        go = LetterPropGrid[i, j].LetterBlockObj;
                    }
                }
            }

            if(go != null)
            {
                AudioSource asrc = (AudioSource)go.transform.GetComponent(typeof(AudioSource));
                if (asrc != null)
                {
                    asrc.clip = Songs.GetNextSong();
                    asrc.PlayDelayed(0);
                }
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

        private static float GetLatestEff()
        {
            if (gs.fortune.Count <= 0)
            {
                return 0.1f;
            }

            int wordtotal = 0;
            foreach (WordScoreItem wsi in gs.fortune)
            {
                wordtotal += wsi.Score;
            }

            return (float)wordtotal / (float)gs.fortune.Count;
        }

        internal static Material GetFortuneColor(int value = -1)
        {
            FortuneLevel comparevalue = 0;

            if (value == -1)
            {
                comparevalue = GetFortune();
            }
            else
            {
                comparevalue = GetFortuneLevel(value);
            }


            switch (comparevalue)
            {
                case FortuneLevel.Bad:
                    return (BadFortuneMaterial);
                case FortuneLevel.Good:
                    return (GoodFortuneMaterial);
                case FortuneLevel.Great:
                    return (GreatFortuneMaterial);
                case FortuneLevel.None:
                    return (NoWordMaterial);
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
            selMagicTileList.Add(boardScript.SelectLet(lp_sel.I, lp_sel.J, true));
        }

        public static void MagicDeselect()
        {
            foreach(GameObject go in selMagicTileList)
            {
                boardScript.DeselectLet(go);
            }

            selMagicTileList.Clear();
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

        internal static GameEndReasons ProcessLetters()
        {
            List<LetterProp> removeList = new List<LetterProp>() ;

            int vowelcount = 0;

            for (int i = 0; i < Gridsize; i++)
            {
                for (int j = 0; j < Gridsize; j++)
                {
                    LetterProp curlp = LetterPropGrid[i, j];

                    if(!EngLetterScoring.IsConsonant(LetterPropGrid[i,j].ASCIIString))
                    {
                        vowelcount++;
                    }

                    if (curlp.IsBurning())
                    {
                        if (curlp.J <= 0)
                        {
                            // GameOver
                            return GameEndReasons.BURNING_TILE;
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

            if(vowelcount <= 2 && gs.level <= 3)
            {
                boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("NeedVowels"));
                AwardSpell("VowelDust");
            }

            foreach(LetterProp lp in removeList)
            {
                RemoveAndReplaceTile(lp.I, lp.J);
            }

            WordWarAI wwai = new WordWarAI(LetterPropGrid);
            if(!wwai.AnyWords() && gs.awarded.Count <= 0)
            {
                return GameEndReasons.NO_WORDS;
            }

            return GameEndReasons.NOT_OVER;

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

        internal static void DebugMode()
        {
            gs.mana = 100;
            gs.level = 20;
        }


    }
}

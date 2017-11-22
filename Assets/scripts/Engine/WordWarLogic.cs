using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace WordSpell
{
    partial class WSGameState
    {
        [DataContractAttribute]
        public class WordScoreItem
        {
            [DataMember]
            public string word { get; set; }
            [DataMember]
            public string wordscorestring { get; set; }
            [DataMember]
            public int score { get; set; }
            [DataMember]
            public int simplescore { get; set; }
        }

        static List<WordScoreItem> FortuneWordScoreHistory = new List<WordScoreItem>();
        static List<WordScoreItem> BestWordScores = new List<WordScoreItem>();
        static List<WordScoreItem> BestWordScoresSimple = new List<WordScoreItem>();
        static List<WordScoreItem> LongestWords = new List<WordScoreItem>();
        static List<WordScoreItem> AllWords = new List<WordScoreItem>();
        static List<WordScoreItem> HistoryWords = new List<WordScoreItem>();
        static List<int> BestGameScores = new List<int>();

        static List<WordScoreItem> TryWordList = new List<WordScoreItem>();

        const string BestWordScoreFileName = "BestWordScores.txt";
        const string BestWordScoresSimpleFileName = "BestWordScoresSimple.txt";
        const string BestLongestWordsFileName = "BestLongestWords.txt";
        const string BestOverallScoresFileName = "BestOverallScores.txt";
        const string AllWordScoresFileName = "AllWordScores.txt";
        const string FortuneScoresFileName = "FortuneScores.txt";
        const string HistoryScoresFileName = "HistoryScores.txt";

        static private Board boardScript;

        static private LetterProp[,] LetterPropGrid = null;

        static List<LetterProp> SelLetterList = new List<LetterProp>();

        public const int gridsize = 9;

        private static int level = 1;

        static System.Random r;

        //public static TextBlock CurrentWord { get; private set; }
        public static int CurrentLevel { get { return level; } private set { } }

        public static double TotalEfficiency { get; internal set; }

        public static int Manna = 0;

        private static SpellInfo NextSpell = null;

        //private static int[] Levels = { 0, 25, 60, 100, 160, 230, 310, 400, 500, 650, 850, 1000, 1300, 1600, 2000, 2500, 3000, 4600, 5200, 10000, 20000, 30000  };
        private static int[] Levels = { 0, 20, 40, 60, 80, 100, 120, 140, 160, 180, 200, 220, 1300, 1600, 2000, 5000, 10000 };
        private static bool levelup = false;

        public static int totalScore;
        public static int HighScoreWordValue = 0;
        public static string HighScoreWord;
        public static string HighScoreWordTally;
        public static int totalwords = 0;
        public static double Efficiency;
        private const int EffWordCount = 3;
        private const int NumberOfTopScores = 20;

        //private static TextBlock LevelText;
        //private static TextBlock ScoreText;
        //private static TextBlock WordScoreText;
        //private static TextBlock MannaScoreText;
        //private static TextBlock EffText;
        //private static ListBox HistoryList;
        //public static Popup LetterTipPopup;
        //private static TextBlock PopupText;
        private static int FortuneLevelCount;

        internal static void DebugMode()
        {
            Manna = 100;
            level = 20;
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

        public static void InitGameGlobal()
        {
            GameObject go = GameObject.Find("BoardBackground");
            boardScript = (Board)go.GetComponent(typeof(Board));

            EngLetterScoring.LoadDictionary();
            LetterProp.InitLetterPropertyList(boardScript);

        }

        public static void InitNewGame()
        {

            // Reset all variables
            Replay();

            // Stuff useful for development, don't ship
            Spells.AwardAllSpells();
            r = new System.Random(20);

            LetterProp.InitProbability(level);

            LetterPropGrid = new LetterProp[gridsize, gridsize];

            Spells.UpdateSpellsForLevel(level);
        }



        //public async static Task InitLogic()
        //{
        //    await LoadStats();
        //}

        //public async static void InitializeLetterButtonGrid(Grid _LetterGrid, TextBlock _CurrentWord, ListBox _TryList, TextBlock _MannaScoreText, TextBlock _effText, TextBlock _levelText, TextBlock _scoreText, TextBlock _wordText, ListBox _historyList, Popup _flyout, TextBlock _popuptext, double h)
        //{
        //    LetterGrid = _LetterGrid;
        //    CurrentWord = _CurrentWord;
        //    TryList = _TryList;
        //    MannaScoreText = _MannaScoreText;
        //    EffText = _effText;
        //    LevelText = _levelText;
        //    ScoreText = _scoreText;
        //    WordScoreText = _wordText;
        //    HistoryList = _historyList;
        //    LetterTipPopup = _flyout;
        //    PopupText = _popuptext;

        //    double aw = (int)LetterGrid.ActualWidth;
        //    double ah = (int)LetterGrid.ActualHeight;

        //    double bs = Math.Min(aw, h);

        //    LetterGrid.Width = bs;
        //    LetterGrid.Height = bs;

        //    ButtonWidth = (int)(bs / gridsize);
        //    ButtonHeight = (int)(bs / gridsize);

        //    // create buttons
        //    LetterPropGrid = new LetterProp[gridsize, gridsize];

        //    RowDefinition[] rows = new RowDefinition[gridsize];
        //    ColumnDefinition[] columns = new ColumnDefinition[gridsize];

        //    for (int i = 0; i < columns.Length; i++)
        //    {
        //        columns[i] = new ColumnDefinition();
        //        LetterGrid.ColumnDefinitions.Add(columns[i]);
        //    }

        //    for (int i = 0; i < rows.Length; i++)
        //    {
        //        rows[i] = new RowDefinition();
        //        LetterGrid.RowDefinitions.Add(rows[i]);
        //    }

        //    LetterProp.InitProbability(level);

        //    if(WordWarLogic.IsSavedGame())
        //    {
        //        bool gameloaded = await LoadGame();
        //        if(!gameloaded)
        //        {
        //            NewLetterButtonGrid();
        //        }
        //    }
        //    else
        //    {
        //        WordWarLogic.SetSavedGame();
        //        NewLetterButtonGrid();
        //    }

        //    UpdateManaScore();
        //    UpdateStats();
        //}


        private static void UpdateManaScore()
        {
            boardScript.SetMana(Manna.ToString());
            //MannaScoreText.Text = "M: " + Manna.ToString();
        }

        //internal static SolidColorBrush GetFortuneColor()
        //{
        //    switch (WordWarLogic.GetFortune())
        //    {
        //        case WordWarLogic.FortuneLevel.Bad:
        //            return (BadFortune);
        //        case WordWarLogic.FortuneLevel.Good:
        //            return (GoodFortune);
        //        case WordWarLogic.FortuneLevel.Great:
        //            return (GreatFortune);
        //    }

        //    return BadFortune;
        //}

        //internal static SolidColorBrush GetFortuneColor(int score)
        //{
        //    switch (WordWarLogic.GetFortune(score))
        //    {
        //        case WordWarLogic.FortuneLevel.Bad:
        //            return (BadFortune);
        //        case WordWarLogic.FortuneLevel.Good:
        //            return (GoodFortune);
        //        case WordWarLogic.FortuneLevel.Great:
        //            return (GreatFortune);
        //    }

        //    return BadFortune;
        //}

        internal static void UpdateFortune()
        {
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
            LetterPropGrid[i, j] = new LetterProp(level, levelup, i, j, tf);

            if (levelup == true)
            {
                levelup = false;
            }
        } 

        public static void LetterClick(int i, int j)
        {
            LetterProp lp = LetterPropGrid[i, j];

            //LetterTipPopup.IsOpen = false;

            if (NextSpell != null)
            {
                SpellInfo.SpellOut so = CastSpell(NextSpell, lp);

                if (so.si == null && so.worked)
                {
                    if (freeSpell)
                    {
                        Spells.RemoveAwardedSpells(NextSpell);
                    }
                    else
                    {
                        ChangeManna(-NextSpell.MannaPoints);
                    }
                }
                NextSpell = so.si;
            }
            else
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
                //TryList.Items.Add(wsi_I.word + " " + wsi_I.score.ToString());
            }
        }


        internal static void TurnOver()
        {
            boardScript.ClearTryList();
            //TryList.Items.Clear();
            TryWordList.Clear();
        }

        internal static void Replay()
        {
            totalScore = 0;
            level = 1;
            TotalEfficiency = 0;
            Efficiency = 0;
            HighScoreWordValue = 0;
            HighScoreWord = "";
            totalwords = 0;
            Manna = 0;

            //ButtonList.Clear();

            //LetterGrid.Children.Clear();

            //NewLetterButtonGrid();
        }

        internal static void SubmitWord()
        {
            string s = GetCurrentWord().ToLower();

            if (EngLetterScoring.IsWord(s))
            {

                ScoreStats ss = RecordWordScore();
                RemoveWordAndReplaceTiles();

                Deselect(null);
                boardScript.ResetSubmitButton();

                bool gameOver = ProcessLetters();
                if (gameOver)
                {
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

                    if (CheckNextLevel(totalScore))
                    {
                        boardScript.LevelSound();
                        string levelmsg = "Welcom to Level " + CurrentLevel.ToString() + "\n\n";
                        if (Spells.HasSpells())
                        {
                            levelmsg += "You have new spells";
                        }

                        boardScript.ShowMsg(levelmsg);
                    }


                    boardScript.ScoreWordSound();
                }
            }
            else
            {
                Deselect(null);

                //CurrentWord.Text = "Not a known word.  Try again";
            }

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
        }

        internal static bool CheckNextLevel(int totalScore)
        {
            if (level < Levels.Length - 1 && totalScore >= Levels[level])
            {
                level++;
                levelup = true;

                //LevelText.Text = "L: " + CurrentLevel.ToString();

                if (level >= 5)
                {
                    ChangeManna(6);
                }

                Spells.UpdateSpellsForLevel(level);
                LetterProp.InitProbability(level);
            }

            return levelup;
        }

        internal static int ScoreWord()
        {
            return EngLetterScoring.ScoreWord(GetCurrentWord());
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

            //HistoryList.Items.Insert(0, GetWordTally());
            boardScript.AddHistory(GetCurrentWord() + " " + GetWordTally());

            totalScore += wordTotal;
            if (wordTotal > HighScoreWordValue)
            {
                HighScoreWordValue = wordTotal;
                HighScoreWord = GetCurrentWord();
                HighScoreWordTally = EngLetterScoring.GetWordTally(SelLetterList);
            }

            WordScoreItem wsi = new WordScoreItem() { word = GetCurrentWord(), score = wordTotal, wordscorestring = EngLetterScoring.GetWordTally(SelLetterList), simplescore = ScoreWordSimple() };

            ss.bonus = EngLetterScoring.LengthBonus(wsi.word);

            FortuneWordScoreHistory.Add(wsi);
            if (FortuneWordScoreHistory.Count > EffWordCount)
            {
                FortuneWordScoreHistory.RemoveAt(0);
            }

            HistoryWords.Add(wsi);

            CheckTopBestWordScores(wsi);
            CheckTopBestWordScoresSimple(wsi);
            CheckTopLongestWordScores(wsi);

            totalwords++;

            TotalEfficiency = totalScore / totalwords;
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
                Manna += (FortuneLevelCount - 4);
            }
            ss.MannaScore = ScoreManna();

            // If it's a big or price word, give them a spell based on the word.
            string curword = GetCurrentWord();
            if (wordTotal > 14 || curword.Length >= 8)
            {
                SpellInfo si = null;

                if (wordTotal > 70 || curword.Length > 16)
                {
                    si = Spells.GetSpell(6, level);
                }
                else if (wordTotal > 55 || curword.Length > 15)
                {
                    si = Spells.GetSpell(5, level);
                }
                else if (wordTotal > 45 || curword.Length > 14)
                {
                    si = Spells.GetSpell(4, level);
                }
                else if (wordTotal > 35 || curword.Length > 13)
                {
                    si = Spells.GetSpell(3, level);
                }
                else if (wordTotal > 25 || curword.Length > 11)
                {
                    si = Spells.GetSpell(2, level);
                }
                else if (wordTotal > 17 || curword.Length > 9)
                {
                    si = Spells.GetSpell(1, level);
                }
                else if (wordTotal > 14 || curword.Length >= 8)
                {
                    si = Spells.GetSpell(0, level);
                }

                ss.si = si;
            }

            UpdateFortune();

            UpdateStats();

            return ss;
        }

        private static void UpdateStats()
        {
            //WordScoreText.Text = "Best: " + HighScoreWordTally;

            boardScript.SetScore(totalScore.ToString());

            if (totalwords > 0)
            {
                TotalEfficiency = totalScore / totalwords;
            }
            //EffText.Text = "E: " + Efficiency.ToString("#.#") + "(" + TotalEfficiency.ToString("#.##") + ")";

            boardScript.SetLevel(level.ToString());

            boardScript.SetMana(Manna.ToString());
        }

        private static void CheckTopLongestWordScores(WordScoreItem wsi)
        {
            int indx = LongestWords.FindIndex(f => (f.word.Length < wsi.word.Length));
            if (indx >= 0)
            {
                LongestWords.Insert(indx, wsi);
            }
            else
            {
                LongestWords.Add(wsi);
            }
            if (LongestWords.Count > NumberOfTopScores)
            {
                LongestWords.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScoresSimple(WordScoreItem wsi)
        {
            int indx = BestWordScoresSimple.FindIndex(f => (f.score < wsi.simplescore));
            if (indx >= 0)
            {
                BestWordScoresSimple.Insert(indx, wsi);
            }
            else
            {
                BestWordScoresSimple.Add(wsi);
            }
            if (BestWordScoresSimple.Count > NumberOfTopScores)
            {
                BestWordScoresSimple.RemoveAt(NumberOfTopScores);
            }
        }

        private static void CheckTopBestWordScores(WordScoreItem wsi)
        {
            int indx = BestWordScores.FindIndex(f => (f.score < wsi.score));
            if (indx >= 0)
            {
                BestWordScores.Insert(indx, wsi);
            }
            else
            {
                BestWordScores.Add(wsi);
            }
            if (BestWordScores.Count > NumberOfTopScores)
            {
                BestWordScores.RemoveAt(NumberOfTopScores);
            }
        }

        private static int ScoreWordSimple()
        {
            return EngLetterScoring.ScoreWordSimple(SelLetterList);
        }

        private static double GetLatestEff()
        {
            int wordtotal = 0;
            foreach (WordScoreItem wsi in FortuneWordScoreHistory)
            {
                wordtotal += wsi.score;
            }

            return (double)wordtotal / (double)FortuneWordScoreHistory.Count;
        }

        public static int ScoreManna()
        {
            int addedManna = EngLetterScoring.ScoreManna(SelLetterList);
            Manna += addedManna;
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
                Tile t = (Tile)LetterOntop.Tf.GetChild(0).gameObject.GetComponent(typeof(Tile));
                t.SetPos(i, jp);

                LetterOntop.J = jp;

                LetterPropGrid[i, jp] = LetterPropGrid[i, jp + 1];

                LetterOntop.LetterFallCount++;
            }

            float fallCount = LetterPropGrid[i, gridsize - 1].LetterFallCount;
            Transform lbi = boardScript.NewTile(i, gridsize - 1, fallCount);
            NewLetter(i, gridsize - 1, lbi);
            LetterPropGrid[i, gridsize - 1].LetterFallCount = fallCount;

            RemoveTile(toRemove);
        }


        public static void RemoveTile(LetterProp toRemove)
        {
            toRemove.Tf.Translate(0.0f, 0.0f, -1f);

            Rigidbody rb = toRemove.Tf.GetChild(0).GetComponent(typeof(Rigidbody)) as Rigidbody;
            rb.useGravity = true;
            rb.isKinematic = false;

            float xf = (r.Next(100) - 50) / 150;
            float yf = (r.Next(10) - 5) / 6;
            rb.AddForce(new Vector3(xf, yf, -2.0f), ForceMode.VelocityChange);

            float xr = r.Next(100) / 10f;
            float yr = r.Next(100) / 10f;
            float zr = r.Next(100) / 10f;
            rb.AddTorque(new Vector3(xr, yr, zr), ForceMode.VelocityChange);
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

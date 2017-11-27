using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordSpell
{
    public class SpellInfo
    {
        public struct SpellOut
        {
            public SpellInfo si;
            public bool worked;
        }

        public enum SpellType
        {
            DestroyLetter,
            DestroyGroup,
            WordHint,
            WordHint2,
            RandomVowels,
            Burn,
            LetterSwap,
            ChangeToVowel,
            ConvertLetter,
            RotateCW,
            RotateCCW,
            Rotate180,
            HintOnLetter,
            AnyLetter,
            RowBGone,
            ColumnBGone,
        };

        public SpellType spellType;
        public string FriendlyName;
        public int MannaPoints;
        public int SpellLevel;
        public bool Immediate;
        public string ImageName;
        private Sprite image = null;

        const string ImagePath = "Images/Spells/";
        public Sprite Image
        {
            get
            {
                if(image == null && ImageName != null)
                {
                    image = Resources.Load<Sprite>(ImagePath + ImageName);
                }

                return image;
            }
        }
    }

    class Spells
    {
        private const int SpellPerRow = 3;
        static System.Random r = new System.Random();

        static List<SpellInfo> AllSpells = new List<SpellInfo>
        {
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyLetter, FriendlyName = "Snipe",    MannaPoints = 10, SpellLevel = 13, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyGroup, FriendlyName = "Bomb",      MannaPoints = 8, SpellLevel = 8, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ChangeToVowel, FriendlyName = "Vowelize", MannaPoints = 7, SpellLevel = 10, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.WordHint, FriendlyName = "Hint",          MannaPoints = 10, SpellLevel = 11, Immediate = true }},
            { new SpellInfo { spellType = SpellInfo.SpellType.WordHint2, FriendlyName = "Hint++",       MannaPoints = 14, SpellLevel = 14, Immediate = true }},
            { new SpellInfo { spellType = SpellInfo.SpellType.Burn, FriendlyName = "Burn" ,             MannaPoints = 6, SpellLevel = 6, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.LetterSwap, FriendlyName = "Swap",        MannaPoints = 6, SpellLevel = 5, Immediate = false, ImageName = "Swap" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RandomVowels, FriendlyName = "Vowel Dust", MannaPoints = 10, SpellLevel = 15, Immediate = true }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ConvertLetter, FriendlyName = "Convert",  MannaPoints = 12, SpellLevel = 7, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateCW, FriendlyName = "Rotate CW",  MannaPoints = 3, SpellLevel = 9, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateCCW, FriendlyName = "Rotate CCW",  MannaPoints = 3, SpellLevel = 9, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.Rotate180, FriendlyName = "Rotate 180",  MannaPoints = 5, SpellLevel = 12, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.HintOnLetter, FriendlyName = "Letter Hint",  MannaPoints = 15, SpellLevel = 16, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.AnyLetter, FriendlyName = "Any Letter",  MannaPoints = 12, SpellLevel = 17, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RowBGone, FriendlyName = "Row b'Gone",  MannaPoints = 12, SpellLevel = 18, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ColumnBGone, FriendlyName = "Column b'Gone",  MannaPoints = 12, SpellLevel = 18, Immediate = false }},
        };

        internal static void RestFoundSpells()
        {
            AwardedSpells.Clear();
        }

        public static List<SpellInfo> AvailableSpells = new List<SpellInfo>();
        public static List<SpellInfo> AwardedSpells = new List<SpellInfo>();
        private static SpellInfo NextSpell;
        private static LetterProp LetterSwapFirst;
        private static int LetterSwapStep;
        private static bool awarded = false;
        private static int state = 0;
        private static LetterProp lp = null;
        private static List<LetterProp> RandomLetterList = new List<LetterProp>();

        #region SpellManagement

        internal static SpellInfo GetSpell(int v, int level)
        {
            // Generate a random spell weighting by v

            List<SpellInfo> inplay = new List<SpellInfo>();

            for (int i = v; i >= 0; i--)
            {
                foreach (SpellInfo si in AllSpells)
                {
                    if (si.SpellLevel <= (level + i + 5))
                    {
                        inplay.Add(si);
                    }
                }
            }

            int itemnumber = r.Next(inplay.Count);
            SpellInfo rsi = inplay[itemnumber];
            AwardedSpells.Add(rsi);

            return rsi;
        }

        public static void AwardAllSpells()
        {
            foreach (SpellInfo si in AllSpells)
            {
                AwardedSpells.Add(si);
            }
        }

        public static SpellInfo FindSpell(string name)
        {
            return( AllSpells.Find(x => (x.FriendlyName == name)) );
        }

        internal static void RemoveAwardedSpells(SpellInfo selectedSpell)
        {
            foreach(SpellInfo si in AwardedSpells)
            {
                if(si.spellType == selectedSpell.spellType)
                {
                    AwardedSpells.Remove(si);
                    break;
                }
            }
        }

        internal static void UpdateSpellsForLevel(int level)
        {
            AvailableSpells.Clear();
            foreach (SpellInfo si in AllSpells)
            {
                if (level >= si.SpellLevel)
                {
                    AvailableSpells.Add(si);
                }
            }
        }

        internal static bool HasSpells()
        {
            return (AvailableSpells.Count > 0);
        }

        #endregion SpellManagement

        #region SpellCasting

        public static void ReadySpell(string spellname, bool _awarded)
        {
            NextSpell = FindSpell(spellname);
            awarded = _awarded;

            if(NextSpell.Immediate)
            {
                CastSpell();
            }
        }

        public static bool EvalSpell(LetterProp _lp)
        {
            if(NextSpell != null)
            {
                lp = _lp;
                CastSpell();
                return true;
            }
            return false;
        }

        static void CompleteSpell()
        {
            if(awarded)
            {
                AwardedSpells.Remove(NextSpell);
            }

            NextSpell = null;
            state = 0;
        }

        public static SpellInfo.SpellOut CastSpell()
        {
            SpellInfo.SpellOut so;
            so.si = null;
            so.worked = true;

            switch (NextSpell.spellType)
            {
                case SpellInfo.SpellType.DestroyLetter:
                    SpellDestroyLetter(lp);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.DestroyGroup:
                    SpellDestroyLetterGroupSmall(lp);
                    break;
                case SpellInfo.SpellType.RandomVowels:
                    switch(state)
                    {
                        case 0:
                            CreateRandomVowels(5);
                            state++;
                            break;
                        case 1:
                            foreach (LetterProp lp in RandomLetterList)
                            {
                                lp.UpdateLetterDisplay();
                                lp.FlipTileForward();
                                lp.TileIdle();
                            }
                            CompleteSpell();
                            RandomLetterList.Clear();
                            state = 0;
                            break;
                    }
                    break;
                case SpellInfo.SpellType.ChangeToVowel:
                    switch(state)
                    {
                        case 0:
                            ChangeToVowel(lp);
                            state++;
                            break;
                        case 1:
                            lp.UpdateLetterDisplay();
                            lp.FlipTileForward();
                            lp.TileIdle();
                            CompleteSpell();
                            break;
                    }
                    break;
                case SpellInfo.SpellType.Burn:
                    BurnTile(lp);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.LetterSwap:
                    if (LetterSwapStep == 0)
                    {
                        LetterSwapFirst = lp;
                        LetterSwapStep = 1;
                        so.si = NextSpell;
                        return so;
                    }
                    else
                    {
                        so.worked = SwapLetters(lp, LetterSwapFirst);
                        LetterSwapFirst = null;
                        LetterSwapStep = 0;
                        so.si = null;
                        CompleteSpell();
                    }
                    break;
                case SpellInfo.SpellType.ConvertLetter:
                    ConvertLetterTile(lp);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.WordHint:
                    GetBestHint(10);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.WordHint2:
                    GetBestHint(200);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.RotateCW:
                    so.worked = Rotate(lp, -1);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.RotateCCW:
                    so.worked = Rotate(lp, 1);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.Rotate180:
                    so.worked = Rotate(lp, 4);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.HintOnLetter:
                    GetBestHint(lp);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.AnyLetter:
                    //PickALetter p = new PickALetter();
                    //var result = p.ShowAsync();
                    // if(result == ContentDialogResult.Primary)
                    {
                        //lp.letter = p.letter;
                        so.worked = true;
                    }
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.ColumnBGone:
                    for (int i = WSGameState.gridsize - 1; i >= 0; i--)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, i);
                    }
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.RowBGone:
                    for (int i = WSGameState.gridsize - 1; i >= 0; i--)
                    {
                        WSGameState.RemoveAndReplaceTile(i, lp.J);
                    }
                    CompleteSpell();
                    break;
            }

            so.si = null;
            return so;
        }

        public static void FinishSpellAnim()
        {
            if(NextSpell != null)
            {
                switch (NextSpell.spellType)
                {
                    case SpellInfo.SpellType.DestroyLetter:
                        break;
                    case SpellInfo.SpellType.DestroyGroup:
                        CastSpell();
                        break;
                    case SpellInfo.SpellType.RandomVowels:
                        foreach (LetterProp lp in RandomLetterList)
                        {
                            lp.UpdateLetterDisplay();
                            lp.FlipTileForward();
                            lp.TileIdle();
                        }
                        CompleteSpell();
                        RandomLetterList.Clear();
                        break;
                    case SpellInfo.SpellType.ChangeToVowel:
                        lp.UpdateLetterDisplay();
                        lp.FlipTileForward();
                        lp.TileIdle();
                        CompleteSpell();
                        break;
                }
            }
        }

        private static void ChangeToVowel(LetterProp lp)
        {
            switch(state)
            {
                case 0:
                    lp.FlipTileBack();
                    lp.letter = EngLetterScoring.RandomVowel();
                    state=0;
                    break;
            }
        }

        private static void GetBestHint(LetterProp lp)
        {
            WordWarAI wwai = new WordWarAI(WSGameState.LetterPropGrid);
            List<WordWarAI.Word> wl = wwai.FindAllWords(lp);
            WordWarAI.Word bw = new WordWarAI.Word();
            foreach (WordWarAI.Word w in wl)
            {
                if (bw.GetScore < w.GetScore)
                {
                    bw = w;
                }
            }

            WSGameState.boardScript.ShowMsg("Best word is " + bw.GetWord);
        }

        private static bool Rotate(LetterProp lp, int v)
        {
            if (!(lp.I > 0 && lp.J > 0 && lp.I < WSGameState.gridsize - 1 && lp.J < WSGameState.gridsize - 1))
            {
                WSGameState.boardScript.ShowMsg("Rotating doesn't work along the edges.");
                return false;
            }
            else
            {
                if (v == -1)
                {
                    // 
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1].LetterUCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J - 1].LetterLCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1].LetterLCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J].LetterDCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1].LetterDCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J + 1].LetterRCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1].LetterRCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J].LetterUCount = 1.0f;

                    SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I, lp.J - 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I, lp.J - 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J], WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I, lp.J + 1], WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I - 1, lp.J], false);
                }
                else if(v == 1)
                {
                    // 
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1].LetterRCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J - 1].LetterRCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1].LetterUCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J].LetterUCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1].LetterLCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J + 1].LetterLCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1].LetterDCount = 1.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J].LetterDCount = 1.0f;

                    SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I - 1, lp.J], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J], WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I, lp.J + 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J], WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], false);
                    SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I, lp.J - 1], false);
                }
                else
                {
                    // 
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1].LetterRCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1].LetterUCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J - 1].LetterUCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1].LetterUCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1].LetterLCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J].LetterLCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1].LetterLCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1].LetterDCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I, lp.J + 1].LetterDCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1].LetterRCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1].LetterDCount = 2.0f;
                    WSGameState.LetterPropGrid[lp.I - 1, lp.J].LetterRCount = 2.0f;

                    for (int vn = 0; vn < v; vn++)
                    {
                        SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I - 1, lp.J], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J], WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I, lp.J + 1], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I, lp.J + 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1], WSGameState.LetterPropGrid[lp.I + 1, lp.J], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J], WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], false);
                        SwapLetters(WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1], WSGameState.LetterPropGrid[lp.I, lp.J - 1], false);
                    }
                }
            }
            return true;
        }

        private static void GetBestHint(int min)
        {
            WordWarAI wwai = new WordWarAI(WSGameState.LetterPropGrid);
            List<WordWarAI.Word> wl = wwai.FindAllWords();
            WordWarAI.Word bw = new WordWarAI.Word();
            foreach (WordWarAI.Word w in wl)
            {
                if (bw.GetScore < w.GetScore)
                {
                    bw = w;
                    if (bw.GetScore >= min)
                        break;
                }
            }

            WSGameState.boardScript.ShowMsg("Best word is " + bw.GetWord);
        }

        private static void ConvertLetterTile(LetterProp lp)
        {
            byte changeletter = lp.letter;
            for (int i = WSGameState.gridsize - 1; i >= 0; i--)
            {
                for (int j = WSGameState.gridsize - 1; j >= 0; j--)
                {

                    if (WSGameState.LetterPropGrid[i, j].letter == changeletter)
                    {
                        WSGameState.LetterPropGrid[i, j].letter = EngLetterScoring.GetRandomLetter(false, WSGameState.GetFortune());
                    }
                }
            }
        }

        private static bool SwapLetters(LetterProp lpa, LetterProp lpb, bool move = true)
        {
            if (Math.Abs(lpa.I - lpb.I) - Math.Abs(lpa.J - lpb.J) > 1)
            {
                WSGameState.boardScript.ShowMsg("Swapping only works with adjcent letters, up/down/left/right.");
                return false;
            }
            else
            {
                int ti = lpa.I;
                lpa.I = lpb.I;
                lpb.I = ti;

                int tj = lpa.J;
                lpa.J = lpb.J;
                lpb.J = tj;

                if(move)
                {
                    Vector3 tv = lpa.Tf.position;
                    lpa.Tf.position = lpb.Tf.position;
                    lpb.Tf.position = tv;
                }

                WSGameState.LetterPropGrid[lpa.I, lpa.J] = lpa;
                WSGameState.LetterPropGrid[lpb.I, lpb.J] = lpb;
            }

            return true;
        }

        private static void BurnTile(LetterProp lp)
        {
            lp.ChangeTileTo(LetterProp.TileTypes.Burning);
        }

        private static void CreateRandomVowels(int v)
        {
            int n = v;
            int x = 20;  // Try at most to change 20 letters

            while (n > 0 && x > 0)
            {
                int i = r.Next(WSGameState.gridsize);
                int j = r.Next(WSGameState.gridsize);

                if (EngLetterScoring.IsConsonant((string)WSGameState.LetterPropGrid[i, j].ASCIIString))
                {
                    WSGameState.LetterPropGrid[i, j].FlipTileBack();
                    WSGameState.LetterPropGrid[i, j].letter = EngLetterScoring.RandomVowel();
                    RandomLetterList.Add(WSGameState.LetterPropGrid[i, j]);
                    n--;
                }
                x--;
            }
        }

        private static void SpellDestroyLetterGroupSmall(LetterProp lp)
        {
            switch (state)
            {
                case 0:
                    lp.BlowupTile();
                    state++;
                    break;
                case 1:
                    if (lp.J - 1 >= 0)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, lp.J - 1);
                    }
                    WSGameState.RemoveAndReplaceTile(lp.I, lp.J);

                    if (lp.J + 1 < WSGameState.gridsize)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, lp.J + 1);
                    }

                    WSGameState.RemoveAndReplaceTile(lp.I, lp.J);

                    if (lp.I - 1 >= 0)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I - 1, lp.J);
                    }

                    if (lp.I + 1 < WSGameState.gridsize)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I + 1, lp.J);
                    }
                    CompleteSpell();
                    break;
            }
        }

        private static void SpellDestroyLetter(LetterProp lp)
        {
            WSGameState.RemoveAndReplaceTile(lp.I, lp.J);
        }

        #endregion SpellCasting
    }

    //    partial class WSGameState
    //    {
    //        private static int LetterSwapStep;
    //        private static LetterProp LetterSwapFirst;

    //        private static bool resume = false;
    //        private static bool freeSpell;

    //        public static bool Resume { get { return resume; } set { resume = value; } }

    //        private static SpellInfo.SpellOut CastSpell(SpellInfo si, LetterProp lp)
    //        {
    //            SpellInfo.SpellOut so;
    //            so.si = null;
    //            so.worked = true;
    ////            bool SpellWorked = true;
    //  //          NextSpell = null;

    //            switch (si.spellType)
    //            {
    //                case SpellInfo.SpellType.DestroyLetter:
    //                    SpellDestroyLetter(lp);
    //                    break;
    //                case SpellInfo.SpellType.DestroyGroup:
    //                    SpellDestroyLetterGroupSmall(lp);
    //                    break;
    //                case SpellInfo.SpellType.RandomVowels:
    //                    CreateRandomVowels(5);
    //                    break;
    //                case SpellInfo.SpellType.ChangeToVowel:
    //                    lp.FlipTileBack();
    //                    lp.letter = EngLetterScoring.RandomVowel();
    //                    so.si = si;
    //                    return so;
    //                    break;
    //                case SpellInfo.SpellType.Burn:
    //                    BurnTile(lp);
    //                    break;
    //                case SpellInfo.SpellType.LetterSwap:
    //                    if(LetterSwapStep == 0)
    //                    {
    //                        LetterSwapFirst = lp;
    //                        LetterSwapStep = 1;
    //                        so.si = si;
    //                        return so;
    //                    }
    //                    else
    //                    {
    //                        so.worked = SwapLetters(lp, LetterSwapFirst);
    //                        LetterSwapFirst = null;
    //                        LetterSwapStep = 0;
    //                        so.si = null;
    //                    }
    //                    break;
    //                case SpellInfo.SpellType.ConvertLetter:
    //                    ConvertLetterTile(lp);
    //                    break;
    //                case SpellInfo.SpellType.WordHint:
    //                    GetBestHint(10);
    //                    break;
    //                case SpellInfo.SpellType.WordHint2:
    //                    GetBestHint(200);
    //                    break;
    //                case SpellInfo.SpellType.RotateL:
    //                    so.worked = Rotate(lp, -1);
    //                    break;
    //                case SpellInfo.SpellType.RotateR:
    //                    so.worked = Rotate(lp, 1);
    //                    break;
    //                case SpellInfo.SpellType.Rotate180:
    //                    so.worked = Rotate(lp, 4);
    //                    break;
    //                case SpellInfo.SpellType.HintOnLetter:
    //                    GetBestHint(lp);
    //                    break;
    //                case SpellInfo.SpellType.AnyLetter:
    //                    //PickALetter p = new PickALetter();
    //                    //var result = p.ShowAsync();
    //                   // if(result == ContentDialogResult.Primary)
    //                    {
    //                        //lp.letter = p.letter;
    //                        so.worked = true;
    //                    }
    //                    break;
    //                case SpellInfo.SpellType.ColumnBGone:
    //                    for(int i = gridsize - 1; i >= 0; i--)
    //                    {
    //                        RemoveAndReplaceTile(lp.I, i);
    //                    }
    //                    break;
    //                case SpellInfo.SpellType.RowBGone:
    //                    for (int i = gridsize - 1; i >= 0; i--)
    //                    {
    //                        RemoveAndReplaceTile(i, lp.J);
    //                    }
    //                    break;
    //            }

    //            //freeSpell = false;
    //            so.si = null;

    //            return so;
    //        }

    //        private static void GetBestHint(LetterProp lp)
    //        {
    //            WordWarAI wwai = new WordWarAI(LetterPropGrid);
    //            List<WordWarAI.Word> wl = wwai.FindAllWords(lp);
    //            WordWarAI.Word bw = new WordWarAI.Word();
    //            foreach (WordWarAI.Word w in wl)
    //            {
    //                if (bw.GetScore < w.GetScore)
    //                {
    //                    bw = w;
    //                }
    //            }

    //            boardScript.ShowMsg("Best word is " + bw.GetWord);
    //        }

    //        private static bool Rotate(LetterProp lp, int v)
    //        {
    //            if (!(lp.I > 0 && lp.J > 0 && lp.I < gridsize-1 && lp.J < gridsize-1))
    //            {
    //                boardScript.ShowMsg("Rotating doesn't work along the edges.");
    //                return false;
    //            }
    //            else
    //            {
    //                if(v <= -1)
    //                {
    //                    SwapLetters(LetterPropGrid[lp.I - 1, lp.J - 1], LetterPropGrid[lp.I, lp.J - 1]);
    //                    SwapLetters(LetterPropGrid[lp.I, lp.J - 1], LetterPropGrid[lp.I + 1, lp.J - 1]);
    //                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J - 1], LetterPropGrid[lp.I + 1, lp.J]);
    //                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J], LetterPropGrid[lp.I + 1, lp.J + 1]);
    //                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J + 1], LetterPropGrid[lp.I, lp.J + 1]);
    //                    SwapLetters(LetterPropGrid[lp.I, lp.J + 1], LetterPropGrid[lp.I - 1, lp.J + 1]);
    //                    SwapLetters(LetterPropGrid[lp.I - 1, lp.J + 1], LetterPropGrid[lp.I - 1, lp.J]);
    //                }
    //                else
    //                {
    //                    for(int vn = 0; vn < v; vn++)
    //                    {
    //                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J - 1], LetterPropGrid[lp.I - 1, lp.J]);
    //                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J], LetterPropGrid[lp.I - 1, lp.J + 1]);
    //                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J + 1], LetterPropGrid[lp.I, lp.J + 1]);
    //                        SwapLetters(LetterPropGrid[lp.I, lp.J + 1], LetterPropGrid[lp.I + 1, lp.J + 1]);
    //                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J + 1], LetterPropGrid[lp.I + 1, lp.J]);
    //                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J], LetterPropGrid[lp.I + 1, lp.J - 1]);
    //                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J - 1], LetterPropGrid[lp.I, lp.J - 1]);
    //                    }
    //                }
    //            }
    //            return true;
    //        }

    //        private static void GetBestHint(int min)
    //        {
    //            WordWarAI wwai = new WordWarAI(LetterPropGrid);
    //            List<WordWarAI.Word> wl = wwai.FindAllWords();
    //            WordWarAI.Word bw = new WordWarAI.Word();
    //            foreach (WordWarAI.Word w in wl)
    //            {
    //                if (bw.GetScore < w.GetScore)
    //                {
    //                    bw = w;
    //                    if (bw.GetScore >= min)
    //                        break;
    //                }
    //            }

    //            boardScript.ShowMsg("Best word is " + bw.GetWord);
    //        }

    //        private static void ChangeManna(int manna)
    //        {
    //            Manna += manna;
    //            UpdateManaScore();
    //        }

    //        private static void ConvertLetterTile(LetterProp lp)
    //        {
    //            byte changeletter = lp.letter;
    //            for (int i = gridsize - 1; i >= 0; i--)
    //            {
    //                for (int j = gridsize - 1; j >= 0; j--)
    //                {

    //                    if(LetterPropGrid[i, j].letter == changeletter)
    //                    {
    //                        LetterPropGrid[i, j].letter = EngLetterScoring.GetRandomLetter(false, GetFortune());
    //                    }
    //                }
    //            }
    //        }

    //        private static bool SwapLetters(LetterProp lpa, LetterProp lpb)
    //        {
    //            if (Math.Abs(lpa.I - lpb.I) - Math.Abs(lpa.J - lpb.J) > 1)
    //            {
    //                boardScript.ShowMsg("Swapping only works with adjcent letters, up/down/left/right.");
    //                return false;
    //            }
    //            else
    //            {
    //                int ti = lpa.I;
    //                lpa.I = lpb.I;
    //                lpb.I = ti;

    //                int tj = lpa.J;
    //                lpa.J = lpb.J;
    //                lpb.J = tj;

    //                Vector3 tv = lpa.Tf.position;
    //                lpa.Tf.position = lpb.Tf.position;
    //                lpb.Tf.position = tv;

    //                //int tti = lpa.i;


    //                LetterPropGrid[lpa.I, lpa.J] = lpa;
    //                LetterPropGrid[lpb.I, lpb.J] = lpb;
    //            }

    //            return true;
    //        }

    //        private static void BurnTile(LetterProp lp)
    //        {
    //            lp.ChangeTileTo(LetterProp.TileTypes.Burning);
    //        }

    //        private static void CreateRandomVowels(int v)
    //        {
    //            int n = v;
    //            int x = 20;  // Try at most to change 20 letters

    //            while (n > 0 && x > 0)
    //            {
    //                int i = r.Next(gridsize);
    //                int j = r.Next(gridsize);

    //                if (EngLetterScoring.IsConsonant((string)LetterPropGrid[i, j].ASCIIString))
    //                {
    //                    LetterPropGrid[i, j].FlipTileBack();
    //                    LetterPropGrid[i, j].letter = EngLetterScoring.RandomVowel();
    //                    LetterPropGrid[i, j].FlipTileForward();
    //                    LetterPropGrid[i, j].TileIdle();
    //                    n--; 
    //                }
    //                x--;
    //            }
    //        }

    //        private static void SpellDestroyLetterGroupSmall(LetterProp lp)
    //        {
    //            if (lp.J - 1 >= 0)
    //            {
    //                RemoveAndReplaceTile(lp.I, lp.J - 1);
    //            }
    //            RemoveAndReplaceTile(lp.I, lp.J);

    //            if (lp.J + 1 < gridsize)
    //            {
    //                RemoveAndReplaceTile(lp.I, lp.J + 1);
    //            }

    //            RemoveAndReplaceTile(lp.I, lp.J);

    //            if (lp.I - 1 >= 0)
    //            {
    //                RemoveAndReplaceTile(lp.I - 1, lp.J);
    //            }

    //            if (lp.I + 1 < gridsize)
    //            {
    //                RemoveAndReplaceTile(lp.I + 1, lp.J);
    //            }
    //        }

    //        private static void SpellDestroyLetter(LetterProp lp)
    //        {
    //            RemoveAndReplaceTile(lp.I, lp.J);
    //        }

    //        internal static void ReadySpell(SpellInfo selectedSpell, bool _freespell)
    //        {
    //            freeSpell = _freespell;
    //            if (selectedSpell.Immediate)
    //            {
    //                SpellInfo.SpellOut so;

    //                so = CastSpell(selectedSpell, null);
    //                if (freeSpell && so.worked)
    //                {
    //                    Spells.RemoveAwardedSpells(selectedSpell);
    //                    freeSpell = false;
    //                }
    //                else
    //                {
    //                    ChangeManna(-selectedSpell.MannaPoints);
    //                }
    //            }
    //            else
    //            {
    //                //CastSpell(selectedSpell, null, NextSpell);
    //                NextSpell = selectedSpell;
    //            }
    //        }
    //    }
}

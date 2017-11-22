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
            RotateL,
            RotateR,
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
            { new SpellInfo { spellType = SpellInfo.SpellType.LetterSwap, FriendlyName = "Swap",        MannaPoints = 6, SpellLevel = 5, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RandomVowels, FriendlyName = "Vowel Dust", MannaPoints = 10, SpellLevel = 15, Immediate = true }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ConvertLetter, FriendlyName = "Convert",  MannaPoints = 12, SpellLevel = 7, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateL, FriendlyName = "Rotate Left",  MannaPoints = 3, SpellLevel = 9, Immediate = false }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateR, FriendlyName = "Rotate Right",  MannaPoints = 3, SpellLevel = 9, Immediate = false }},
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

        //public static void ListButtons(Grid g, Grid fg, int manna, RoutedEventHandler spellClick, RoutedEventHandler spellFoundClick)
        //{
        //    int buttonwidth = (int)(g.Width / SpellPerRow);

        //    RowDefinition[] rows = new RowDefinition[7];
        //    ColumnDefinition[] columns = new ColumnDefinition[SpellPerRow];
        //    RowDefinition[] frows = new RowDefinition[7];
        //    ColumnDefinition[] fcolumns = new ColumnDefinition[SpellPerRow];

        //    for (int x = 0; x < columns.Length; x++)
        //    {
        //        columns[x] = new ColumnDefinition();
        //        g.ColumnDefinitions.Add(columns[x]);
        //        fcolumns[x] = new ColumnDefinition();
        //        fg.ColumnDefinitions.Add(fcolumns[x]);
        //    }

        //    for (int x = 0; x < rows.Length; x++)
        //    {
        //        rows[x] = new RowDefinition();
        //        g.RowDefinitions.Add(rows[x]);
        //        frows[x] = new RowDefinition();
        //        fg.RowDefinitions.Add(frows[x]);
        //    }

        //    int i = 0, j = 0;

        //    foreach (SpellInfo si in AvailableSpells)
        //    {
        //        Button b = new Button();
        //        CreateSpellButton(b, si);

        //        b.Content = si.FriendlyName + "(" + si.MannaPoints.ToString() + ")";
        //        b.Click += spellClick;

        //        if (manna < si.MannaPoints)
        //        {
        //            b.IsEnabled = false;
        //        }

        //        Grid.SetRow(b, j);
        //        Grid.SetColumn(b, i);

        //        g.Children.Add(b);

        //        i++;
        //        if (i >= SpellPerRow)
        //        {
        //            i = 0;
        //            j++;
        //        }
        //    }

        //    i = 0;
        //    j = 0;

        //    foreach (SpellInfo si in FoundSpells)
        //    {
        //        Button b = new Button();
        //        CreateSpellButton(b, si);

        //        b.Content = si.FriendlyName;
        //        b.Click += spellFoundClick;

        //        Grid.SetRow(b, j);
        //        Grid.SetColumn(b, i);

        //        fg.Children.Add(b);

        //        i++;
        //        if (i >= SpellPerRow)
        //        {
        //            i = 0;
        //            j++;
        //        }
        //    }
        //}

        //private static void CreateSpellButton(Button b, SpellInfo si)
        //{
        //    b.DataContext = si;
        //    b.FontSize = 16;
        //    b.Margin = new Thickness(1);
        //    b.Padding = new Thickness(1);
        //    b.Width = 120;
        //    b.Height = 28;
        //    b.VerticalAlignment = VerticalAlignment.Center;
        //}

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
    }

    partial class WSGameState
    {
        private static int LetterSwapStep;
        private static LetterProp LetterSwapFirst;

        private static bool resume = false;
        private static bool freeSpell;

        public static bool Resume { get { return resume; } set { resume = value; } }
        
        private  static SpellInfo.SpellOut CastSpell(SpellInfo si, LetterProp lp)
        {
            SpellInfo.SpellOut so;
            so.si = null;
            so.worked = true;
//            bool SpellWorked = true;
  //          NextSpell = null;

            switch (si.spellType)
            {
                case SpellInfo.SpellType.DestroyLetter:
                    SpellDestroyLetter(lp);
                    break;
                case SpellInfo.SpellType.DestroyGroup:
                    SpellDestroyLetterGroupSmall(lp);
                    break;
                case SpellInfo.SpellType.RandomVowels:
                    CreateRandomVowels(5);
                    break;
                case SpellInfo.SpellType.ChangeToVowel:
                    lp.letter = EngLetterScoring.RandomVowel();
                    break;
                case SpellInfo.SpellType.Burn:
                    BurnTile(lp);
                    break;
                case SpellInfo.SpellType.LetterSwap:
                    if(LetterSwapStep == 0)
                    {
                        LetterSwapFirst = lp;
                        LetterSwapStep = 1;
                        so.si = si;
                        return so;
                    }
                    else
                    {
                        so.worked = SwapLetters(lp, LetterSwapFirst);
                        LetterSwapFirst = null;
                        LetterSwapStep = 0;
                        so.si = null;
                    }
                    break;
                case SpellInfo.SpellType.ConvertLetter:
                    ConvertLetterTile(lp);
                    break;
                case SpellInfo.SpellType.WordHint:
                    GetBestHint(10);
                    break;
                case SpellInfo.SpellType.WordHint2:
                    GetBestHint(200);
                    break;
                case SpellInfo.SpellType.RotateL:
                    so.worked = Rotate(lp, -1);
                    break;
                case SpellInfo.SpellType.RotateR:
                    so.worked = Rotate(lp, 1);
                    break;
                case SpellInfo.SpellType.Rotate180:
                    so.worked = Rotate(lp, 4);
                    break;
                case SpellInfo.SpellType.HintOnLetter:
                    GetBestHint(lp);
                    break;
                case SpellInfo.SpellType.AnyLetter:
                    //PickALetter p = new PickALetter();
                    //var result = p.ShowAsync();
                   // if(result == ContentDialogResult.Primary)
                    {
                        //lp.letter = p.letter;
                        so.worked = true;
                    }
                    break;
                case SpellInfo.SpellType.ColumnBGone:
                    for(int i = gridsize - 1; i >= 0; i--)
                    {
                        RemoveAndReplaceTile(lp.I, i);
                    }
                    break;
                case SpellInfo.SpellType.RowBGone:
                    for (int i = gridsize - 1; i >= 0; i--)
                    {
                        RemoveAndReplaceTile(i, lp.J);
                    }
                    break;
            }

            //freeSpell = false;
            so.si = null;

            return so;
        }

        internal static void CastSpell(SpellInfo si)
        {
            NextSpell = si;
        }

        private static void GetBestHint(LetterProp lp)
        {
            WordWarAI wwai = new WordWarAI(LetterPropGrid);
            List<WordWarAI.Word> wl = wwai.FindAllWords(lp);
            WordWarAI.Word bw = new WordWarAI.Word();
            foreach (WordWarAI.Word w in wl)
            {
                if (bw.GetScore < w.GetScore)
                {
                    bw = w;
                }
            }

            boardScript.ShowMsg("Best word is " + bw.GetWord);
        }

        private static bool Rotate(LetterProp lp, int v)
        {
            if (!(lp.I > 0 && lp.J > 0 && lp.I < gridsize-1 && lp.J < gridsize-1))
            {
                boardScript.ShowMsg("Rotating doesn't work along the edges.");
                return false;
            }
            else
            {
                if(v <= -1)
                {
                    SwapLetters(LetterPropGrid[lp.I - 1, lp.J - 1], LetterPropGrid[lp.I, lp.J - 1]);
                    SwapLetters(LetterPropGrid[lp.I, lp.J - 1], LetterPropGrid[lp.I + 1, lp.J - 1]);
                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J - 1], LetterPropGrid[lp.I + 1, lp.J]);
                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J], LetterPropGrid[lp.I + 1, lp.J + 1]);
                    SwapLetters(LetterPropGrid[lp.I + 1, lp.J + 1], LetterPropGrid[lp.I, lp.J + 1]);
                    SwapLetters(LetterPropGrid[lp.I, lp.J + 1], LetterPropGrid[lp.I - 1, lp.J + 1]);
                    SwapLetters(LetterPropGrid[lp.I - 1, lp.J + 1], LetterPropGrid[lp.I - 1, lp.J]);
                }
                else
                {
                    for(int vn = 0; vn < v; vn++)
                    {
                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J - 1], LetterPropGrid[lp.I - 1, lp.J]);
                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J], LetterPropGrid[lp.I - 1, lp.J + 1]);
                        SwapLetters(LetterPropGrid[lp.I - 1, lp.J + 1], LetterPropGrid[lp.I, lp.J + 1]);
                        SwapLetters(LetterPropGrid[lp.I, lp.J + 1], LetterPropGrid[lp.I + 1, lp.J + 1]);
                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J + 1], LetterPropGrid[lp.I + 1, lp.J]);
                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J], LetterPropGrid[lp.I + 1, lp.J - 1]);
                        SwapLetters(LetterPropGrid[lp.I + 1, lp.J - 1], LetterPropGrid[lp.I, lp.J - 1]);
                    }
                }
            }
            return true;
        }

        //internal static void ResizeButtonGrid(double w, double v)
        //{
        //    if(LetterGrid == null)
        //    {
        //        return;
        //    }

        //    double bs = Math.Min(w, v);

        //    LetterGrid.Width = bs;
        //    LetterGrid.Height = bs;

        //    ButtonWidth = (int)(bs / gridsize) ;
        //    ButtonHeight = (int)(bs / gridsize) ;

        //    for (int i = 0; i < LetterGrid.ColumnDefinitions.Count; i++)
        //    {
        //        LetterGrid.ColumnDefinitions[i].Width = new GridLength(ButtonWidth);
        //    }

        //    for (int i = 0; i < LetterGrid.RowDefinitions.Count; i++)
        //    {
        //        LetterGrid.RowDefinitions[i].Height = new GridLength(ButtonHeight);
        //    }

        //    for(int i = 0;i< gridsize; i++)
        //    {
        //        for(int j = 0;j< gridsize; j++)
        //        {
        //            LetterPropGrid[i, j].b.Width = ButtonWidth - 1;
        //            LetterPropGrid[i, j].b.Height = ButtonHeight - 1;
        //            LetterPropGrid[i, j].b.FontSize =  LetterProp.GetFontSize();
        //        }
        //    }
        //}

        private static void GetBestHint(int min)
        {
            //WordWarAI wwai = new WordWarAI(LetterPropGrid);
            //List<WordWarAI.Word> wl = wwai.FindAllWords();
            //WordWarAI.Word bw = new WordWarAI.Word();
            //foreach (WordWarAI.Word w in wl)
            //{
            //    if (bw.GetScore < w.GetScore)
            //    {
            //        bw = w;
            //        if (bw.GetScore >= min)
            //            break;
            //    }
            //}

            //Announcment a = new Announcment("Best word is " + bw.GetWord);
            //await a.ShowAsync();
        }

        private static void ChangeManna(int manna)
        {
            Manna += manna;
            //WordWarLogic.MannaScoreText.Text = "M: " + Manna.ToString();
        }

        private static void ConvertLetterTile(LetterProp lp)
        {
            byte changeletter = lp.letter;
            for (int i = gridsize - 1; i >= 0; i--)
            {
                for (int j = gridsize - 1; j >= 0; j--)
                {
                    
                    if(LetterPropGrid[i, j].letter == changeletter)
                    {
                        LetterPropGrid[i, j].letter = EngLetterScoring.GetRandomLetter(false, GetFortune());
                    }
                }
            }
        }

        private static bool SwapLetters(LetterProp lpa, LetterProp lpb)
        {
            if (Math.Abs(lpa.I - lpb.I) - Math.Abs(lpa.J - lpb.J) > 1)
            {
                boardScript.ShowMsg("Swapping only works with adjcent letters, up/down/left/right.");
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

                Vector3 tv = lpa.Tf.position;
                lpa.Tf.position = lpb.Tf.position;
                lpb.Tf.position = tv;

                //int tti = lpa.i;


                LetterPropGrid[lpa.I, lpa.J] = lpa;
                LetterPropGrid[lpb.I, lpb.J] = lpb;
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
                int i = r.Next(gridsize);
                int j = r.Next(gridsize);

                if (EngLetterScoring.IsConsonant((string)LetterPropGrid[i, j].ASCIIString))
                {
                    LetterPropGrid[i, j].letter = EngLetterScoring.RandomVowel();
                    n--;
                }
                x--;
            }
        }

        private static void SpellDestroyLetterGroupSmall(LetterProp lp)
        {
            if (lp.J - 1 >= 0)
            {
                RemoveAndReplaceTile(lp.I, lp.J - 1);
            }
            RemoveAndReplaceTile(lp.I, lp.J);

            if (lp.J + 1 < gridsize)
            {
                RemoveAndReplaceTile(lp.I, lp.J + 1);
            }

            RemoveAndReplaceTile(lp.I, lp.J);

            if (lp.I - 1 >= 0)
            {
                RemoveAndReplaceTile(lp.I - 1, lp.J);
            }

            if (lp.I + 1 < gridsize)
            {
                RemoveAndReplaceTile(lp.I + 1, lp.J);
            }
        }

        private static void SpellDestroyLetter(LetterProp lp)
        {
            RemoveAndReplaceTile(lp.I, lp.J);
        }

        internal static void ReadySpell(SpellInfo selectedSpell, bool _freespell)
        {
            freeSpell = _freespell;
            if (selectedSpell.Immediate)
            {
                SpellInfo.SpellOut so;
                
                so = CastSpell(selectedSpell, null);
                if (freeSpell && so.worked)
                {
                    Spells.RemoveAwardedSpells(selectedSpell);
                    freeSpell = false;
                }
                else
                {
                    ChangeManna(-selectedSpell.MannaPoints);
                }
            }
            else
            {
                //CastSpell(selectedSpell, null, NextSpell);
                NextSpell = selectedSpell;
            }
        }
    }
}

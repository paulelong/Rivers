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

        public string SpellHelp
        {
            get { return LocalizationManager.instance.GetLocalizedValue(ImageName); }
        }

    }

    class Spells
    {
        #region Privates
        private const int SpellPerRow = 3;
        //static System.Random r = new System.Random();
        private static SpellInfo NextSpell;
        private static LetterProp LetterSwapFirst;
        private static LetterProp LetterSwapSecond;
        private static bool awarded = false;
        private static SpellCompletedSuccessfullyDelegate spellCompelteDelegate;
        private static int state = 0;
        private static LetterProp lp = null;
        private static List<LetterProp> RandomLetterList = new List<LetterProp>();
        #endregion Privates

        static List<SpellInfo> allSpells = new List<SpellInfo>
        {
            { new SpellInfo { spellType = SpellInfo.SpellType.LetterSwap,   FriendlyName = "Swap",      MannaPoints = 10, SpellLevel = 5, Immediate = false, ImageName = "Swap" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.Burn,         FriendlyName = "Lava" ,     MannaPoints = 6, SpellLevel = 6, Immediate = false, ImageName = "Burn"  }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RandomVowels, FriendlyName = "Vowel Dust", MannaPoints = 10, SpellLevel = 7, Immediate = true, ImageName = "VowelDust" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ConvertLetter, FriendlyName = "Convert",  MannaPoints = 12, SpellLevel = 8, Immediate = false, ImageName = "ConvertLetter" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyGroup, FriendlyName = "Bomb",      MannaPoints = 8, SpellLevel = 9, Immediate = false, ImageName = "Bomb" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateCW,     FriendlyName = "Rotate CW",  MannaPoints = 3, SpellLevel = 10, Immediate = false, ImageName = "CWRotate"  }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RotateCCW,    FriendlyName = "Rotate CCW",  MannaPoints = 3, SpellLevel = 10, Immediate = false, ImageName = "CCWRotate"  }},
            { new SpellInfo { spellType = SpellInfo.SpellType.WordHint,     FriendlyName = "Hint",      MannaPoints = 10, SpellLevel = 11, Immediate = true,ImageName = "Hint" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.Rotate180,    FriendlyName = "Rotate 180",  MannaPoints = 5, SpellLevel = 12, Immediate = false, ImageName = "Rotate180"  }},
            { new SpellInfo { spellType = SpellInfo.SpellType.DestroyLetter, FriendlyName = "Snipe",    MannaPoints = 10, SpellLevel = 13, Immediate = false, ImageName = "Snipe" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.WordHint2,    FriendlyName = "Hint++",    MannaPoints = 14, SpellLevel = 14, Immediate = true,ImageName = "HintPlus" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ChangeToVowel, FriendlyName = "Vowelize", MannaPoints = 7, SpellLevel = 15, Immediate = false, ImageName = "Vowelize" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.HintOnLetter, FriendlyName = "Letter Hint",  MannaPoints = 15, SpellLevel = 16, Immediate = false,ImageName = "HintLet" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.AnyLetter,    FriendlyName = "Any Letter",  MannaPoints = 12, SpellLevel = 17, Immediate = false, ImageName = "AnyLetter" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.RowBGone,     FriendlyName = "Row b'Gone",  MannaPoints = 12, SpellLevel = 18, Immediate = false, ImageName = "RowGone" }},
            { new SpellInfo { spellType = SpellInfo.SpellType.ColumnBGone,  FriendlyName = "Col b'Gone",  MannaPoints = 12, SpellLevel = 18, Immediate = false, ImageName = "ColGone" }},
        };

        public static List<SpellInfo> AllSpells
        {
            get { return allSpells;  }
        }

        public static List<SpellInfo> AvailableSpells = new List<SpellInfo>();

        public static SpellInfo LastSuccessfulSpell;

        public static int LastManaCost { get; internal set; }

        public delegate void SpellCompletedSuccessfullyDelegate();

        #region SpellManagement

        internal static SpellInfo GetSpell(int v, int level)
        {
            // Generate a random spell weighting by v

            List<SpellInfo> inplay = new List<SpellInfo>();

            for (int i = v; i >= 0; i--)
            {
                foreach (SpellInfo si in allSpells)
                {
                    if (si.SpellLevel <= (level + i + 5))
                    {
                        inplay.Add(si);
                    }
                }
            }

            int itemnumber = WSGameState.Rnd.Next(inplay.Count);
            SpellInfo rsi = inplay[itemnumber];

            return rsi;
        }

        public static SpellInfo FindSpell(string name)
        {
            return( allSpells.Find(x => (x.FriendlyName == name)) );
        }

        public static bool SpellReady()
        {
            return NextSpell != null;
        }

        internal static void UpdateSpellsForLevel(int level)
        {
            AvailableSpells.Clear();
            foreach (SpellInfo si in allSpells)
            {
                if (level >= si.SpellLevel)
                {
                    AvailableSpells.Add(si);
                }
            }

            if (Spells.AvailableSpells.Count > 0 || WSGameState.AwardedSpells.Count > 0)
            {
                WSGameState.boardScript.ShowSpellStuff();
            }
        }

        internal static bool HasSpells()
        {
            return (AvailableSpells.Count > 0);
        }

        #endregion SpellManagement

        #region SpellCasting

        public static void ReadySpell(string spellname, bool _awarded, SpellCompletedSuccessfullyDelegate _spellCompleteDelegate)
        {
            NextSpell = FindSpell(spellname);
            awarded = _awarded;
            spellCompelteDelegate = _spellCompleteDelegate;

            WSGameState.boardScript.ShowMagic(NextSpell.SpellHelp);
            WSGameState.boardScript.UpdateMagicPic(NextSpell.Image);

            //if(NextSpell.Immediate)
            //{
            //    CastSpell();
            //}
        }

        public static bool EvalSpell(LetterProp _lp)
        {
            if(NextSpell != null)
            {
                lp = _lp;
                ExecuteSelectionSpell();
                return true;
            }
            return false;
        }

        static void CompleteSpell(bool worked = true)
        {
            WSGameState.MagicDeselect();
            if(worked)
            {
                LastSuccessfulSpell = NextSpell;
                if (awarded)
                {
                    LastManaCost = 0;
                }
                else
                {
                    LastManaCost = -NextSpell.MannaPoints;
                }

                // Why do I need a delegate here?
                spellCompelteDelegate();
            }

            //WSGameState.boardScript.ShowCancelCast(false);
            NextSpell = null;
            state = 0;
        }

        public static void AbortSpell()
        {
            WSGameState.MagicDeselect();
            NextSpell = null;
            state = 0;
            //WSGameState.boardScript.SetSpellButton(true);
        }

        // Do the seelction, if neceesary for the spell.
        public static void ExecuteSelectionSpell(string s = null)
        {
            if (NextSpell == null)
            {
                return;
            }

            switch (NextSpell.spellType)
            {
                case SpellInfo.SpellType.DestroyGroup:
                    WSGameState.MagicDeselect();
                    WSGameState.MagicSelect(lp);
                    //if()
                    WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I, lp.J + 1]);
                    WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I, lp.J - 1]);
                    WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I + 1, lp.J]);
                    WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I - 1, lp.J]);
                    break;
                case SpellInfo.SpellType.RotateCW:
                case SpellInfo.SpellType.RotateCCW:
                case SpellInfo.SpellType.Rotate180:
                    WSGameState.MagicDeselect();
                    if(lp.I > 0 && lp.I < WSGameState.Gridsize - 1 && lp.J > 0 && lp.J < WSGameState.Gridsize)
                    {
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I, lp.J + 1]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I, lp.J - 1]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I + 1, lp.J]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I - 1, lp.J]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I + 1, lp.J + 1]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I - 1, lp.J + 1]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I + 1, lp.J - 1]);
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I - 1, lp.J - 1]);
                    }
                    break;
                case SpellInfo.SpellType.DestroyLetter:
                case SpellInfo.SpellType.ChangeToVowel:
                case SpellInfo.SpellType.Burn:
                case SpellInfo.SpellType.ConvertLetter:
                case SpellInfo.SpellType.HintOnLetter:
                case SpellInfo.SpellType.AnyLetter:
                    WSGameState.MagicDeselect();
                    WSGameState.MagicSelect(lp);
                    break;
                case SpellInfo.SpellType.RandomVowels:
                case SpellInfo.SpellType.WordHint:
                case SpellInfo.SpellType.WordHint2:
                    break;
                case SpellInfo.SpellType.LetterSwap:
                    switch (state)
                    {
                        case 0:
                            LetterSwapFirst = lp;
                            WSGameState.MagicSelect(lp);
                            state++;
                            break;
                        case 1:
                            if (!LettersAdjacent(lp, LetterSwapFirst))
                            {
                                WSGameState.MagicDeselect();
                                LetterSwapFirst = lp;
                            }
                            else
                            {
                                LetterSwapSecond = lp;
                                state++;
                            }

                            WSGameState.MagicSelect(lp);
                            break;
                        case 2:
                            if (LettersAdjacent(lp, LetterSwapFirst))
                            {
                                WSGameState.MagicDeselect();
                                WSGameState.MagicSelect(LetterSwapFirst);
                                LetterSwapSecond = lp;
                            }
                            else if(LettersAdjacent(lp, LetterSwapSecond))
                            {
                                WSGameState.MagicDeselect();
                                WSGameState.MagicSelect(LetterSwapSecond);
                                LetterSwapFirst = lp;
                            }
                            else
                            {
                                LetterSwapFirst = lp;
                                WSGameState.MagicDeselect();
                                state--;
                            }

                            WSGameState.MagicSelect(lp);
                            break;

                    }
                    break;
                case SpellInfo.SpellType.ColumnBGone:
                    WSGameState.MagicDeselect();

                    for (int i = 0; i < WSGameState.Gridsize; i++)
                    {
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[lp.I, i]);
                    }
                    break;
                case SpellInfo.SpellType.RowBGone:
                    WSGameState.MagicDeselect();

                    for(int i = 0; i < WSGameState.Gridsize; i++)
                    {
                        WSGameState.MagicSelect(WSGameState.LetterPropGrid[i, lp.J]);
                    }
                    break;
            }
        }

        public static void CastSpell2(string s = null)
        {
            if(NextSpell == null)
            {
                return;
            }

            switch (NextSpell.spellType)
            {
                case SpellInfo.SpellType.DestroyLetter:
                    SpellDestroyLetter(lp);
                    WSGameState.boardScript.PlaySnipeSound();
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
                            if (RandomLetterList.Count > 0)
                            {
                                LetterProp _lp = RandomLetterList[0];

                                _lp.UpdateLetterDisplay();
                                _lp.FlipTileForward();
                                _lp.TileIdle();

                                WSGameState.boardScript.PlayTwinkleSound();

                                RandomLetterList.RemoveAt(0);
                            }

                            if (RandomLetterList.Count <= 0)
                            {
                                CompleteSpell();
                                RandomLetterList.Clear();
                                state = 0;
                            }
                            break;
                    }
                    break;
                case SpellInfo.SpellType.ChangeToVowel:
                    switch(state)
                    {
                        case 0:
                            lp.letter = EngLetterScoring.RandomVowel();
                            lp.FlipTileBack();
                            state++;
                            break;
                        case 1:
                            lp.UpdateLetterDisplay();
                            lp.FlipTileForward();
                            lp.TileIdle();
                            CompleteSpell();
                            WSGameState.boardScript.PlayTwinkleSound();
                            break;
                    }
                    break;
                case SpellInfo.SpellType.Burn:
                    BurnTile(lp);
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.LetterSwap:
                    if(SwapLetters(LetterSwapSecond, LetterSwapFirst, false))
                    {
                        WSGameState.boardScript.PlaySwapSound();

                        SwapMovement(LetterSwapSecond, LetterSwapFirst);

                        CompleteSpell();
                    }
                    else
                    {    
                        CompleteSpell(false);
                    }
                    break;
                case SpellInfo.SpellType.ConvertLetter:
                    switch(state)
                    {
                        case 0:
                            ConvertLetterTile(lp);
                            state++;
                            break;
                        case 1:
                            if (RandomLetterList.Count > 0)
                            {
                                LetterProp _lp = RandomLetterList[0];

                                _lp.UpdateLetterDisplay();
                                _lp.FlipTileForward();
                                _lp.TileIdle();

                                RandomLetterList.RemoveAt(0);
                            }

                            if (RandomLetterList.Count <= 0)
                            {
                                WSGameState.boardScript.PlayTwinkleSound();
                                CompleteSpell();
                                RandomLetterList.Clear();
                                state = 0;
                            }
                            break;
                    }
                    break;
                case SpellInfo.SpellType.WordHint:
                    GetBestHint(10);
                    WSGameState.boardScript.PlayTwinkleSound();
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.WordHint2:
                    GetBestHint(200);
                    WSGameState.boardScript.PlayTwinkleSound();
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.RotateCW:
                    WSGameState.boardScript.PlayRotateSound();
                    CompleteSpell(Rotate(lp, -1));
                    break;
                case SpellInfo.SpellType.RotateCCW:
                    WSGameState.boardScript.PlayRotateSound();
                    CompleteSpell(Rotate(lp, 1));
                    break;
                case SpellInfo.SpellType.Rotate180:
                    WSGameState.boardScript.PlayRotateSound();
                    CompleteSpell(Rotate(lp, 4));
                    break;
                case SpellInfo.SpellType.HintOnLetter:
                    GetBestHint(lp);
                    CompleteSpell();
                    WSGameState.boardScript.PlayTwinkleSound();
                    break;
                case SpellInfo.SpellType.AnyLetter:
                    //lp.AnimationEnabled = true;
                    switch (state)
                    {
                        case 0:
                            WSGameState.boardScript.SelectLetterToChange();
                            WSGameState.MagicSelect(lp);
                            state++;
                            break;
                        case 1:
                            if(s == "" || !char.IsLetter(s[0]) || s.Length > 1)
                            {
                                WSGameState.boardScript.ShowMsg("That entry was not valid.  Must enter a single letter.");
                                CompleteSpell(false);
                            }
                            else
                            {
                                lp.FlipTileBack();
                                lp.letter = (byte)s[0];
                                state++;
                            }
                            break;
                        case 2:
                            lp.UpdateLetterDisplay();
                            lp.FlipTileForward();
                            WSGameState.boardScript.PlayTwinkleSound();
                            CompleteSpell();
                            break;
                    }
                    break;
                case SpellInfo.SpellType.ColumnBGone:
                    for (int i = WSGameState.Gridsize - 1; i >= 0; i--)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, i);
                    }
                    WSGameState.boardScript.PlayZapSound();
                    CompleteSpell();
                    break;
                case SpellInfo.SpellType.RowBGone:
                    for (int i = WSGameState.Gridsize - 1; i >= 0; i--)
                    {
                        WSGameState.RemoveAndReplaceTile(i, lp.J);
                    }
                    WSGameState.boardScript.PlayZapSound();
                    CompleteSpell();
                    break;
            }
        }

        private static void SwapMovement(LetterProp lp1, LetterProp lp2)
        {
            LetterProp _lp1, _lp2;

            // On top
            if(lp1.I == lp2.I)
            {
                if (lp1.J > lp2.J)
                {
                    _lp1 = lp1;
                    _lp2 = lp2;
                }
                else
                {
                    _lp1 = lp2;
                    _lp2 = lp1;
                }

                _lp1.LetterRotVU = 180f;
                _lp1.LetterRotVUAxis = _lp1.LetTF.position + new Vector3(0, 0.5f * WSGameState.GridScale, 0);
                _lp1.LetterRotVUCAxis = _lp1.LetTF.position;
                //_lp1.AnimationEnabled = false;

                _lp2.LetterRotVD = 180f;
                _lp2.LetterRotVDAxis = _lp2.LetTF.position - new Vector3(0, 0.5f * WSGameState.GridScale, 0);
                _lp2.LetterRotVDCAxis = _lp2.LetTF.position;
                //_lp2.AnimationEnabled = false;
            } // Side by side
            else
            {
                if(lp1.I < lp2.I)
                {
                    _lp1 = lp1;
                    _lp2 = lp2;
                }
                else
                {
                    _lp1 = lp2;
                    _lp2 = lp1;
                }

                _lp1.LetterRotHL = 180f;
                _lp1.LetterRotHLAxis = _lp1.LetTF.position - new Vector3(0.5f * WSGameState.GridScale, 0, 0);
                _lp1.LetterRotHLCAxis = _lp1.LetTF.position;
                //_lp1.AnimationEnabled = false;

                _lp2.LetterRotHR = 180f;
                _lp2.LetterRotHRAxis = _lp2.LetTF.position + new Vector3(0.5f * WSGameState.GridScale, 0, 0);
                _lp2.LetterRotHLCAxis = _lp2.LetTF.position;
                //_lp2.AnimationEnabled = false;
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
            if (!(lp.I > 0 && lp.J > 0 && lp.I < WSGameState.Gridsize - 1 && lp.J < WSGameState.Gridsize - 1))
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

        public static void GetBestHint(int min)
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

            if(bw.GetWord.Length >= 3)
            {
                WSGameState.boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("HintPhrase") + bw.GetWord);
            }
            else
            {
                WSGameState.boardScript.ShowMsg(LocalizationManager.instance.GetLocalizedValue("HintNotFound"));
            }
        }

        private static void ConvertLetterTile(LetterProp lp)
        {
            // Since we change lp during the loop, we want to remember the original letter to change.
            char changeletter = lp.ASCIIChar;

            RandomLetterList.Clear();

            for (int i = WSGameState.Gridsize - 1; i >= 0; i--)
            {
                for (int j = WSGameState.Gridsize - 1; j >= 0; j--)
                {

                    if (WSGameState.LetterPropGrid[i, j].ASCIIChar == changeletter)
                    {
                        //WSGameState.LetterPropGrid[i, j].AnimationEnabled = true;

                        WSGameState.LetterPropGrid[i, j].FlipTileBack();
                        WSGameState.LetterPropGrid[i, j].letter = EngLetterScoring.GetRandomLetter(false, WSGameState.GetFortune());
                        RandomLetterList.Add(WSGameState.LetterPropGrid[i, j]);
                    }
                }
            }
        }

        private static bool LettersAdjacent(LetterProp lpa, LetterProp lpb)
        {
            if (((lpa.I == lpb.I) && Math.Abs(lpa.J - lpb.J) == 1) ||
                    ((lpa.J == lpb.J) && Math.Abs(lpa.I - lpb.I) == 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool SwapLetters(LetterProp lpa, LetterProp lpb, bool move = true)
        {
            if( LettersAdjacent(lpa, lpb))
            {
                int ti = lpa.I;
                lpa.I = lpb.I;
                lpb.I = ti;

                int tj = lpa.J;
                lpa.J = lpb.J;
                lpb.J = tj;

                if(move)
                {
                    Vector3 tv = lpa.LetTF.position;
                    lpa.LetTF.position = lpb.LetTF.position;
                    lpb.LetTF.position = tv;
                }

                WSGameState.LetterPropGrid[lpa.I, lpa.J] = lpa;
                WSGameState.LetterPropGrid[lpb.I, lpb.J] = lpb;
            }
            else
            {
                WSGameState.boardScript.ShowMsg("Swapping only works with adjacent letters, up/down or left/right.");
                return false;
            }

            return true;
        }

        private static void BurnTile(LetterProp lp)
        {

            if (lp.TileType == LetterProp.TileTypes.Speaker)
            {
                lp.StopBackgroundMusic();
                Debug.Log("Burned speaker, finding new one");
                WSGameState.NewMusicTile();
            }

            lp.TileType = LetterProp.TileTypes.Burning;

            //lp.AnimationEnabled = true;
            lp.ChangeTileTo(LetterProp.TileTypes.Burning);
        }

        private static void CreateRandomVowels(int v)
        {
            int n = v;
            int x = 20;  // Try at most to change 20 letters

            RandomLetterList.Clear();

            while (n > 0 && x > 0)
            {
                int i = WSGameState.Rnd.Next(WSGameState.Gridsize);
                int j = WSGameState.Rnd.Next(WSGameState.Gridsize);

                if (EngLetterScoring.IsConsonant((string)WSGameState.LetterPropGrid[i, j].ASCIIString))
                {
                    //WSGameState.LetterPropGrid[i, j].AnimationEnabled = true;
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
                    WSGameState.boardScript.PlayBombSound();
                    if (lp.J - 1 >= 0)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, lp.J - 1);
                    }
                    WSGameState.RemoveAndReplaceTile(lp.I, lp.J);

                    if (lp.J + 1 < WSGameState.Gridsize)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I, lp.J + 1);
                    }

                    WSGameState.RemoveAndReplaceTile(lp.I, lp.J);

                    if (lp.I - 1 >= 0)
                    {
                        WSGameState.RemoveAndReplaceTile(lp.I - 1, lp.J);
                    }

                    if (lp.I + 1 < WSGameState.Gridsize)
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
}

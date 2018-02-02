using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace WordSpell
{
    class WordWarAI
    {
        private LetterProp[,] LetterGrid;
        private char[,] LetterGridByte;
        private bool[,] LetterGridUsed;
        private char[] curword = new char[32];
        private int curwordoff = 0;

        int i_length;
        int j_length;
        
        private List<Word> WordsFromLetter = new List<Word>();
        private List<LetterProp> CurWordList = new List<LetterProp>();

        //static SolidColorBrush AnimateButtonColor = new SolidColorBrush(Colors.Red);

        struct WordElement
        {
            public WordElement(int _i, int _j, byte _letter)
            {
                i = _i;
                j = _j;
                letter = _letter;
            }

            public int i, j;
            byte letter;

            public byte Letter
            {
                get
                {
                    return letter;
                }
            }
        }

        public struct Word
        {
            public Word(List<LetterProp> lplist)
            {
                word = EngLetterScoring.GetCurrentWord(lplist);
                score = EngLetterScoring.ScoreWord(word);
            }

            public int GetScore
            {
                get { return score; }
            }

            public string GetWord
            {
                get { return word; }
            }

            string word;
            int score;
        }

        private string CurrentWordString()
        {
            string ret = "";

            foreach(WordElement we in CurrentWordFind)
            {
                ret += Convert.ToChar(we.Letter);
            }

            return ret;
        }

        private List<WordElement> CurrentWordFind = new List<WordElement>();

        public WordWarAI(LetterProp[,] _LetterButtons)
        {
            LetterGrid = _LetterButtons;

            i_length = _LetterButtons.GetLength(0);
            j_length = _LetterButtons.GetLength(1);

            LetterGridByte = new char[i_length, j_length];
            LetterGridUsed = new bool[i_length, j_length];

            for (int i = 0; i < i_length; i++)
            {
                for (int j = 0; j < j_length; j++)
                {
                    LetterGridUsed[i, j] = false;
                    LetterGridByte[i, j] = Convert.ToChar(_LetterButtons[i, j].letter);
                }
            }
        }

        public bool AnyWords()
        {
            for (int i = 0; i < i_length; i++)
            {
                for (int j = 0; j < j_length; j++)
                {
                    curwordoff = 0;
                    if(FindAnyWord(i, j))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // a z
        // t z

        public bool FindAnyWord2(int i, int j)
        {
            bool ret = false;

            if (CurrentWordFind.Exists(key => (key.i == i && key.j == j)))
            {
                return false;
            }

            CurrentWordFind.Add(new WordElement(i, j, LetterGrid[i, j].letter));
            //Debug.WriteLine(i.ToString() + " " + j.ToString() + " " + CurrentWordString());          

            if (CurrentWordFind.Count < 15)
            {
                string curword = CurrentWordString();

                if (EngLetterScoring.PartialExists(curword))
                {
                    if (CurrentWordFind.Count >= 3)
                    {
                        if (EngLetterScoring.IsWord(curword))
                        {
                            ret = true;
                        }
                    }

                    if (i > 0)
                    {
                        if (j > 0)
                        {
                            if(!ret && FindAnyWord(i - 1, j - 1))
                            {
                                ret = true;
                            }
                        }
                        if(!ret && FindAnyWord(i - 1, j))
                        {
                            ret = true;
                        }
                        if (j < WSGameState.Gridsize - 1)
                        {
                            if(!ret && FindAnyWord(i - 1, j + 1))
                            {
                                ret = true;
                            }
                        }
                    }
                    if (j > 0)
                    {
                        if(!ret && FindAnyWord(i, j - 1))
                        {
                            ret = true;
                        }
                    }
                    if (j < WSGameState.Gridsize - 1)
                    {
                        if(!ret && FindAnyWord(i, j + 1))
                        {
                            ret = true;
                        }
                    }
                    if (i < WSGameState.Gridsize - 1)
                    {
                        if (j > 0)
                        {
                            if(!ret && FindAnyWord(i + 1, j - 1))
                            {
                                ret = true;
                            }
                        }
                        if(!ret && FindAnyWord(i + 1, j))
                        {
                            ret = true;
                        }
                        if (j < WSGameState.Gridsize - 1)
                        {
                            if(!ret && FindAnyWord(i + 1, j + 1))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }

            CurrentWordFind.RemoveAt(CurrentWordFind.Count - 1);

            return ret;
        }

        public bool FindAnyWord(int i, int j)
        {
            bool ret = false;

            if (LetterGridUsed[i,j])
            {
                return false;
            }

            LetterGridUsed[i, j] = true;

            curword[curwordoff++] = LetterGridByte[i, j];

            //Debug.WriteLine(i.ToString() + " " + j.ToString() + " " + CurrentWordString());          

            if (curwordoff <= 15)
            {
                string _curword = new string(curword, 0, curwordoff);

                if (EngLetterScoring.PartialExists(_curword))
                {
                    if (curwordoff >= 3)
                    {
                        if (EngLetterScoring.IsWord(_curword))
                        {
                            ret = true;
                        }
                    }

                    if (i > 0)
                    {
                        if (j > 0)
                        {
                            if (!ret && FindAnyWord(i - 1, j - 1))
                            {
                                ret = true;
                            }
                        }
                        if (!ret && FindAnyWord(i - 1, j))
                        {
                            ret = true;
                        }
                        if (j < WSGameState.Gridsize - 1)
                        {
                            if (!ret && FindAnyWord(i - 1, j + 1))
                            {
                                ret = true;
                            }
                        }
                    }
                    if (j > 0)
                    {
                        if (!ret && FindAnyWord(i, j - 1))
                        {
                            ret = true;
                        }
                    }
                    if (j < WSGameState.Gridsize - 1)
                    {
                        if (!ret && FindAnyWord(i, j + 1))
                        {
                            ret = true;
                        }
                    }
                    if (i < WSGameState.Gridsize - 1)
                    {
                        if (j > 0)
                        {
                            if (!ret && FindAnyWord(i + 1, j - 1))
                            {
                                ret = true;
                            }
                        }
                        if (!ret && FindAnyWord(i + 1, j))
                        {
                            ret = true;
                        }
                        if (j < WSGameState.Gridsize - 1)
                        {
                            if (!ret && FindAnyWord(i + 1, j + 1))
                            {
                                ret = true;
                            }
                        }
                    }
                }
            }

            curword[curwordoff] = '\0';
            curwordoff--;

            LetterGridUsed[i, j] = false;

            return ret;
        }

        public List<Word> FindAllWords()
        {
            List<Word> words = new List<Word>();

            for(int i = 0; i < i_length; i++)
            {
                for(int j = 0; j < j_length; j++)
                {
                    curwordoff = 0;
                    WordsFromLetter.Clear();

                    CurrentWordFind.Clear();
                    CurWordList.Clear();

                    FindAllWords(i, j);

                    foreach(Word w in WordsFromLetter)
                    {
                        if(!words.Contains(w))
                        {
                            words.Add(w);
                        }
                    }
                }
            }

            return words;
        }

        public List<Word> FindAllWords(LetterProp lp)
        {
            List<Word> words = new List<Word>();

            for (int i = 0; i < i_length; i++)
            {
                for (int j = 0; j < j_length; j++)
                {
                    curwordoff = 0;
                    WordsFromLetter.Clear();

                    CurrentWordFind.Clear();
                    CurWordList.Clear();

                    FindAllWords(i, j, lp);

                    foreach (Word w in WordsFromLetter)
                    {
                        if (!words.Contains(w))
                        {
                            words.Add(w);
                        }
                    }
                }
            }

            return words;
        }

        private void FindAllWords(StringBuilder curword, int i, int j)
        {
            curword.Append(LetterGrid[i, j].ASCIIChar);

            if(curword.Length < 15)
            {
                if (curword.Length >= 3)
                {
                    if (EngLetterScoring.IsWord(curword.ToString()))
                    {
                        Word w = new Word(CurWordList);
                        if (!WordsFromLetter.Contains(w))
                        {
                            WordsFromLetter.Add(w);
                        }
                    }
                }

                if (EngLetterScoring.PartialExists(curword.ToString()))
                {
                    if (i > 0)
                    {
                        if (j > 0)
                        {
                            FindAllWords(curword, i - 1, j - 1);
                        }
                        FindAllWords(curword, i - 1, j);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(curword, i - 1, j + 1);
                        }
                    }
                    if (j > 0)
                    {
                        FindAllWords(curword, i, j - 1);
                    }
                    if (j < WSGameState.Gridsize - 1)
                    {
                        FindAllWords(curword, i, j + 1);
                    }
                    if (i < WSGameState.Gridsize - 1)
                    {
                        if (j > 0)
                        {
                            FindAllWords(curword, i + 1, j - 1);
                        }
                        FindAllWords(curword, i + 1, j);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(curword, i + 1, j + 1);
                        }
                    }
                }
            }

            curword.Remove(curword.Length - 1, 1);
            // one by one add each letter.  If there is no match, then stop that branch
        }

        private void FindAllWords(int i, int j)
        {
            if (CurrentWordFind.Exists(key => (key.i == i && key.j == j)))
            {
                return;
            }

            CurrentWordFind.Add(new WordElement(i, j, LetterGrid[i, j].letter));
            CurWordList.Add(LetterGrid[i, j]);

            if (CurrentWordFind.Count < 20)
            {
                string _curword = CurrentWordString();

                if (CurrentWordFind.Count >= 3)
                {
                    if (EngLetterScoring.IsWord(_curword))
                    {
                        Word w = new Word(CurWordList);
                        if(!WordsFromLetter.Contains(w))
                        {
                            WordsFromLetter.Add(w);
                        }
                    }
                }

                if (EngLetterScoring.PartialExists(_curword))
                {
                    if (i > 0)
                    {
                        if (j > 0)
                        {
                            FindAllWords(i - 1, j - 1);
                        }
                        FindAllWords(i - 1, j);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(i - 1, j + 1);
                        }
                    }
                    if (j > 0)
                    {
                        FindAllWords(i, j - 1);
                    }
                    if (j < WSGameState.Gridsize - 1)
                    {
                        FindAllWords(i, j + 1);
                    }
                    if (i < WSGameState.Gridsize - 1)
                    {
                        if (j > 0)
                        {
                            FindAllWords(i + 1, j - 1);
                        }
                        FindAllWords(i + 1, j);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(i + 1, j + 1);
                        }
                    }
                }
            }

            CurrentWordFind.RemoveAt(CurrentWordFind.Count - 1);
            CurWordList.RemoveAt(CurWordList.Count - 1);
        }

        private void FindAllWords(int i, int j, LetterProp lp)
        {
            if (CurrentWordFind.Exists(key => (key.i == i && key.j == j)))
            {
                return;
            }

            CurrentWordFind.Add(new WordElement(i, j, LetterGrid[i, j].letter));
            CurWordList.Add(LetterGrid[i, j]);

            if (CurrentWordFind.Count < 20)
            {
                string _curword = CurrentWordString();

                if (CurrentWordFind.Count >= 3)
                {
                    if (EngLetterScoring.IsWord(_curword) && CurrentWordFind.Exists(key => key.i== lp.I && key.j == lp.J))
                    {
                        Word w = new Word(CurWordList);
                        if (!WordsFromLetter.Contains(w))
                        {
                            WordsFromLetter.Add(w);
                        }
                    }
                }

                if (EngLetterScoring.PartialExists(_curword))
                {
                    if (i > 0)
                    {
                        if (j > 0)
                        {
                            FindAllWords(i - 1, j - 1, lp);
                        }
                        FindAllWords(i - 1, j, lp);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(i - 1, j + 1, lp);
                        }
                    }
                    if (j > 0)
                    {
                        FindAllWords(i, j - 1, lp);
                    }
                    if (j < WSGameState.Gridsize - 1)
                    {
                        FindAllWords(i, j + 1, lp);
                    }
                    if (i < WSGameState.Gridsize - 1)
                    {
                        if (j > 0)
                        {
                            FindAllWords(i + 1, j - 1, lp);
                        }
                        FindAllWords(i + 1, j, lp);
                        if (j < WSGameState.Gridsize - 1)
                        {
                            FindAllWords(i + 1, j + 1, lp);
                        }
                    }
                }
            }

            CurrentWordFind.RemoveAt(CurrentWordFind.Count - 1);
            CurWordList.RemoveAt(CurWordList.Count - 1);
        }
    }
}

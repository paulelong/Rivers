using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace WordSpell
{
    class EngLetterScoring
    {
        const string PartialLookupCache = "LookupCache.lst";
        const string DictionaryCache = "DictionaryCache.lst";
        static System.Random r = new System.Random(21);

        static char[] Vowels = { 'A', 'E', 'I', 'O', 'U' };
        static char[] RequiredLettersForWord = { 'a', 'e', 'i', 'o', 'u', 'y' };

        static private List<string> DictionaryLookup = new List<string>();
        static private List<string> PartialLookup = new List<string>();

        static public void LoadDictionary()
        {
            TextAsset DictFile = (TextAsset)Resources.Load("EngDictA");
            string[] words = DictFile.text.Split('\n');

            DictionaryLookup = new List<string>();
            foreach (String rs in words)
            {
                string s = rs.TrimEnd();

                if (!s.Contains("'") && s.Length > 2 && s.IndexOfAny(RequiredLettersForWord) >= 0)
                {
                    DictionaryLookup.Add(s);
                }
            }

            CreatePartialLookup();

            // Build partial list for each unique letter combination.
            //foreach (string s in DictionaryLookup)
            //{
            //    for (int i = 1; i <= s.Length; i++)
            //    {
            //        string partial = s.Substring(0, i);
            //        if (PartialLookup.BinarySearch(partial) < 0)
            //        {
            //            PartialLookup.Add(partial);
            //        }
            //    }
            //}
        }
            //    try
            //    {
            //        DictionaryLookup = await WordWarLogic.LoadList<List<string>>(DictionaryCache);
            //    }
            //    catch (Exception ex)
            //    {
            //        Announcment a = new Announcment(DictionaryCache + " " + ex.Message);
            //        await a.ShowAsync();
            //    }

            //    if(DictionaryLookup == null || DictionaryLookup.Count <= 0)
            //    {
            //        DictionaryLookup = new List<string>();

            //        var folders = (await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFoldersAsync()).To‌​List();
            //        StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Dictionaries\EngDictA.txt");

            //        IList<String> dictlines = await FileIO.ReadLinesAsync(file);

            //        foreach (String s in dictlines)
            //        {
            //            if (!s.Contains("'") && s.Length > 2 && s.IndexOfAny(RequiredLettersForWord) >= 0)
            //            {
            //                DictionaryLookup.Add(s);
            //            }
            //        }

            //        try
            //        {
            //            await WordWarLogic.SaveList<List<string>>(DictionaryLookup, DictionaryCache);
            //        }

            //        catch (Exception ex)
            //        {
            //            Announcment a = new Announcment(DictionaryCache + " " + ex.Message);
            //            await a.ShowAsync();
            //        }
            //    }

            //    // Build partial list for each unique letter combination.
            //    foreach (string s in DictionaryLookup)
            //    {
            //        for (int i = 1; i <= s.Length; i++)
            //        {
            //            string partial = s.Substring(0, i);
            //            if (PartialLookup.BinarySearch(partial) < 0)
            //            {
            //                PartialLookup.Add(partial);
            //            }
            //        }
            //    }
            //}

        static public bool IsWord(string word)
        {
//            DictionaryLookup.BinarySearch(word);
            return (DictionaryLookup.BinarySearch(word.ToLower()) >= 0);
        }

        static public void CreatePartialLookup()
        {
            string filePath = Application.persistentDataPath + "/" + PartialLookupCache;
            // Is it cached already?
            if (File.Exists(filePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(filePath, FileMode.Open);
                PartialLookup = (List<string>)bf.Deserialize(file);
                file.Close();
            }
            else
            {
                // Build partial list for each unique letter combination.
                foreach (string s in DictionaryLookup)
                {
                    for (int i = 1; i <= s.Length; i++)
                    {
                        string partial = s.Substring(0, i);
                        if (PartialLookup.BinarySearch(partial) < 0)
                        {
                            PartialLookup.Add(partial);
                        }
                    }
                }

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(filePath);
                bf.Serialize(file, PartialLookup);
                file.Close();
            }
        }

        internal static bool IsWord(StringBuilder curword)
        {
            return IsWord(curword.ToString());
        }

        private static Dictionary<char, int> values = new Dictionary<char, int>()
        {
            {'a', 1 },
            {'b', 3 },
            {'c', 3 },
            {'d', 2 },
            {'e', 1 },
            {'f', 4 },
            {'g', 2 },
            {'h', 4 },
            {'i', 1 },
            {'j', 8 },
            {'k', 5 },
            {'l', 1 },
            {'m', 3 },
            {'n', 1 },
            {'o', 1 },
            {'p', 2 },
            {'q', 10 },
            {'r', 1 },
            {'s', 1 },
            {'t', 1 },
            {'u', 1 },
            {'v', 4 },
            {'w', 4 },
            {'x', 8 },
            {'y', 4 },
            {'z', 10 },
            {'A', 1 },
            {'B', 3 },
            {'C', 3 },
            {'D', 2 },
            {'E', 1 },
            {'F', 4 },
            {'G', 2 },
            {'H', 4 },
            {'I', 1 },
            {'J', 8 },
            {'K', 5 },
            {'L', 1 },
            {'M', 3 },
            {'N', 1 },
            {'O', 1 },
            {'P', 2 },
            {'Q', 10 },
            {'R', 1 },
            {'S', 1 },
            {'T', 1 },
            {'U', 1 },
            {'V', 4 },
            {'W', 4 },
            {'X', 8 },
            {'Y', 4 },
            {'Z', 10 },
        };

        internal static bool PartialExists(string curword)
        {
            string curwordlower = curword.ToLower();
            // TODO Check optimization
            return PartialLookup.BinarySearch(curwordlower) >= 0;
            //return DictionaryLookup.Exists(x => x.StartsWith(curwordlower));
        }

        public static byte GetRandomLetter(bool isBurning, WSGameState.FortuneLevel fl)
        {
            byte b;

            if (fl == WSGameState.FortuneLevel.Bad)
            {
                b = (byte)r.Next('A', 'Z'+1);
            }
            else if (fl == WSGameState.FortuneLevel.Good)
            {
                int maxvalue = 10;

                int p = r.Next(5);
                if (p <= 2)
                {
                    maxvalue = 3;
                }
                else if (p <= 4)
                {
                    maxvalue = 5;
                }
                else
                {
                    maxvalue = 8;
                }

                do
                {
                    b = (byte)r.Next('A', 'Z'+1);
                } while (values[(char)b] >= maxvalue);
            }
            else
            {
                bool goodletter = false;

                if (r.Next(10) < 8)
                {
                    goodletter = true;
                }

                do
                {
                    b = (byte)r.Next('A', 'Z'+1);
                } while (values[(char)b] >= 3 && goodletter);
            }

            if ((b == 'Q' || b == 'Z' || b == 'J' || b == 'X') && isBurning)
            {
                b = (byte)'E';
            }

            return b; // System.Text.ASCIIEncoding.ASCII.GetString(new[] { b });
        }

        public static int ScoreWord(string s)
        {
            int score = 0;
            foreach (char n in s)
            {
                score += values[n];
            }

            score += LengthBonus(s);

            return score;
        }

        public static int LengthBonus(string s)
        {
            int score = 0;

            if (s.Length > 4)
            {
                score = 1;
            }
            if (s.Length > 5)
            {
                score = 2;
            }
            if (s.Length > 6)
            {
                score = 3;
            }
            if (s.Length > 7)
            {
                score = 4;
            }
            if (s.Length > 8)
            {
                score = 5;
            }
            if (s.Length > 9)
            {
                score = 6;
            }
            if (s.Length > 10)
            {
                score = 10;
            }
            if (s.Length > 11)
            {
                score = 12;
            }
            if (s.Length > 12)
            {
                score = 15;
            }
            if (s.Length > 13)
            {
                score = 17;
            }
            if (s.Length > 14)
            {
                score = 20;
            }
            if (s.Length > 15)
            {
                score = 30;
            }
            if (s.Length > 16)
            {
                score = 50;
            }
            if (s.Length > 17)
            {
                score = 100;
            }

            return score;
        }

        internal static int LetterValue(byte letter)
        {
            return values[Convert.ToChar(letter)];
        }

        internal static int ScoreWord(List<LetterProp> lplist)
        {
            int _score = 0;
            int wordMult = 1;
            string word = "";

            foreach (LetterProp lp in lplist)
            {
                string s = lp.ASCIIString.ToLower();
                word += s;

                _score += lp.GetLetterMult() * values[s[0]];
                wordMult += lp.GetWordMult();
            }
            return _score * wordMult + LengthBonus(word);
        }

        //internal static int ScoreWord(List<Button> buttonList)
        //{
        //    int _score = 0;
        //    int wordMult = 1;
        //    string word = "";

        //    foreach (Button b in buttonList)
        //    {
        //        string s = (b.Content as string).ToLower();
        //        word += s;

        //        LetterProp lp = b.DataContext as LetterProp;

        //        _score += lp.GetLetterMult() * values[s[0]];
        //        wordMult += lp.GetWordMult();
        //    }
        //    return _score * wordMult + LengthBonus(word);
        //}

        internal static int ScoreWordSimple(List<LetterProp> buttonList)
        {
            int _score = 0;

            foreach (LetterProp b in buttonList)
            {
                _score += values[b.ASCIIChar];
            }
            return _score;
        }

        internal static string ScoreWordString(List<LetterProp> lp_list)
        {
            string _scorestr = "";
            string wordMultTotal = "";
            int totalmult = 1;
            string word = "";

            foreach (LetterProp lp in lp_list)
            {
                //string s = (b.Content as string).ToLower();
                word += lp.ASCIIString;

                //LetterProp lp = b.DataContext as LetterProp;

                string s = lp.ASCIIString;
                _scorestr += s.ToUpper() + values[s[0]].ToString();

                int lettermult = lp.GetLetterMult();
                if (lettermult >= 2)
                {
                    _scorestr += "x" + lettermult.ToString();
                }
                _scorestr += " ";

                int wordmult = lp.GetWordMult();
                totalmult *= wordmult;
                if (wordmult > 1)
                {
                    wordMultTotal += lp.GetWordMult().ToString() + "x";
                }
            }

            if (totalmult > 1)
            {
                if (totalmult >= 2)
                {
                    return _scorestr + "x" + totalmult.ToString() + "[" + wordMultTotal + "]";
                }
                else
                {
                    return _scorestr + "x" + totalmult.ToString();
                }
            }

            if (LengthBonus(word) > 0)
            {
                return _scorestr + " BONUS(" + LengthBonus(word) + ")" + wordMultTotal;
            }
            else
            {
                return _scorestr + wordMultTotal;
            }
        }

        public static string GetWordTally(List<LetterProp> lp_list)
        {
            string curword = GetCurrentWord(lp_list);
            return ScoreWord(lp_list) + "=>" + EngLetterScoring.ScoreWordString(lp_list);
        }

        //public static string GetCurrentWord(List<Button> buttonList)
        //{
        //    string s = "";

        //    foreach (Button b in buttonList)
        //    {
        //        s += b.Content;
        //    }
        //    return s;
        //}

        public static string GetCurrentWord(List<LetterProp> lplist)
        {
            string s = "";

            foreach (LetterProp lp in lplist)
            {
                s += lp.ASCIIChar;
            }
            return s;
        }

        internal static int ScoreManna(List<LetterProp> lp_list)
        {
            int lettercnt = 0;
            int mannacnt = 0;

            foreach (LetterProp lp in lp_list)
            {
                if (lp.IsManna())
                {
                    mannacnt++;
                }
                lettercnt++;
            }

            return (lettercnt * mannacnt);

        }

        internal static bool IsConsonant(string content)
        {
            if(content == "A" || content == "E" || content == "I" || content == "O" || content == "U")
            {
                return false;
            }
            return true;         
        }

        internal static byte RandomVowel()
        {
            int vowelnum = r.Next(5);
            return (byte)Vowels[vowelnum];
        }
    }
}

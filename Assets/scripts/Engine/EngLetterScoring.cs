using System;
using System.Collections.Generic;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace WordSpell
{
    [XmlRootAttribute("StringList")]
    public class SerializableStringList
    {
        public List<string> list = new List<string>();

        public void Add(string s)
        {
            list.Add(s);
        }

        public int BinarySearch(string s)
        {
            return list.BinarySearch(s);
        }
    }

    [XmlRootAttribute("ValidDictionaryWords")]
    public class SerializableDictList
    {
        public List<string> list = new List<string>();

        public void Add(string s)
        {
            list.Add(s);
        }

        public int BinarySearch(string s)
        {
            return list.BinarySearch(s);
        }
    }

    public static class EngLetterScoring
    {
        const string PartialLookupCache = "EngDictALookupCache.xml";
        const string DictionaryCache = "EngDictACache.xml";
        private const string DictionaryTextName = "EngDictA.txt";

        public static string DictionaryTextPath
        {
            get
            {
                return Path.Combine( Application.streamingAssetsPath, DictionaryTextName);
            }
        }

        public static string DictionaryCachePath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, DictionaryCache);
            }
        }

        public static string PartialLookupCachePath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, PartialLookupCache);
            }
        }

        public static SerializableDictList DictionaryLookup
        {
            get
            {
                return dictionaryLookup;
            }

            set
            {
                dictionaryLookup = value;
            }
        }

        static public bool DictionaryCacheReady { get; private set; }
        static public bool DictionharyPartialCacheReady { get; private set; }
        static public bool DictionaryTextReady { get; private set; }

        public static readonly string[] LevelMsgs = 
        {
            "Level 2 introduces blue tiles which are worth double the letter value",
            "Beware of lava tiles, if they reach the bottom, the game is over",
            "Level 4 introduces double word tiles which double the word score",
            "Level 5 introduces tripple letter tiles which triple the leter value.",
            "Level 6 introduces triple word tiles which triple the word score",
        };

        public static readonly string[] IncorrectWordPhrases =
        {
            "Nice word...if you are a Martian :)  Please try again.",
            "Good try, but only earthbound languages will work.",
            "I'm sure you think that's a word, but it's not in my dictionary",
            "Creative, but that's not a word.",
            "Not every combination of letters spell a word.",
        };


        static public string GetIncorrectWordPhrase()
        {
            int r = WSGameState.Rnd.Next(IncorrectWordPhrases.Length);
            return IncorrectWordPhrases[r];
        }

        static char[] Vowels = { 'A', 'E', 'I', 'O', 'U' };
        public static char[] RequiredLettersForWord = { 'a', 'e', 'i', 'o', 'u', 'y' };

        static private SerializableDictList dictionaryLookup = new SerializableDictList();
        static private SerializableStringList PartialLookup = new SerializableStringList();

        static string DictionaryText;

        static public void LoadDictionaryData(WWW www)
        {
            try
            {
                if (string.IsNullOrEmpty(www.error))
                {
                    DictionaryLookup = LocalizationManager.XmlDeserializeFromWWW<SerializableDictList>(www);

                    Logging.StartDbg("ldd=" + dictionaryLookup.list.Count);
                }
                else
                {
                    Logging.StartDbg("ldd!" + www.url + ":::" + www.error);
                }

            }
            catch(Exception e)
            {
                Logging.StartDbg("ldd!!" + e.ToString());
            }

            DictionaryCacheReady = true;
        }

        static public void PartialLookupData(WWW www)
        {
            try
            {
                if (string.IsNullOrEmpty(www.error))
                {
                    PartialLookup = LocalizationManager.XmlDeserializeFromWWW<SerializableStringList>(www);

                    Logging.StartDbg("pld=" + PartialLookup.list.Count);
                }
                else
                {
                    Logging.StartDbg("pld!" + www.url + ":::" + www.error);
                }
            }
            catch (Exception e)
            {
                Logging.StartDbg("ldd!!" + e.ToString());
            }

            DictionharyPartialCacheReady = true;
        }

        static public void DictionaryTextData(WWW www)
        {
            if (string.IsNullOrEmpty(www.error))
            {
                DictionaryText = www.text;
            }
            else
            {
                Logging.StartDbg("dtd!" + www.url + ":::" + www.error);
            }

            DictionaryTextReady = true;
        }

        /// <summary>
        /// Checks to see if dictionary and partial cache is loaded properly, if not it rebuilds from text file.
        /// </summary>
        static public void ReloadDictionary()
        {

        }

        static public void LoadDictionary()
        {
            Logging.StartDbg("ld0", timestamp:true);

            //string filePath = Application.persistentDataPath + "/" + DictionaryCache;
            //LocalizationManager.AsyncLoadDictionary(filePath);

            if(dictionaryLookup.list.Count <= 0)
            {
                Logging.StartDbg("ld2", timestamp: true);
                RebuildDictionaryFromTextFile(DictionaryCachePath);
                Logging.StartDbg("ld3", timestamp: true);
            }

            Logging.StartDbg("Dictionary loaded " + dictionaryLookup.list.Count);

            // Is it cached already?
        //    if (File.Exists(filePath))
        //    {
        //        Logging.StartDbg("ld1", timestamp: true);
        //        try
        //        {
        //            XmlSerializer xs = new XmlSerializer(typeof(SerializableDictList));

        //            using (FileStream fs = new FileStream(filePath, FileMode.Open))
        //            {
        //                DictionaryLookup = (SerializableDictList)xs.Deserialize(fs);
        //            }
        //        }
        //        catch (System.Xml.XmlException)
        //        {
        //            Something went wrong, so let's rebuilld
        //            Logging.StartDbg("ld0!", timestamp: true);
        //            RebuildDictionaryFromTextFile(filePath);
        //            Logging.StartDbg("ld1!", timestamp: true);
        //        }
        //    }
        //    else
        //    {
        //        Logging.StartDbg("ld2", timestamp: true);
        //        RebuildDictionaryFromTextFile(filePath);
        //        Logging.StartDbg("ld3", timestamp: true);
        //    }

        //    Logging.StartDbg("ld4", timestamp: true);
        //    CreatePartialLookup();
        //    Logging.StartDbg("ldx", timestamp: true);
        }

        static public void RebuildDictionaryCache()
        {
            Logging.StartDbg("rd1", timestamp: true);
            string[] words = DictionaryText.Split('\n');
            Logging.StartDbg("rd2", timestamp: true);

            foreach (String rs in words)
            {
                string s = rs.TrimEnd();

                if (!s.Contains("'") && s.Length > 2 && s.IndexOfAny(RequiredLettersForWord) >= 0)
                {
                    DictionaryLookup.Add(s);
                }
            }
            Logging.StartDbg("rd3", timestamp: true);

            XmlSerializer xs = new XmlSerializer(typeof(SerializableDictList));

            Logging.StartDbg("rd4", timestamp: true);

            using (FileStream fs = new FileStream(DictionaryCachePath, FileMode.Create))
            {
                xs.Serialize(fs, DictionaryLookup);
            }
            Logging.StartDbg("rd5", timestamp: true);

        }

        static public void RebuildDictionaryFromTextFile(string filePath)
        {
            Logging.StartDbg("rdftf0", timestamp: true);
            TextAsset DictFile = Resources.Load(filePath) as TextAsset;
            if (DictFile != null)
            {
                Logging.StartDbg("rdftf1", timestamp: true);
                string[] words = DictFile.text.Split('\n');
                Logging.StartDbg("rdftf2", timestamp: true);

                foreach (String rs in words)
                {
                    string s = rs.TrimEnd();

                    if (!s.Contains("'") && s.Length > 2 && s.IndexOfAny(RequiredLettersForWord) >= 0)
                    {
                        DictionaryLookup.Add(s);
                    }
                }
                Logging.StartDbg("rdftf3", timestamp: true);

                XmlSerializer xs = new XmlSerializer(typeof(SerializableDictList));

                Logging.StartDbg("rdftf4", timestamp: true);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    xs.Serialize(fs, DictionaryLookup);
                }
                Logging.StartDbg("rdftf5", timestamp: true);
            }

            Logging.StartDbg("rdftfx", timestamp: true);
        }

        static public void CreatePartialLookup()
        {
            Logging.StartDbg("cpl0", timestamp: true);

            string filePath = Application.persistentDataPath + "/" + PartialLookupCache;
            // Is it cached already?
            if (File.Exists(filePath))
            {
                Logging.StartDbg("cpl1", timestamp: true);
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(SerializableStringList));

                    using(FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        PartialLookup = (SerializableStringList)xs.Deserialize(fs);
                    }
                }
                catch(System.Xml.XmlException)
                {
                    // Something went wrong, so let's rebuilld
                    Logging.StartDbg("cpl!");
                    BuildPartialLookup(filePath);
                    Logging.StartDbg("cpl2");
                }
            }
            else
            {
                Logging.StartDbg("cpl3", timestamp: true);
                BuildPartialLookup(filePath);
                Logging.StartDbg("cpl4", timestamp: true);
            }
        }

        static private void ReuildPartialLookupCache()
        {
            Logging.StartDbg("bpl0");
            // Build partial list for each unique letter combination.
            foreach (string s in DictionaryLookup.list)
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
            Logging.StartDbg("bpl1");

            //LocalizationManager.StoreFile(PartialLookupCachePath, LocalizationManager.XmlSerializeToString<SerializableStringList>(PartialLookup));

            XmlSerializer xs = new XmlSerializer(typeof(SerializableStringList));

            Logging.StartDbg("bpl2");

            using (FileStream fs = new FileStream(Path.Combine(Application.persistentDataPath, PartialLookupCache), FileMode.Create))
            {
                xs.Serialize(fs, PartialLookup);
            }
            Logging.StartDbg("bplx");
        }

        static private void BuildPartialLookup(string filePath)
        {
            Logging.StartDbg("bpl0");
            // Build partial list for each unique letter combination.
            foreach (string s in DictionaryLookup.list)
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
            Logging.StartDbg("bpl1");

            XmlSerializer xs = new XmlSerializer(typeof(SerializableStringList));

            Logging.StartDbg("bpl2");

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                xs.Serialize(fs, PartialLookup);
            }
            Logging.StartDbg("bplx");
        }

        static public string GetLevelMsg(int n)
        {
            if ((n-2) < LevelMsgs.Length)
            {
                return LevelMsgs[n-2];
            }
            else
            {
                return "";
            }
        }

        static public bool IsWord(string word)
        {
            return (DictionaryLookup.BinarySearch(word.ToLower()) >= 0);
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
                b = (byte)WSGameState.Rnd.Next('A', 'Z'+1);
            }
            else if (fl == WSGameState.FortuneLevel.Good)
            {
                int maxvalue = 10;

                int p = WSGameState.Rnd.Next(5);
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
                    b = (byte)WSGameState.Rnd.Next('A', 'Z'+1);
                } while (values[(char)b] >= maxvalue);
            }
            else
            {
                bool goodletter = false;

                if (WSGameState.Rnd.Next(10) < 8)
                {
                    goodletter = true;
                }

                do
                {
                    b = (byte)WSGameState.Rnd.Next('A', 'Z'+1);
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
            return EngLetterScoring.ScoreWordString(lp_list);
        }

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
            int vowelnum = WSGameState.Rnd.Next(5);
            return (byte)Vowels[vowelnum];
        }
    }
}

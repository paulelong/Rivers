using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordSpell;

namespace Assets.scripts.Engine
{
    public abstract class LetterScoring
    {
        public const string Version = "2.0.5";

        const string PartialLookupCache = "LookupCache.xml";
        const string DictionaryCache = "DictionaryCache.lst";

        public static string Intro1;
        public static string Intro2;

        public abstract void GlobalInit();
        public abstract void IsWord(string word);
        public abstract string ScoreWordString(List<LetterProp> lp_list);


        public abstract void LoadDictionary();

    }
}

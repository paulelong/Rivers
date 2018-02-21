using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WordSpell;

namespace GenDict
{
    class Program
    {
        static SerializableDictList DictionaryLookup = new SerializableDictList();
        static SerializableStringList PartialLookup = new SerializableStringList();


        static void Main(string[] args)
        {
            if(args.Length <= 0)
            {
                Console.WriteLine("Usage GenDict DictionaryText [directory]");
                Console.WriteLine(" creates GenDictCache.xml and GenDictLookupCache.xml");
            }
            else
            {
                if(File.Exists(args[0]))
                {
                    string root = Path.GetFileNameWithoutExtension(args[0]);

                    string loc;

                    if (args.Length > 1)
                    {
                        loc = args[1] + "\\";
                    }
                    else
                    {
                        loc = ".\\";
                    }

                    RebuildDictionaryCache(args[0], loc + root + "Cache.xml");

                    BuildPartialLookup(loc + root + "LookupCache.xml");

                    Console.WriteLine("Loaded " + DictionaryLookup.list.Count + " words into " + loc + root + "Cache.xml.");
                }
                else
                {
                    Console.WriteLine("Couldn't open file " + args[0]);
                }
            }
        }

        static void RebuildDictionaryCache(string dictpath, string xmlpath)
        {
            using (FileStream fs = new FileStream(dictpath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    string[] words = reader.ReadToEnd().Split('\n');

                    foreach (String rs in words)
                    {
                        string s = rs.TrimEnd();

                        if (!s.Contains("'") && s.Length > 2 && s.IndexOfAny(EngLetterScoring.RequiredLettersForWord) >= 0)
                        {
                            DictionaryLookup.Add(s);
                        }
                    }

                    XmlSerializer xsxml = new XmlSerializer(typeof(SerializableDictList));

                    using (FileStream fsxml = new FileStream(xmlpath, FileMode.Create))
                    {
                        xsxml.Serialize(fsxml, DictionaryLookup);
                    }
                }
            }
        }

        static private void BuildPartialLookup(string filePath)
        {
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

            XmlSerializer xs = new XmlSerializer(typeof(SerializableStringList));

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                xs.Serialize(fs, PartialLookup);
            }
        }
    }
}

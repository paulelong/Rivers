using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordSpell
{

    static class Logging
    {
        private static string DebugString = " ";
        private static string DebugString2 = " ";
        private static string lastPlayDbg = "";

        public static string StartDbgInfo
        {
            get { return DebugString; }
        }

        public static string PlayDbgInfo
        {
            get { return DebugString2; }
        }

        public static string NewestLog
        {
            get
            {
                return (DebugString + "\n" + lastPlayDbg);
            }
        }

        public static string LastPlayLog
        {
            get
            {
                return lastPlayDbg;
            }
        }

        public static void StartDbg(string s, char sep = ',', bool timestamp = false)
        {
            string dbgs;

            if(timestamp)
            {
                dbgs = "\n[" + DateTime.Now.ToString("mm:ss.ff") + "] " + s + "\n";
            }
            else
            {
                dbgs = s + sep;
            }

            DebugString += dbgs;
        }

        public static void PlayDbg(string s, char sep = ',', bool last = false, bool timestamp = false)
        {
            if (timestamp)
            {
                DebugString2 += "\n[" + DateTime.Now.ToString("mm:ss.ff") + "]" + s + "\n";
            }
            else
            {
                DebugString2 += s + sep;
            }

            if (last)
            {
                lastPlayDbg = DebugString2;
                DebugString2 = "";
            }
        }
    }
}

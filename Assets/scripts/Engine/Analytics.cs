using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;

namespace WordSpell
{
    class WSAnalytics
    {
        internal static void RecordAnalyticsGameOver(GameStats gs)
        {
            Analytics.CustomEvent("gameOver", new Dictionary<string, object>
            {
                { "score", gs.score },
                { "level", gs.level },
                { "mana", gs.mana },
                { "boardsize", gs.boardsize },
                { "numWords", gs.history.Count },
                { "awarded", gs.awarded.Count },
                { "spellsAttempted", gs.spellsAttempted },
                { "spellsCasted", gs.spellsCasted },
                { "spellsAborted", gs.spellsAborted },
                { "board", WSGameState.PrintGameBoard() },
            });
        }

        internal static void RecordAnalyticsLevelReached(GameStats gs)
        {
            Analytics.CustomEvent("levelReached", new Dictionary<string, object>
            {
                { "score", gs.score },
                { "level", gs.level },
                { "mana", gs.mana },
                { "boardsize", gs.boardsize },
                { "numWords", gs.history.Count },
                { "awarded", gs.awarded.Count },
                { "spellsAttempted", gs.spellsAttempted },
                { "spellsCasted", gs.spellsCasted },
                { "spellsAborted", gs.spellsAborted },
                { "board", WSGameState.PrintGameBoard() },
            });
        }

        internal static void RecordAnalyticsStartGame(GameStats gs, int seed)
        {
            Analytics.CustomEvent("startGame", new Dictionary<string, object>
            {
                { "boardsize", gs.boardsize },
                { "board", WSGameState.PrintGameBoard() },
                { "randomSeed", seed },
            });
        }

        internal static void RecordAnalyticsLoadTime(TimeSpan span)
        {
            Analytics.CustomEvent("loadTime", new Dictionary<string, object>
            {
                { "timespan", span.Seconds },
            });
        }

        public void DebugTest()
        {
            UnityEngine.Object[] gos = (UnityEngine.Object[])Resources.LoadAll("");
            foreach (UnityEngine.Object go in gos)
            {
                if (go.ToString().Length > 100)
                {
                    Logging.StartDbg(go.ToString().Substring(0, 20), '\n');
                }
                else
                {
                    Logging.StartDbg(go.ToString(), '\n');
                }
            }

            Logging.StartDbg("__");
            gos = (UnityEngine.Object[])Resources.LoadAll("Songs");
            foreach (UnityEngine.Object go in gos)
            {
                Logging.StartDbg(go.ToString(), '\n');
            }
        }

        internal static void EmailDev(string Ex1Str, string Ex2Str)
        {
            //email Id to send the mail to
            string email = "paulelong@outlook.com";
            //subject of the mail
            string subject = MyEscapeURL("WordSpell bug report " + Application.version);
            //body of the mail which consists of Device Model and its Operating System
            string body = MyEscapeURL("Please add an explantion of the move you just attempted.  For instance, I spelled a for letter word to get rid of a lava tile.  Try to add anything relevant, like a spell you just attempted.\n\n\n\n" +
             "________" +
             "\n\nPlease Do Not Modify This\n\n" +
             "Model: " + SystemInfo.deviceModel + "\n\n" +
                "OS: " + SystemInfo.operatingSystem + "\n\n" +
                "Version: " + Application.version + "\n\n" +
                "Startup Dbg: " + Logging.StartDbgInfo + "\n\n" +
                "Play Dbg: " + Logging.LastPlayLog + "\n\n" +
                "Ex1: " + Ex1Str + "\n\n" +
                "Ex2: " + Ex2Str + "\n\n" +
                //"Stats(" + GamePersistence.StatsText.Length + "): " + GamePersistence.StatsText + "\n\n" +
                "Game(" + GamePersistence.GameWords.Length + "): " + GamePersistence.GameWords + "\n\n" +
                "GameBoard:\n" + WSGameState.PrintGameBoard() +
             "________");
            //Open the Default Mail App
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        internal static string MyEscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WordSpell;
using System;
using System.Net.Mail;
using System.Net;

public class Board : MonoBehaviour
{
    #region Fields
    WSListBox TryListBox;
    WSListBox HistoryListBox;
    WSListBox SpellListBox;
    WSListBox AwardedSpellListBox;

    WSListBox BestWordListBox;
    WSListBox BestWordSimpleListBox;
    WSListBox HighScoresListBox;
    WSListBox LongestListBox;

    private float newFortuneScale;
    private float fortuneScale = 0f;

    string DebugString = "";
    string DebugString2 = "";

    string Ex1Str = "";
    string Ex2Str = "";

    private bool SpellCasted = false;
    private float LastActionTime;
    private string lastPlayDbg = "";

    #endregion Fields

    #region Constants
    const float gridYoff = -2.45f;
    const int numgrid = 9;
    const float inc = 1f; // (float)(size * scale_factor);
    const int half_offset = WSGameState.gridsize / 2;

    const string SpellNamePath = "TextPanel/Name";
    const string SpellCostPath = "TextPanel/Cost";
    const string SpellImagePath = "ButtonPanel/Image";

    const float fortuneChangeSpeed = .02f;
    private const float TimeTillHint = 200f;
    #endregion Constants

    // Passed in from Board scene
    #region Unity Objects
    public Transform LetterBoxPrefab;
    public Transform LetterSpeakerPrefab;
    public GameObject SelectPrefab;
    public Transform LavaLightPrefab;
    public Transform SpellPrefab;
    public Transform TextPrefab;

    // Canvas
    public GameObject ControlCanvas;
    public GameObject StartCanvas;
    public GameObject MsgCanvas;
    public GameObject OptionCanvas;
    public GameObject SpellCanvas;
    public GameObject InputCanvas;
    public GameObject SystemMenu;
    public GameObject SubmitButtonGO;
    public GameObject CastButton;
    public Camera BoardCam;

    // Where the letter grid goes
    public GameObject GameAreaPanel;

    public GameObject FortuneBar;

    public GameObject LevelText;
    public GameObject ScoreText;
    public GameObject ManaText;

    public GameObject TryList;
    public GameObject HistoryList;
    public GameObject SpelllList;
    public GameObject AwardedSpellList;

    public GameObject BestWordList;
    public GameObject BestWordSimpleList;
    public GameObject HighScoresList;
    public GameObject LongestList;

    public AudioClip SubmitWordSound;
    public AudioClip NewLevelSound;
    public AudioClip GameOverSound;
    public AudioClip SwapSound;
    public AudioClip SnipeSound;
    public AudioClip LavaSound;

    #endregion Unity Objects

    #region Init
    // Use this for initialization
    void Start()
    {

        //var gol = GameObject.FindGameObjectsWithTag("ScoreText");
        //foreach(GameObject go in gol)
        //{
        //    Debug.Log(go.name);
        //}
        try
        {
            StartDbg("S0");
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //        SetUserInfo(GamePersistence.TestPersistence());

            WSGameState.InitGameGlobal();
            StartDbg("S1");

            SetStoryInfo(EngLetterScoring.Intro0, EngLetterScoring.Intro1, EngLetterScoring.Intro2, EngLetterScoring.Intro3);

            StartDbg("S2");

            SetVersion(Application.version);

            TryListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            TryListBox.InitWSListBox(TryList, TextPrefab);
            HistoryListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            HistoryListBox.InitWSListBox(HistoryList, TextPrefab);
            SpellListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            SpellListBox.InitWSListBox(SpelllList, SpellPrefab);
            AwardedSpellListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            AwardedSpellListBox.InitWSListBox(AwardedSpellList, SpellPrefab);

            BestWordListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            BestWordListBox.InitWSListBox(BestWordList, TextPrefab);
            BestWordSimpleListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            BestWordSimpleListBox.InitWSListBox(BestWordSimpleList, TextPrefab);
            HighScoresListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            HighScoresListBox.InitWSListBox(HighScoresList, TextPrefab);
            LongestListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            LongestListBox.InitWSListBox(LongestList, TextPrefab);

            LocateCamera();

            HideSpellStuff();

            StartCanvas.SetActive(true);
            StartDbg("S3.2");

            LoadStats();
            StartDbg("S4");

            if (GamePersistence.SavedGameExists())
            {
                StartGame();
            }
            StartDbg("S5");
        }
        catch (Exception ex)
        {
            StartDbg("!1");
            ShowMsg(DebugString + "\nEXCEPTION 1\nPlease take screen shot (on iOS hold down power and press home button), to take a picture to send to me.  Exception is: " + ex.ToString(), true);
            Ex1Str += "EXCEPTION 1:\n" + ex.ToString();
        }

        ResetTimer();
        StartDbg("Sx");
    }

    void LocateCamera()
    {
        int w = Screen.width;
        int h = Screen.height;
        float dpi = Screen.dpi;

        float aspect = (float)w / (float)h;
        if (aspect > 0.65f)
        {
            aspect = 0.65f;
        }

        // Set the camera in the middle of the screen.
        // R code to calculate best fit line for aspect ratios
        //             s8  480/800  800/1280
        // ratios = c(.486,  .6,    .625)
        // sz     = c(9.5,    8,    7.4)
        // off    = c(-5,    -3.6,    -2.5)
        // off = c(17, 21.5,  22)

        // off_fit = lm(off ~ ratios)
        // sz_fit = lm(sz ~ ratios)

        float CamZ = (aspect * 23.54f) - 27.69f;
        if (CamZ > -13.4f)
        {
            CamZ = -13.4f;
        }

        CamZ = -16.5f;
        BoardCam.transform.position = new Vector3(0, gridYoff, CamZ);
        //BoardCam.orthographicSize = (aspect * -11.0f) + 15.23f;

        Debug.Log("pos: " + GameAreaPanel.transform.position);
        Debug.Log("local: " + GameAreaPanel.transform.localPosition);
    }

    public void EndGanme()
    {
        StartCoroutine(EndGameDelay());
    }

    public Transform NewTile(int i, int j, float newtilepos = 0)
    {
        // If it's a new tile, put it above the screen so animation can set it into place.

        Transform lbi = Instantiate(LetterBoxPrefab, new Vector3((i - half_offset) * inc, (j - half_offset + newtilepos) * inc, 0), Quaternion.identity);
        lbi.localScale *= inc;

        return lbi;
    }

    public Transform NewTile(int i, int j, LetterProp.TileTypes tt, float newtilepos = 0)
    {
        // If it's a new tile, put it above the screen so animation can set it into place.
        Transform lbi = null;

        switch (tt)
        {
            case LetterProp.TileTypes.Speaker:
                lbi = Instantiate(LetterSpeakerPrefab, new Vector3((i - half_offset) * inc, (j - half_offset + newtilepos) * inc, 0), Quaternion.identity);
                lbi.localScale *= inc;
                break;
            default:
                //lbi = Instantiate(LetterSpeakerPrefab, new Vector3((i - half_offset) * inc, (j - half_offset + newtilepos) * inc, 0), Quaternion.identity);
                lbi = Instantiate(LetterBoxPrefab, new Vector3((i - half_offset) * inc, (j - half_offset + newtilepos) * inc, 0), Quaternion.identity);
                lbi.localScale *= inc;
                break;
        }

        return lbi;
    }

    public void DestroyLetterObject(Transform _tf)
    {
        Destroy(_tf.gameObject);
    }

    public Transform NewLavaLight()
    {
        Transform lavalight = Instantiate(LavaLightPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        return lavalight;
    }

    public void HideSpellStuff()
    {
        CastButton.SetActive(false);
    }

    public void ShowSpellStuff()
    {
        CastButton.SetActive(true);
    }

    #endregion Init

    void LoadStats()
    {
        StartDbg("LS0");
        WSGameState.LoadStats();
        StartDbg("LS1");
        RefreshStats();
        StartDbg("LSx");
    }

    void RefreshStats()
    {
        StartDbg("RS1");
        HighScoresListBox.CreateList(WSGameState.BestGameScores);
        StartDbg("RS0");
        LongestListBox.CreateList(WSGameState.LongestWords, true);
        BestWordListBox.CreateList(WSGameState.BestWordScores, true);
        BestWordSimpleListBox.CreateList(WSGameState.BestWordScoresSimple, true);
        StartDbg("RSx");
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (WSGameState.CurrentLevel < 4 && WSGameState.GameInProgress && Time.realtimeSinceStartup - LastActionTime > TimeTillHint && !OptionCanvas.activeSelf)
            {
                ResetTimer();
                ShowOption("You seem stuck, do you want to me to find you a word?");
            }

            if (WSGameState.dbg && WSGameState.LetterPropGrid[5, 8].LetTF.position.y - 5f < .1)
            {
                WSGameState.dbg = false;
            }

            if (MsgCanvas.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    HideMsg();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (InputCanvas.activeSelf)
                    {
                        SelectLetterToChangeDone();
                    }
                    else
                    {
                        SubmitWord();
                    }
                }
            }

            if (newFortuneScale - fortuneScale > 0.01f)
            {
                fortuneScale += fortuneChangeSpeed;
                FortuneBar.transform.localScale = new Vector3(fortuneScale, 1, 1);
            }

            if (newFortuneScale - fortuneScale < -0.01f)
            {
                fortuneScale -= fortuneChangeSpeed;
                FortuneBar.transform.localScale = new Vector3(fortuneScale, 1, 1);
            }
        }
        catch (Exception ex)
        {
            StartDbg("!2");
            ShowMsg(DebugString2 + "\nEXCEPTION 2\nPlease take screen shot (on iOS hold down power and press home button), to take a picture to send to me.  Exception is: " + ex.ToString(), true);
            Ex2Str += "EXCEPTION2:\n" + ex.ToString();

        }
    }

    public void ResetTimer()
    {
        LastActionTime = Time.realtimeSinceStartup;
    }

    // Handlers
    #region Handlers

    public void OnMouseClick()
    {
        ResetTimer();

        HideMsg();

        if (SystemMenu.activeSelf)
        {
            SystemMenu.SetActive(false);
        }
    }

    public void OnApplicationQuit()
    {
        SaveGameState();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SaveGameState();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        SaveGameState();
    }

    void SaveGameState()
    {
        if (!StartCanvas.activeSelf && WSGameState.GameInProgress)
        {
            WSGameState.Save();
            WSGameState.SaveStats();
        }
    }

    // Button commands
    public void QuitApp()
    {
        WSGameState.Save();
        Application.Quit();
    }

    public void SubmitWord()
    {
        ResetTimer();
        WSGameState.SubmitWord();
    }

    public void ResetSubmitButton()
    {
        SetCurrentWord("Click on letters to spell a word");
        IndicateGoodWord(WSGameState.WordValidity.Garbage);
    }

    public void HamburgerMenu()
    {
        ResetTimer();
        if (SystemMenu.activeSelf)
        {
            SystemMenu.SetActive(false);
        }
        else
        {
            SystemMenu.SetActive(true);
        }
    }

    public void SaveGame()
    {
        WSGameState.Save();
        SystemMenu.SetActive(false);
    }

    public void QuitGame()
    {
        SystemMenu.SetActive(false);
        WSGameState.GameOver();
    }

    public void ResetApp()
    {
        SystemMenu.SetActive(false);
        WSGameState.GameOver();
        GamePersistence.ResetSavedData();
    }

    public void SendEmail2()
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
            "Startup Dbg: " + DebugString + "\n\n" +
            "Play Dbg: " + lastPlayDbg + "\n\n" +
            "Ex1: " + Ex1Str + "\n\n" +
            "Ex2: " + Ex2Str + "\n\n" +
            "Stats(" + GamePersistence.StatsText.Length + "): " + GamePersistence.StatsText + "\n\n" +
            "Game(" + GamePersistence.GameText.Length + "): " + GamePersistence.GameText + "\n\n" +
         "________");
        //Open the Default Mail App
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

    string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    public void SendEmail()
    {
        MailMessage mail = new MailMessage();
        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        mail.From = new MailAddress("paulelong@outlooik@gmail.com");
        mail.To.Add("g.amati@ucl.ac.uk");
        mail.Subject = "Prova";

        SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        SmtpServer.Port = 587;
        SmtpServer.Credentials = (ICredentialsByHost)CredentialCache.DefaultNetworkCredentials; //(ICredentialsByHost) new NetworkCredential("GMAILACCOUNT","PASSWORD");
        SmtpServer.EnableSsl = true;
        SmtpServer.Timeout = 20000;
        SmtpServer.UseDefaultCredentials = false;
        try
        {
            SmtpServer.Send(mail);
        }
        catch (SmtpException myEx)
        {
            Debug.Log("Expection: \n" + myEx.ToString());
        }
    }

    public void ShowOption(string text)
    {
        Text t = OptionCanvas.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
        RectTransform rt = MsgCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        t.text = text;

        OptionCanvas.SetActive(true);
    }

    public void OptionYes()
    {
        Spells.GetBestHint(10);
        OptionCanvas.SetActive(false);
        ResetTimer();
    }

    public void OptionNo()
    {
        OptionCanvas.SetActive(false);
        ResetTimer();
    }

    #endregion Handlers

    public void StartGame()
    {
        StartCanvas.SetActive(false);
        ControlCanvas.SetActive(true);

        WSGameState.InitNewGame();

        StartDbg("SG1");
        for (int i = 0; i < WSGameState.gridsize; i++)
        {
            for (int j = 0; j < WSGameState.gridsize; j++)
            {
                LetterProp lp = WSGameState.NewLetter(i, j);
                Transform lbi = NewTile(i, j, lp.TileType);
                lp.SetTransform(lbi);
            }
        }
        StartDbg("SG2");

        WSGameState.NewMusicTile();

        // Check if there is a saved game.
        WSGameState.Load();
        StartDbg("SGx");
    }

    IEnumerator EndGameDelay()
    {
        yield return new WaitForSeconds(1.5f);

        ShowMsg("Game Over");
        PlayEndGameSound();
        yield return new WaitForSeconds(3);
        HideMsg();

        RefreshStats();

        StartCanvas.SetActive(true);
    }

    public void StartDbg(string s)
    {
        DebugString += s + "-";
        SetUserInfo(DebugString + "\n" + DebugString2);
    }

    public void PlayDbg(string s, bool last = false)
    {
        DebugString2 += s + "-";
        SetUserInfo(DebugString + "\n" + DebugString2);
        if(last)
        {
            lastPlayDbg = DebugString2;
            DebugString2 = "";
        }
    }

    #region Controls

    // ----------------------------------------------------------
    // Status settings for Control UI element values

    public void ShowMsg(string text, bool bigmsg = false)
    {
        Text t = MsgCanvas.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
        RectTransform rt = MsgCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        t.text = text;
        if(bigmsg)
        {
            t.fontSize = 24;
        }
        else
        {
            t.fontSize = 40;
        }
        MsgCanvas.SetActive(true);
    }

    public void HideMsg()
    {
        MsgCanvas.SetActive(false);
    }

    public GameObject SelectLet(int i, int j, bool isMagic = false)
    {
        GameObject t = (GameObject)Instantiate(SelectPrefab, new Vector3((i - half_offset) * inc, (j - half_offset) * inc, 0.6f), Quaternion.identity);

        GameObject hl = t.transform.GetChild(0).gameObject;
        GameObject vt = t.transform.GetChild(1).gameObject;
        GameObject hr = t.transform.GetChild(2).gameObject;
        GameObject vb = t.transform.GetChild(3).gameObject;

        Material m;
        if(isMagic)
        {
            m = WSGameState.GetMagicMat();
        }
        else
        {
            m = WSGameState.GetFortuneColor();

        }
        hl.GetComponent<MeshRenderer>().material = m;
        vt.GetComponent<MeshRenderer>().material = m;
        hr.GetComponent<MeshRenderer>().material = m;
        vb.GetComponent<MeshRenderer>().material = m;

        return t;
    }

    public void DeselectLet(GameObject t)
    {
        t.SetActive(false);
        Destroy(t);
    }

    public void SetFortune(float scale, Material m)
    {
        newFortuneScale = scale;

        MeshRenderer mr = FortuneBar.GetComponent<MeshRenderer>();

        FortuneBar.GetComponent<MeshRenderer>().material = m;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    public void SetCurrentWord(string s)
    {
        UnityEngine.UI.Text t = SubmitButtonGO.GetComponentInChildren(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = s;
    }

    public void SetScore(string s)
    {
        UnityEngine.UI.Text t = ScoreText.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = s;
    }

    public void SetLevel(string s)
    {
        UnityEngine.UI.Text t = LevelText.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = s;
    }

    public void SetMana(string s)
    {
        UnityEngine.UI.Text t = ManaText.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = s;
    }

    public void AddHistory(string s)
    {
        // bugbug: why does the left side get cut off?  I'll add a space as a workaround.
        HistoryListBox.InsertText(" " + s);
    }

    public void ClearHistory()
    {
        HistoryListBox.Clear();
    }

    public void AddTryList(string s)
    {
        TryListBox.AddText(" " + s);
    }

    public void ClearTryList()
    {
        TryListBox.Clear();
    }

    public void SetUserInfo(string s)
    {
        SystemMenu.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = s;
    }

    public void SetVersion(string s)
    {
        SystemMenu.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = s;
    }

    public void SetStoryInfo(string s0, string s1, string s2, string s3)
    {
        StartCanvas.transform.Find("Back0/Intro0").GetComponent<Text>().text = s0;
        StartCanvas.transform.Find("Back1/Intro1").GetComponent<Text>().text = s1;
        StartCanvas.transform.Find("Back2/Intro2").GetComponent<Text>().text = s2;
        StartCanvas.transform.Find("Back3/Intro3").GetComponent<Text>().text = s3;
    }

    #endregion Controls

    #region Spells

    // Spell related stuff
    public void AddSpellList(WSListBox spellbox, SpellInfo si, bool awarded = false)
    {
        Transform item = spellbox.Add();
        item.transform.name = si.FriendlyName;

        UnityEngine.UI.Text s = item.Find(SpellNamePath).GetComponent<UnityEngine.UI.Text>();
        s.text = si.FriendlyName;

        UnityEngine.UI.Text c = item.Find(SpellCostPath).GetComponent<UnityEngine.UI.Text>();
        if (!awarded)
        {
            c.text = si.MannaPoints.ToString();
        }
        else
        {
            c.text = "";
        }

        UnityEngine.UI.Image i = item.Find(SpellImagePath).GetComponent<UnityEngine.UI.Image>();
        i.sprite = si.Image;

        Button b = item.Find(SpellImagePath).GetComponent<Button>();
        b.onClick.AddListener(delegate { SelectSpell(si.FriendlyName, awarded); } );
        if(!awarded && WSGameState.EnoughMana(si.MannaPoints))
        {
            b.enabled = false;
        }
    }

    public void ClearSpellList(WSListBox spellbox)
    {
        spellbox.Clear();
    }

    public void ShowSpells()
    {
        if (!SpellCasted)
        {
            ClearSpellList(SpellListBox);

            foreach (SpellInfo si in Spells.AvailableSpells)
            {
                AddSpellList(SpellListBox, si);
            }

            ClearSpellList(AwardedSpellListBox);

            foreach (SpellInfo si in WSGameState.AwardedSpells)
            {
                AddSpellList(AwardedSpellListBox, si, true);
            }

            SpellCanvas.SetActive(true);
        }
        else
        {
            Spells.AbortSpell();

            Text t = CastButton.transform.GetChild(0).GetComponent<Text>();
            t.text = "Cast";

            SpellCasted = false;
        }
    }

    public void CancelSpells()
    {
        SpellCanvas.SetActive(false);
    }

    void SelectSpell(string spellName, bool awarded)
    {
        SpellCanvas.SetActive(false);

        // Awarded spells need to be removed from the list
        Spells.ReadySpell(spellName, awarded, SpellSucceded);

        // So spell can be canceled, change button text
        if(Spells.SpellReady())
        {
            Text t = CastButton.transform.GetChild(0).GetComponent<Text>();
            t.text = "Abort";

            SpellCasted = true;
        }
    }

    public void SelectLetterToChange()
    {
        Text textField = InputCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
        textField.text = "Input a letter, then OK, to change the tile you selected.";
        //Text t = InputCanvas.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(2).GetComponent<Text>();
        //t.text = "";

        InputCanvas.SetActive(true);

        InputField inf = InputCanvas.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<UnityEngine.UI.InputField>();
        EventSystem.current.SetSelectedGameObject(inf.gameObject);
        inf.text = "";
    }

    public void SelectLetterToChangeDone()
    {
        InputCanvas.SetActive(false);

        Text t = InputCanvas.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(2).GetComponent<Text>();

        Spells.CastSpell(t.text.ToUpper());
    }

    void SpellSucceded()
    {
        WSGameState.ChangeManna(Spells.LastManaCost);

        SpellInfo si = WSGameState.AwardedSpells.Find(x => (x.spellType == Spells.LastSuccessfulSpell.spellType));
        WSGameState.AwardedSpells.Remove(si);

        Text t = CastButton.transform.GetChild(0).GetComponent<Text>();
        t.text = "Cast";

        SpellCasted = false;
    }
    #endregion Spells

    public void IndicateGoodWord(WSGameState.WordValidity wordStatus)
    {
        var theColor = SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors;

        switch (wordStatus)
        {
            case WSGameState.WordValidity.Garbage:
                theColor.normalColor = Color.gray;
                theColor.highlightedColor = Color.gray;
                SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;
            case WSGameState.WordValidity.Word:
                theColor.normalColor = new Color32(72, 234, 94, 255);
                theColor.highlightedColor = new Color32(72, 234, 94, 255);
                SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;
            case WSGameState.WordValidity.UsedWord:
                theColor.normalColor = Color.yellow;
                theColor.highlightedColor = Color.yellow;
                SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;

        }
    }

    // Sounds to play
    #region SoundFX
    public void ScoreWordSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(SubmitWordSound);
    }

    public void LevelSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(NewLevelSound, 0.4f);
    }

    public void PlayEndGameSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(GameOverSound);
    }

    public void PlaySwapSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(SwapSound);
    }

    public void PlaySnipeSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(SnipeSound);
    }

    public void PlayLavaSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.PlayOneShot(LavaSound);
    }

    #endregion SoundFX

}

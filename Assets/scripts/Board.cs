﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WordSpell;
using System;
//using System.Net.Mail;
//using System.Net;

public class Board : MonoBehaviour, IEventSystemHandler
{
    #region Fields
    WSListBox TryListBox;
    WSListBox HistoryListBox;
    WSListBox SpellListBox;
    WSListBox AwardedSpellListBox;
    WSListBox SpellsListBox;

    WSListBox BestWordListBox;
    WSListBox BestWordSimpleListBox;
    WSListBox HighScoresListBox;
    WSListBox LongestListBox;

    private float newFortuneScale;
    private float fortuneScale = 0f;

    string Ex1Str = "";
    string Ex2Str = "";

    private bool SpellCasted = false;
    private float LastActionTime;

    int trigger_nf2 = Animator.StringToHash("nf2");
    int trigger_nf3 = Animator.StringToHash("nf3");

    int CastButtonBlinkTrigger = Animator.StringToHash("Blink");
    int CastButtonNormalTrigger = Animator.StringToHash("StopBlink");

    //private int half_offset = WSGameState.Gridsize / 2;


    #endregion Fields

    #region Constants
    const float gridYoff = -3f;
    //const float inc = (float)WSGameState.maxgridsize / (float)WSGameState.gridsize; // (float)(size * scale_factor);

    const string SpellNamePath = "TextPanel/Name";
    const string SpellCostPath = "TextPanel/Cost";
    const string SpellImagePath = "ButtonPanel/Image";
    const string SpellSelPath = "ButtonPanel/Selector";

    private const float FORTUNE_CHANGE_SPEED = 4f;
    private const float TIME_TILL_HINT = 200f;
    private const float FORUTUNE_BAR_SCALE = 17.5f;
    private const float FORTUNE_BAR_SIZE = 55;

    public const string SubmitButtonDefaultKey = "SubmitButtonDefault";
    public const string SubmitButtonAlreadyUsedKey = "SubmitButtonAlreadyUsed";
    public const string SubmitButtonSumbitKey = "SubmitButtonSubmit";


    #endregion Constants

    // Passed in from Board scene
    #region Unity Objects
    //public TextAsset dict;
    public Transform LetterBoxPrefab;
    public Transform LetterSpeakerPrefab;
    public GameObject NotesPrefab;
    public GameObject SelectPrefab;
    public Transform LavaLightPrefab;
    public Transform SpellPrefab;
    public Transform TextPrefab;

    // Canvas
    public GameObject ControlCanvas;
    public GameObject StartCanvas;
    public GameObject MsgCanvas;
    public GameObject MagicCanvas;
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
    //public GameObject Eff;

    public GameObject TryList;
    public GameObject HistoryList;
    public GameObject SpelllList;
    public GameObject AwardedSpellList;
    public GameObject SpellsList;

    public GameObject BestWordList;
    public GameObject BestWordSimpleList;
    public GameObject HighScoresList;
    public GameObject LongestList;

    public GameObject MenuHelpIntro0;
    public GameObject MenuHelpIntro1;
    public GameObject StartHelpIntro0;
    public GameObject StartHelpIntro1;

    public AudioClip SubmitWordSound;
    public AudioClip NewLevelSound;
    public AudioClip GameOverSound;
    public AudioClip SwapSound;
    public AudioClip SnipeSound;
    public AudioClip LavaSound;

    public GameObject ContrastPanel;

    // Particle System
    public ParticleSystem MagicParticles;
    public ParticleSystem Horray;

    private string NextSpellName = "";
    private bool NextSpellAwardState = false;

    public bool blinkIsOn { get; private set; }

    #endregion Unity Objects

    #region Init
    // Use this for initialization
    private void Start()
    {
        try
        {
            Logging.StartDbg("S0", timestamp: true);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //        SetUserInfo(GamePersistence.TestPersistence());

            WSGameState.InitGameGlobal();
            Logging.StartDbg("S1", timestamp: true);

            //SetStoryInfo(EngLetterScoring.Intro0, EngLetterScoring.Intro1, EngLetterScoring.Intro2, EngLetterScoring.Intro3);

            Logging.StartDbg("S2", timestamp: true);

            SetVersion(Application.version);

            TryListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            TryListBox.InitWSListBox(TryList, TextPrefab);
            HistoryListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            HistoryListBox.InitWSListBox(HistoryList, TextPrefab);
            SpellListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            SpellListBox.InitWSListBox(SpelllList, SpellPrefab);
            AwardedSpellListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            AwardedSpellListBox.InitWSListBox(AwardedSpellList, SpellPrefab);

            SpellsListBox = ScriptableObject.CreateInstance(typeof(WSListBox)) as WSListBox;
            SpellsListBox.InitWSListBox(SpellsList, SpellPrefab);


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
            ControlCanvas.SetActive(false);
            Logging.StartDbg("S3.2", timestamp: true);

            GamePersistence.LoadSavedGameData();

            LoadStats();
            Logging.StartDbg("S4", timestamp: true);

            if (GamePersistence.SavedGameExists())
            {
                StartCanvas.SetActive(false);
                ControlCanvas.SetActive(true);

                WSGameState.InitNewGame();

                WSGameState.Load();

                WSGameState.SetPanelSize(ContrastPanel);
            }
            Logging.StartDbg("S5", timestamp: true);

            RefreshSpells();

            //DebugTest();
        }
        catch (Exception ex)
        {
            Logging.StartDbg("!1");
            ShowMsg(Logging.NewestLog + "\nEXCEPTION 1\nPlease take screen shot (on iOS hold down power and press home button), to take a picture to send to me.  Exception is: " + ex.ToString(), true);
            Ex1Str += "EXCEPTION 1:\n" + ex.ToString();
        }

        ResetTimer();
        Logging.StartDbg("Sx", timestamp: true);
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
    }

    public void EndGameAction(string reason)
    {
        ResetSubmitButton();
        StartCoroutine(EndGameDelay(reason));
    }

    public Transform NewTile(int i, int j, float newtilepos = 0)
    {
        // If it's a new tile, put it above the screen so animation can set it into place.

        Transform lbi = Instantiate(LetterBoxPrefab, new Vector3((i - WSGameState.HalfOffset) * WSGameState.GridScale, (j - WSGameState.HalfOffset + newtilepos) * WSGameState.GridScale, 0), Quaternion.identity);
        lbi.localScale *= WSGameState.GridScale;

        return lbi;
    }

    public Transform NewTile(int i, int j, LetterProp.TileTypes tt, float newtilepos = 0)
    {
        // If it's a new tile, put it above the screen so animation can set it into place.
        Transform lbi = null;

        switch (tt)
        {
            case LetterProp.TileTypes.Speaker:
                lbi = Instantiate(LetterSpeakerPrefab, new Vector3((i - WSGameState.HalfOffset) * WSGameState.GridScale, (j - WSGameState.HalfOffset + newtilepos) * WSGameState.GridScale, 0), Quaternion.identity);
                AttachNotes(lbi);
                break;
            default:
                //lbi = Instantiate(LetterSpeakerPrefab, new Vector3((i - WSGameState.HalfOffset) * WSGameState.ScaleSize, (j - WSGameState.HalfOffset + newtilepos) * WSGameState.ScaleSize, 0), Quaternion.identity);
                lbi = Instantiate(LetterBoxPrefab, new Vector3((i - WSGameState.HalfOffset) * WSGameState.GridScale, (j - WSGameState.HalfOffset + newtilepos) * WSGameState.GridScale, 0), Quaternion.identity);
                break;
        }

        lbi.transform.localScale *= WSGameState.GridScale;

        return lbi;
    }

    private void AttachNotes(Transform lbi)
    {
        GameObject notes = Instantiate(NotesPrefab, new Vector3(0, 0, 0), LetterSpeakerPrefab.transform.rotation, lbi);
        notes.transform.position = lbi.transform.position - new Vector3(.1f, .2f, -0.1f);

        GameObject notes2 = Instantiate(NotesPrefab, new Vector3(0, 0, 0), LetterSpeakerPrefab.transform.rotation, lbi);
        notes2.transform.position = lbi.transform.position - new Vector3(0, .2f, -0.1f);
        Animator na2 = notes2.transform.GetChild(0).transform.GetComponent<Animator>();
        na2.SetTrigger(trigger_nf2);

        GameObject notes3 = Instantiate(NotesPrefab, new Vector3(0, 0, 0), LetterSpeakerPrefab.transform.rotation, lbi);
        notes3.transform.position = lbi.transform.position - new Vector3(.2f, .2f, -0.1f);
        Animator na3 = notes3.transform.GetChild(0).transform.GetComponent<Animator>();
        na3.SetTrigger(trigger_nf3);
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
        //CastButton.SetActive(false);
        CastButton.GetComponent<Button>().interactable = false;
    }

    public void ShowSpellStuff()
    {
        //CastButton.SetActive(true);
        CastButton.GetComponent<Button>().interactable = true;
    }

    void LoadStats()
    {
        Logging.StartDbg("LS0");
        WSGameState.LoadStats();
        Logging.StartDbg("LS1");
        RefreshStats();
        Logging.StartDbg("LSx");
    }

    void RefreshStats()
    {
        Logging.StartDbg("RS0");
        if (WSGameState.BestGameScores != null)
        {
            HighScoresListBox.CreateList(WSGameState.BestGameScores);
        }
        else
        {
            Logging.StartDbg("RS0!");
        }

        Logging.StartDbg("RS1");
        if (WSGameState.LongestWords != null)
        {
            LongestListBox.CreateList(WSGameState.LongestWords, true);
        }
        else
        {
            Logging.StartDbg("RS1!");
        }

        Logging.StartDbg("RS2");
        if (WSGameState.BestWordScores != null)
        {
            BestWordListBox.CreateList(WSGameState.BestWordScores, true);
        }
        else
        {
            Logging.StartDbg("RS2!");
        }

        Logging.StartDbg("RS3");
        if (WSGameState.BestWordScoresSimple != null)
        {
            BestWordSimpleListBox.CreateList(WSGameState.BestWordScoresSimple, true);
        }
        else
        {
            Logging.StartDbg("RS3!");
        }

        Logging.StartDbg("RSx");
    }

    public void StartBig()
    {
        StartGame(9);
    }

    public void StartSmall()
    {
        StartGame(7);
    }

    private void StartGame(int _gridsize)
    {
        StartCanvas.SetActive(false);
        ControlCanvas.SetActive(true);

        WSGameState.InitNewGame();
        WSGameState.CreateNewBoard(_gridsize);

        Logging.StartDbg("SG1");

        WSGameState.NewMusicTile();

        ResetTimer();

        RefreshSpells();

        // Calculate Panel for background contrast size
        WSGameState.SetPanelSize(ContrastPanel);

        // Load save game data
        //WSGameState.LoadGame();
        Logging.StartDbg("SGx");
    }

    #endregion Init

    #region Main

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (WSGameState.CurrentLevel < 4 && WSGameState.GameInProgress && Time.realtimeSinceStartup - LastActionTime > TIME_TILL_HINT && !OptionCanvas.activeSelf)
            {
                ResetTimer();
                ShowOption(LocalizationManager.instance.GetLocalizedValue("Stuck"));
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
                fortuneScale += FORTUNE_CHANGE_SPEED * Time.deltaTime;
                FortuneBar.transform.localScale = new Vector3(fortuneScale, FORTUNE_BAR_SIZE, FORTUNE_BAR_SIZE);
            }

            if (newFortuneScale - fortuneScale < -0.01f)
            {
                fortuneScale -= FORTUNE_CHANGE_SPEED * Time.deltaTime;
                FortuneBar.transform.localScale = new Vector3(fortuneScale, FORTUNE_BAR_SIZE, FORTUNE_BAR_SIZE);
            }
        }
        catch (Exception ex)
        {
            Logging.StartDbg("!2");
            ShowMsg(Logging.NewestLog + "\nEXCEPTION 2\nPlease take screen shot (on iOS hold down power and press home button), to take a picture to send to me.  Exception is: " + ex.ToString(), true);
            Ex2Str += "EXCEPTION2:\n" + ex.ToString();

        }
    }

    public void ResetTimer()
    {
        Logging.PlayDbg("rtm=" + LastActionTime.ToString(), timestamp: true);
        LastActionTime = Time.realtimeSinceStartup;
    }

    #endregion Main

    // Handlers
    #region UIHandlers

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
        ResetTimer();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        SaveGameState();
        ResetTimer();
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
        SetCurrentWord(LocalizationManager.instance.GetLocalizedValue(SubmitButtonDefaultKey));
        IndicateGoodWord(WSGameState.WordValidity.Garbage);
    }

    public void HelpMenu()
    {
        ResetTimer();
        if (SystemMenu.activeSelf)
        {
            SystemMenu.SetActive(false);
        }
        else
        {
            SetUserInfo(Logging.NewestLog);

            SystemMenu.SetActive(true);
        }
    }

    public void HelpMenuClose()
    {
        SystemMenu.SetActive(false);
    }

    public void SaveGame()
    {
        WSGameState.Save();
        SystemMenu.SetActive(false);
    }

    public void QuitGame()
    {
        SystemMenu.SetActive(false);
        
        WSGameState.GameOver(WSGameState.GameEndReasons.USER_ENDED);
    }

    public void ResetApp()
    {
        SystemMenu.SetActive(false);
        SpellCanvas.SetActive(false);

        WSGameState.GameOver(WSGameState.GameEndReasons.USER_ENDED);
        GamePersistence.ResetSavedData();
    }

    public void ShowOption(string text)
    {
        Text t = OptionCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
        //RectTransform rt = OptionCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
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

    public void IndicateGoodWord(WSGameState.WordValidity wordStatus)
    {
        //var theColor = SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors;
        var test = SubmitButtonGO.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();

        switch (wordStatus)
        {
            case WSGameState.WordValidity.Garbage:
                test.color = new Color32(200, 200, 200, 255);
                //theColor.normalColor = Color.gray;
                //theColor.highlightedColor = Color.gray;
                //SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;
            case WSGameState.WordValidity.Word:
                test.color = new Color32(72, 234, 94, 255);
                //theColor.normalColor = new Color32(72, 234, 94, 255);
                //theColor.highlightedColor = new Color32(72, 234, 94, 255);
                //SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;
            case WSGameState.WordValidity.UsedWord:
                test.color = Color.yellow;
                //theColor.normalColor = Color.yellow;
                //theColor.highlightedColor = Color.yellow;
                //SubmitButtonGO.GetComponent<UnityEngine.UI.Button>().colors = theColor;
                break;

        }
    }

    IEnumerator EndGameDelay(string msg)
    {
        yield return new WaitForSeconds(1.5f);

        ShowMsg("Game Over\n\n" + msg);
        PlayEndGameSound();
        yield return new WaitForSeconds(5);
        HideMsg();

        RefreshStats();

        ControlCanvas.SetActive(false);
        StartCanvas.SetActive(true);
    }
    
    #endregion IOHandlers
        
    #region Controls

    // ----------------------------------------------------------
    // Status settings for Control UI element values

    public void ShowMsg(string text, bool bigmsg = false)
    {
        Text t = MsgCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();
       // RectTransform rt = MsgCanvas.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        t.text = text;

        MsgCanvas.SetActive(true);
    }

    public void HideMsg()
    {
        MsgCanvas.SetActive(false);
    }

    public void ShowMagic(string text)
    {
        Text t = MagicCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>();

        t.text = text;
        MagicCanvas.SetActive(true);

        SubmitButtonGO.transform.GetComponent<Button>().enabled = false;
    }

    public void HideMagic()
    {
        SubmitButtonGO.transform.GetComponent<Button>().enabled = true;
        MagicCanvas.SetActive(false);
    }

    public void UpdateMagicPic(Sprite s)
    {
        Image i = MagicCanvas.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<UnityEngine.UI.Image>();

        i.sprite = s;
    }

    public GameObject SelectLet(LetterProp lp, bool isMagic = false)
    {
        Material m;

        if (isMagic)
        {
            m = WSGameState.GetMagicMat();
        }
        else
        {
            int wordscore;
            if(EngLetterScoring.IsWord(WSGameState.GetCurrentWord()))
            {
                wordscore = WSGameState.ScoreWord();
            }
            else
            {
                wordscore = 0;
            }

            m = WSGameState.GetFortuneColor(wordscore);
        }

        // Does it have a selector already?
        if (lp.SelectorObject == null)
        { 
            lp.SelectorObject = Instantiate(SelectPrefab, new Vector3((lp.I - WSGameState.HalfOffset) * WSGameState.GridScale, (lp.J - WSGameState.HalfOffset) * WSGameState.GridScale, 0.65f), Quaternion.identity);
            lp.SelectorObject.transform.localScale *= WSGameState.GridScale;
        }

        GameObject hl = lp.SelectorObject.transform.GetChild(0).gameObject;
        GameObject vt = lp.SelectorObject.transform.GetChild(1).gameObject;
        GameObject hr = lp.SelectorObject.transform.GetChild(2).gameObject;
        GameObject vb = lp.SelectorObject.transform.GetChild(3).gameObject;

        hl.GetComponent<MeshRenderer>().material = m;
        vt.GetComponent<MeshRenderer>().material = m;
        hr.GetComponent<MeshRenderer>().material = m;
        vb.GetComponent<MeshRenderer>().material = m;

        return lp.SelectorGO;
    }

    public GameObject SelectLet(int i, int j, bool isMagic = false)
    {
        Material m;

        if(isMagic)
        {
            m = WSGameState.GetMagicMat();
        }
        else
        {
            m = WSGameState.GetFortuneColor(WSGameState.ScoreWord());
        }

        GameObject t;

        t = Instantiate(SelectPrefab, new Vector3((i - WSGameState.HalfOffset) * WSGameState.GridScale, (j - WSGameState.HalfOffset) * WSGameState.GridScale, 0.65f), Quaternion.identity);
        t.transform.localScale *= WSGameState.GridScale;

        GameObject hl = t.transform.GetChild(0).gameObject;
        GameObject vt = t.transform.GetChild(1).gameObject;
        GameObject hr = t.transform.GetChild(2).gameObject;
        GameObject vb = t.transform.GetChild(3).gameObject;

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
        newFortuneScale = scale * FORUTUNE_BAR_SCALE;

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

    public void SetEff(float eff)
    {
        //UnityEngine.UI.Text t = Eff.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        //t.text = string.Format("{0:0.00}", eff);
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
        SystemMenu.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = s;
    }

    public void SetVersion(string s)
    {
        SystemMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = s;
    }

    public void SetStoryInfo(string s0, string s1, string s2, string s3)
    {
        StartCanvas.transform.Find("Back0/Intro0").GetComponent<Text>().text = s0;
        StartCanvas.transform.Find("Back1/Intro1").GetComponent<Text>().text = s1;
        StartCanvas.transform.Find("Back2/Intro2").GetComponent<Text>().text = s2;
        StartCanvas.transform.Find("Back3/Intro3").GetComponent<Text>().text = s3;

        //StartHelpIntro0.GetComponent<Text>().text = s0;
        StartHelpIntro1.GetComponent<Text>().text = s1;
        MenuHelpIntro0.GetComponent<Text>().text = s0;
        MenuHelpIntro1.GetComponent<Text>().text = s1;
    }

    #endregion Controls

    #region Spells

    public void AddSpells(WSListBox spellbox, SpellInfo si, bool awarded = false)
    {
        Transform item = spellbox.Add();
        item.transform.name = si.FriendlyName;

        // Z position seems to get set random value, setting it to -3 so spells show up.
        Vector3 t = item.transform.localPosition;
        t.z = -3f;
        item.transform.localPosition = t;


        UnityEngine.UI.Text s = item.Find(SpellNamePath).GetComponent<UnityEngine.UI.Text>();
        s.text = si.FriendlyName;

        UnityEngine.UI.Text c = item.Find(SpellCostPath).GetComponent<UnityEngine.UI.Text>();
        if (!awarded)
        {
            c.text = si.MannaPoints.ToString() + " " + LocalizationManager.instance.GetLocalizedValue("Mana");
        }
        else
        {
            c.text = LocalizationManager.instance.GetLocalizedValue("Free");
        }

        UnityEngine.UI.Image i = item.Find(SpellImagePath).GetComponent<UnityEngine.UI.Image>();
        i.sprite = si.Image;

        Button b = item.Find(SpellImagePath).GetComponent<Button>();
        b.onClick.AddListener(delegate { SelectSpell(si.FriendlyName, awarded); });

        // Add deselect trigger
        EventTrigger trigger = b.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Deselect;
        entry.callback = new EventTrigger.TriggerEvent();
        UnityEngine.Events.UnityAction<BaseEventData> call = new UnityEngine.Events.UnityAction<BaseEventData>(DeselectSpell);
        entry.callback.AddListener(call);
        trigger.triggers.Add(entry);

        if (!awarded && WSGameState.EnoughMana(si.MannaPoints))
        {
            b.enabled = false;
        }
    }

    public void RefreshSpells()
    {
        ClearSpellList(SpellsListBox);

        foreach (SpellInfo si in Spells.AvailableSpells)
        {
            AddSpells(SpellsListBox, si);
        }

        foreach (SpellInfo si in WSGameState.AwardedSpells)
        {
            AddSpells(SpellsListBox, si, true);
        }
    }

    public void ClearSpellList(WSListBox spellbox)
    {
        foreach(Transform t in spellbox.ListboxObjects)
        {
            Button b = t.Find(SpellImagePath).GetComponent<Button>();
            EventTrigger trigger = b.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.callback.RemoveAllListeners();
        }

        spellbox.Clear();
    }

    public void WandButtonPressed()
    {
        WSGameState.boardScript.HideMagic();
        Spells.CastSpell2();
        BlinkCastButton(false);
    }

    public void CancelSpell()
    {
        HideMagic();
        SpellCasted = false;
        //ShowCancelCast(SpellCasted);
        Spells.AbortSpell();
        PlayMagicParticle(SpellCasted);

        BlinkCastButton(false);
    }

    void SelectSpell(string spellName, bool awarded)
    {
        Debug.Log("Spell:" + spellName);
        WSGameState.NumAttempted++;

        NextSpellName = spellName;
        NextSpellAwardState = awarded;

        StartSpell();

        BlinkCastButton(true);
    }

    public void SelectSpell(BaseEventData data)
    {
        Debug.Log(data.ToString());
    }

    public void DeselectSpell(BaseEventData data)
    {
        Debug.Log("But deselect");

        //BlinkCastButton(false);
    }

    public void OnSelect()
    {
        //base.OnSelect(eventData);
        UnityEngine.Debug.Log("Selected");
    }

    void StartSpell()
    {

        WSGameState.NumCasted++;

        string spellName = NextSpellName;
        bool awarded = NextSpellAwardState;

        //BlinkCastButton(false);

        // Awarded spells need to be removed from the list
        Spells.ReadySpell(spellName, awarded, SpellSucceded);

        // So spell can be canceled, change button text
        if (Spells.SpellReady())
        {
            //SetSpellButton(false);

            SpellCasted = true;
           // ShowCancelCast(true);

            PlayMagicParticle(SpellCasted);
        }
    }

    public void ShowCancelCast(bool enable)
    {
        Text t = CastButton.transform.GetChild(0).GetComponent<Text>();

        if (enable)
        {
            t.enabled = true;            
        }
        else
        {
            t.enabled = false;
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

        RefreshSpells();

        //SetSpellButton(true);
        //ShowCancelCast(true);

        SpellCasted = false;
        PlayMagicParticle(SpellCasted);

        BlinkCastButton(false);
    }

    public void PlayMagicParticle(bool play)
    {
        if (play)
        {
            MagicParticles.Play();
        }
        else
        {
            MagicParticles.Stop();
        }
    }

    void BlinkCastButton(bool blink=true)
    {
        if(blink && !blinkIsOn)
        {
            Animator a = CastButton.transform.GetChild(1).GetComponent<Animator>();
            a.SetTrigger(CastButtonBlinkTrigger);
            blinkIsOn = true;
            Debug.Log("blink on");
        }
        else if(!blink && blinkIsOn)
        {
            Animator a = CastButton.transform.GetChild(1).GetComponent<Animator>();
            a.SetTrigger(CastButtonNormalTrigger);
            blinkIsOn = false;
            Debug.Log("blink off");
        }
    }

    #endregion Spells
    
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
        Horray.Play();
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

    public void SendEmail2()
    {
        WSAnalytics.EmailDev(Ex1Str, Ex2Str);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WordSpell;
using System;

public class Board : MonoBehaviour
{
    #region Fields
    ListBox<UnityEngine.UI.VerticalLayoutGroup> TryListBox;
    ListBox<UnityEngine.UI.VerticalLayoutGroup> HistoryListBox;
    ListBox<UnityEngine.UI.GridLayoutGroup> SpellListBox;
    ListBox<UnityEngine.UI.GridLayoutGroup> AwardedSpellListBox;

    ListBox<UnityEngine.UI.VerticalLayoutGroup> BestWordListBox;
    ListBox<UnityEngine.UI.VerticalLayoutGroup> BestWordSimpleListBox;
    ListBox<UnityEngine.UI.VerticalLayoutGroup> HighScoresListBox;
    ListBox<UnityEngine.UI.VerticalLayoutGroup> LongestListBox;

    private float newFortuneScale;
    private float fortuneScale = 0f;

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
    public GameObject SpellCanvas;
    public GameObject InputCanvas;
    public GameObject SystemMenu;
    public GameObject SubmitButtonGO;
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
    void Start ()
    {
        //var gol = GameObject.FindGameObjectsWithTag("ScoreText");
        //foreach(GameObject go in gol)
        //{
        //    Debug.Log(go.name);
        //}
        AudioClip[] AmbientSongs = Resources.LoadAll<AudioClip>("Songs");
        if (AmbientSongs != null)
        {
            SetUserInfo("Loaded songs " + AmbientSongs.Length);

        }
        else
        {
            SetUserInfo("Didn't load songs");
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
//        SetUserInfo(GamePersistence.TestPersistence());

        WSGameState.InitGameGlobal();

        SetStoryInfo(EngLetterScoring.Intro1, EngLetterScoring.Intro2);
        SetVersion(Application.version);

        TryListBox = new ListBox<VerticalLayoutGroup>(TryList, TextPrefab);
        HistoryListBox = new ListBox<VerticalLayoutGroup>(HistoryList, TextPrefab);
        SpellListBox = new ListBox<GridLayoutGroup>(SpelllList, SpellPrefab);
        AwardedSpellListBox = new ListBox<GridLayoutGroup>(AwardedSpellList, SpellPrefab);

        BestWordListBox = new ListBox<VerticalLayoutGroup>(BestWordList, TextPrefab);
        BestWordSimpleListBox = new ListBox<VerticalLayoutGroup>(BestWordSimpleList, TextPrefab);
        HighScoresListBox = new ListBox<VerticalLayoutGroup>(HighScoresList, TextPrefab);
        LongestListBox = new ListBox<VerticalLayoutGroup>(LongestList, TextPrefab);

        LocateCamera();

        StartCanvas.SetActive(true);

        LoadStats();

        if (GamePersistence.SavedGameExists())
        {
            StartGame();
        }
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
        if(CamZ > -13.4f)
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

    #endregion Init

    void LoadStats()
    {
        WSGameState.LoadStats();
        RefreshStats();
    }

    void RefreshStats()
    {
        LongestListBox.CreateList(WSGameState.LongestWords, true);
        HighScoresListBox.CreateList(WSGameState.BestGameScores);
        BestWordListBox.CreateList(WSGameState.BestWordScores, true);
        BestWordSimpleListBox.CreateList(WSGameState.BestWordScoresSimple, true);
    }

    // Update is called once per frame
    void Update ()
    {
        if(MsgCanvas.activeSelf)
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
                if(InputCanvas.activeSelf)
                {
                    SelectLetterToChangeDone();
                }
                else
                {
                    SubmitWord();
                }
            }
        }

        if(newFortuneScale - fortuneScale > 0.01f)
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

    // Handlers
    #region Handlers

    public void OnMouseClick()
    {
        HideMsg();
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
        if (!StartCanvas.activeSelf)
        {
            WSGameState.Save();
        }

        WSGameState.SaveStats();
    }

    // Button commands
    public void QuitApp()
    {
        WSGameState.Save();
        Application.Quit();
    }

    public void SubmitWord()
    {
        WSGameState.SubmitWord();
    }

    public void ResetSubmitButton()
    {
        SetCurrentWord("Click on letters to spell a word");
        IndicateGoodWord(WSGameState.WordValidity.Garbage);
    }

    public void HamburgerMenu()
    {
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

    #endregion Handlers

    public void StartGame()
    {
        StartCanvas.SetActive(false);
        ControlCanvas.SetActive(true);

        WSGameState.InitNewGame();

        for (int i = 0; i < WSGameState.gridsize; i++)
        {
            for (int j = 0; j < WSGameState.gridsize; j++)
            {
                LetterProp lp = WSGameState.NewLetter(i, j);
                Transform lbi = NewTile(i, j, lp.TileType);
                lp.SetTransform(lbi);
            }
        }

        WSGameState.NewMusicTile();

        // Check if there is a saved game.
        WSGameState.Load();
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

    #region Controls

    // ----------------------------------------------------------
    // Status settings for Control UI element values

    public void ShowMsg(string text)
    {
        MsgCanvas.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = text;
        MsgCanvas.SetActive(true);
    }

    public void HideMsg()
    {
        MsgCanvas.SetActive(false);
    }

    public GameObject SelectLet(int i, int j)
    {
        GameObject t = (GameObject)Instantiate(SelectPrefab, new Vector3((i - half_offset) * inc, (j - half_offset) * inc, 0.6f), Quaternion.identity);

        GameObject hl = t.transform.GetChild(0).gameObject;
        GameObject vt = t.transform.GetChild(1).gameObject;
        GameObject hr = t.transform.GetChild(2).gameObject;
        GameObject vb = t.transform.GetChild(3).gameObject;

        Material m = WSGameState.GetFortuneColor();
        hl.GetComponent<MeshRenderer>().material = m;
        vt.GetComponent<MeshRenderer>().material = m;
        hr.GetComponent<MeshRenderer>().material = m;
        vb.GetComponent<MeshRenderer>().material = m;

        //Animator anim_hr = hl.GetComponent<Animator>();
        //Animator anim_vt = vt.GetComponent<Animator>();
        //Animator anim_hl = hr.GetComponent<Animator>();
        //Animator anim_vb= vb.GetComponent<Animator>();

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
        HistoryListBox.AddText(" " + s);
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
        SystemMenu.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = s;
    }

    public void SetVersion(string s)
    {
        SystemMenu.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = s;
    }

    public void SetStoryInfo(string s1, string s2)
    {
        StartCanvas.transform.Find("Intro1").GetComponent<Text>().text = s1;
        StartCanvas.transform.Find("Intro2").GetComponent<Text>().text = s2;
    }

    #endregion Controls

    public float TranslatToGrid(int i)
    {
        return ((i - half_offset) * inc);
    }

    // Spell related stuff
    public void AddSpellList(ListBox<GridLayoutGroup> spellbox, SpellInfo si, bool awarded = false)
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

    public void ClearSpellList(ListBox<GridLayoutGroup> spellbox)
    {
        spellbox.Clear();
    }

    public void ShowSpells()
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

    public void CancelSpells()
    {
        SpellCanvas.SetActive(false);
    }

    void SelectSpell(string spellName, bool awarded)
    {
        SpellCanvas.SetActive(false);

        // Awarded spells need to be removed from the list
        Spells.ReadySpell(spellName, awarded, SpellSucceded);
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
    }
    
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WordSpell;

public class Board : MonoBehaviour
{
    // Passed in from Board scene
    public Transform LetterBoxPrefab;
    public GameObject SelectPrefab;
    public Transform LavaLightPrefab;
    public Transform SpellPrefab;
    public Transform TextPrefab;
    public GameObject StartCanvas;
    public GameObject MsgCanvas;
    public GameObject SpellCanvas;
    public GameObject tgo;
    public Camera BoardCam;

    public GameObject LevelText;
    public GameObject ScoreText;
    public GameObject ManaText;

    public GameObject TryList;
    public GameObject HistoryList;
    public GameObject SpelllList;
    public GameObject AwardedSpellList;

    public Transform GameArea;

    public AudioClip SubmitWordSound;
    public AudioClip NewLevelSound;
    public AudioClip GameOverSound;

    ListBox<UnityEngine.UI.VerticalLayoutGroup> TryListBox;
    ListBox<UnityEngine.UI.VerticalLayoutGroup> HistoryListBox;
    ListBox<UnityEngine.UI.GridLayoutGroup> SpellListBox;
    ListBox<UnityEngine.UI.GridLayoutGroup> AwardedSpellListBox;

    private bool MsgShowing = false;

    const int numgrid = 9;
    const float scale_factor = 2500;
    private const int aspect_scale = 1;

    //int yoff = 1;
    int half_offset = WSGameState.gridsize / 2;
    float inc = 1f; // (float)(size * scale_factor);
    float gridYoff = 0f;
    float aspect;
    float CamZ;

    int Selected = Animator.StringToHash("StartSel");

    #region Constants
    const string SpellNamePath = "TextPanel/Name";
    const string SpellCostPath = "TextPanel/Cost";
    const string SpellImagePath = "ButtonPanel/Image";
    #endregion Constants

    // Use this for initialization
    void Start ()
    {
        WSGameState.InitGameGlobal();

        TryListBox = new ListBox<VerticalLayoutGroup>(TryList, TextPrefab);
        HistoryListBox = new ListBox<VerticalLayoutGroup>(HistoryList, TextPrefab);
        SpellListBox = new ListBox<GridLayoutGroup>(SpelllList, SpellPrefab);
        AwardedSpellListBox = new ListBox<GridLayoutGroup>(AwardedSpellList, SpellPrefab);

        LocateCamera();
	}

    void LocateCamera()
    {
        int w = Screen.width;
        int h = Screen.height;
        float dpi = Screen.dpi;

        aspect = (float)w / (float)h;
        if (aspect > 0.65f)
        {
            aspect = 0.65f;
        }

        // Set the camera in the middle of the screen.
        // R code to calute bess fit line for aspect ratios
        //             s8  480/800  800/1280
        // ratios = c(.486,  .6,    .625)
        // sz     = c(9.5,    8,    7.4)
        // off    = c(-5,    -3.6,    -2.5)
        // off = c(17, 21.5,  22)

        // off_fit = lm(off ~ ratios)
        // sz_fit = lm(sz ~ ratios)

        gridYoff = (aspect * 15.09f) + -10.21f;
        gridYoff = -1.7f;
        CamZ = (aspect * 23.54f) - 27.69f;
        if(CamZ > -13.4f)
        {
            CamZ = -13.4f;
        }

        BoardCam.transform.position = new Vector3(0, gridYoff, CamZ);
        BoardCam.orthographicSize = (aspect * -11.0f) + 15.23f;
 

        StartCanvas.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        if(MsgShowing)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                MsgCanvas.SetActive(false);
                MsgShowing = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitWord();
            }
        }
    }

    // Handlers

    public void OnMouseClick()
    {
        MsgCanvas.SetActive(false);
        MsgShowing = false;
    }

    public void StartGame()
    {
        StartCanvas.SetActive(false);

        WSGameState.InitNewGame();

        for (int i = 0; i < WSGameState.gridsize; i++)
        {
            for (int j = 0; j < WSGameState.gridsize; j++)
            {
                Transform lbi = NewTile(i, j);

                WSGameState.NewLetter(i, j, lbi);
            }
        }
    }

    IEnumerator EndGameDelay()
    {
        yield return new WaitForSeconds(1.5f);

        ShowMsg("Game Over");
        PlayEndGameSound();
        yield return new WaitForSeconds(3);
        MsgCanvas.SetActive(false);

        StartCanvas.SetActive(true);
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

    public Transform NewLavaLight()
    {
        Transform lavalight = Instantiate(LavaLightPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        return lavalight;
    }

    public void ShowMsg(string text)
    {
        MsgCanvas.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = text;
        MsgCanvas.SetActive(true);
        MsgShowing = true;
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

        Animator anim_hr = hl.GetComponent<Animator>();
        Animator anim_vt = vt.GetComponent<Animator>();
        Animator anim_hl = hr.GetComponent<Animator>();
        Animator anim_vb= vb.GetComponent<Animator>();

        return t;
    }

    public void DeselectLet(GameObject t)
    {
        t.SetActive(false);
        Destroy(t);
    }

    public float TranslatToGrid(int i)
    {
        return ((i - half_offset) * inc);
    }

    // Button commands
    public void QuitGame()
    {
        Application.Quit();
    }

    public void SubmitWord()
    {
        WSGameState.SubmitWord();
    }

    public void ResetSubmitButton()
    {
        SetCurrentWord("Click on letters to spell a word");
        IndicateGoodWord(false);
    }

    // ----------------------------
    // Status settings

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    public void SetCurrentWord(string s)
    {
        UnityEngine.UI.Text t = tgo.GetComponentInChildren(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
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
        Transform item = HistoryListBox.Add();

        UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

        t.text = s;
    }

    public void ClearHistory()
    {
        HistoryListBox.Clear();
    }

    public void AddTryList(string s)
    {
        Transform item = TryListBox.Add();

        UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

        t.text = s;
    }

    public void ClearTryList()
    {
        TryListBox.Clear();
    }

    public void AddSpellList(ListBox<GridLayoutGroup> spellbox, SpellInfo si, bool awarded = false)
    {
        Transform item = spellbox.Add();
        item.transform.name = si.FriendlyName;

        //UnityEngine.UI.Text s = item.GetChild(0).GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text s = item.Find(SpellNamePath).GetComponent<UnityEngine.UI.Text>();
        s.text = si.FriendlyName;

//        UnityEngine.UI.Text c = item.GetChild(1).GetComponent<UnityEngine.UI.Text>();
        UnityEngine.UI.Text c = item.Find(SpellCostPath).GetComponent<UnityEngine.UI.Text>();
        if (!awarded)
        {
            c.text = si.MannaPoints.ToString();
        }
        else
        {
            c.text = "";
        }

        //UnityEngine.UI.Image i = item.GetChild(2).GetComponent<UnityEngine.UI.Image>();
        //UnityEngine.UI.Image i = item.Find(SpellImagePath).GetComponent<UnityEngine.UI.Image>();
        //i.sprite = si.Image;

        // Add the callback so we know we've been selected
        //EventTrigger trigger = i.gameObject.GetComponent<EventTrigger>();

        //EventTrigger.Entry entry = new EventTrigger.Entry();
        //entry.eventID = EventTriggerType.PointerClick;
        //entry.callback.AddListener((eventData) => { SelectSpell((PointerEventData)eventData); });
        //trigger.triggers.Add(entry);

        UnityEngine.UI.Image i = item.Find(SpellImagePath).GetComponent<UnityEngine.UI.Image>();
        i.sprite = si.Image;

        Button b = item.Find(SpellImagePath).GetComponent<Button>();
        //b.targetGraphic = si.Image;
        b.onClick.AddListener(delegate { SelectSpell(si.FriendlyName, awarded); } );
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

        foreach (SpellInfo si in Spells.AwardedSpells)
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

    void SelectSpell(PointerEventData eventData)
    {
        SpellCanvas.SetActive(false);

        // Awarded spells need to be removed from the list
        string x = eventData.pointerCurrentRaycast.gameObject.transform.parent.parent.parent.parent.name;
        Spells.ReadySpell(eventData.pointerCurrentRaycast.gameObject.transform.parent.name, x == "AwardedSpells", SpellSucceded);
    }

   public void OnClicked(Button button)
   {
       SpellCanvas.SetActive(false);
   }

    void SpellSucceded()
    {
        WSGameState.ChangeManna(Spells.LastManaCost);
    }
    
    public void IndicateGoodWord(bool good)
    {
        //UnityEngine.UI.Button t = tgo.GetComponent(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button;
        if (good)
        {
            var theColor = tgo.GetComponent<UnityEngine.UI.Button>().colors;
            theColor.normalColor = new Color32(72, 234, 94, 255);
            theColor.highlightedColor = new Color32(72, 234, 94, 255);
            tgo.GetComponent<UnityEngine.UI.Button>().colors = theColor;
        }
        else
        {
            var theColor = tgo.GetComponent<UnityEngine.UI.Button>().colors;
            theColor.normalColor = Color.white;
            theColor.highlightedColor = Color.white;
            tgo.GetComponent<UnityEngine.UI.Button>().colors = theColor;
        }
    }

    // Sounds to play
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
//        yield WaitForSeconds(3);
    }
}

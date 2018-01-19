using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordSpell
{
    public class LetterProp
    {
        int i, j;

        private TileTypes tt;

        public Transform LetTF { get; private set; }
        private Animator LetterAnimator;

        private byte _letter;
        static private int prob_total;
        bool IsSelected = false;
        int Burning = Animator.StringToHash("Burning");
        int Selected = Animator.StringToHash("StartSel");
        //int LetterFall = Animator.StringToHash("LetterFall");
        //int LetterFallObj = Animator.StringToHash("LetterFallObj");
        int Idle = Animator.StringToHash("Idle");
        int FlipBack = Animator.StringToHash("FlipBack");
        int FlipForward = Animator.StringToHash("FlipForward");
        int Bomb = Animator.StringToHash("Bomb");

        float letterDCount = 0.0f;
        float letterUCount = 0.0f;
        float letterRCount = 0.0f;
        float letterLCount = 0.0f;

        float letterRotHR = 0.0f;
        float letterRotHL = 0.0f;
        float letterRotVU = 0.0f;
        float letterRotVD = 0.0f;

        Vector3 letterRotHRAxis ;
        Vector3 letterRotHLAxis ;
        Vector3 letterRotVUAxis ;
        Vector3 letterRotVDAxis ;

        Vector3 letterRotHRCAxis;
        Vector3 letterRotHLCAxis;
        Vector3 letterRotVUCAxis;
        Vector3 letterRotVDCAxis;

        private void ClearTransform()
        {
            if(LetTF != null)
            {
                boardScript.DestroyLetterObject(LetTF);
            }
        }

        static Dictionary<TileTypes, TileTypeProperties> SortedTiles = new Dictionary<TileTypes, TileTypeProperties>();
        static List<TileProb> TilesForLevel = new List<TileProb>();

#if UNITY_EDITOR
        static System.Random r = new System.Random(21);
#else
        static System.Random r = new System.Random();
#endif

        #region Properties

        //public bool AnimationEnabled
        //{
        //    set
        //    {
        //        //Animator a = LetterBlockObj.GetComponent<Animator>();
        //        //a.Rebind();
        //        //a.enabled = value;
        //    }
        //    get
        //    {
        //        //Animator a = LetterBlockObj.GetComponent<Animator>();
        //        //return (a.enabled);
        //        return true;
        //    }
        //}

        public TileTypes TileType
        {
            get { return tt; }
            set
            {
                if (tt != value)
                {
                    tt = value;

                    // The tile might have an alternate position if it's new, remember that for the new tile type.
                    float altTilePos = J - (WSGameState.gridsize / 2);
                    altTilePos -= LetTF.position.y;

                    ClearTransform();
                    Transform t = boardScript.NewTile(I, J, tt, -altTilePos);
                    SetTransform(t);
                }
            }
        }

        public byte letter
        {
            get
            {
                return _letter;
            }
            set
            {
                _letter = value;
                //UpdateLetterDisplay();
                //b.Content = Convert.ToChar(_letter).ToString();
            }
        }

        public string ASCIIString
        {
            get
            {
                return(Convert.ToChar(_letter).ToString());
            }
        }

        public char ASCIIChar
        {
            get
            {
                return (Convert.ToChar(_letter));
            }
        }

        public float LetterDCount
        {
            get
            {
                return letterDCount;
            }

            set
            {
                letterDCount = value;
            }
        }

        public int I
        {
            get
            {
                return i;
            }

            set
            {
                i = value;
                //if(TileScript != null)
                //{
                //    TileScript.SetPos(i, j);
                //}
            }
        }

        public int J
        {
            get
            {
                return j;
            }

            set
            {
                j = value;
                //if (TileScript != null)
                //{
                //    TileScript.SetPos(i, j);
                //}
            }
        }

        internal void PlaySelect()
        {
            TileBlockScript.PlaySelect();
        }

        public float LetterUCount
        {
            get
            {
                return letterUCount;
            }

            set
            {
                letterUCount = value;
            }
        }

        public float LetterRCount
        {
            get
            {
                return letterRCount;
            }

            set
            {
                letterRCount = value;
            }
        }

        public float LetterLCount
        {
            get
            {
                return letterLCount;
            }

            set
            {
                letterLCount = value;
            }
        }

        public float LetterRotHR
        {
            get
            {
                return letterRotHR;
            }

            set
            {
                letterRotHR = value;
            }
        }

        public float LetterRotHL
        {
            get
            {
                return letterRotHL;
            }

            set
            {
                letterRotHL = value;
            }
        }

        public float LetterRotVU
        {
            get
            {
                return letterRotVU;
            }

            set
            {
                letterRotVU = value;
            }
        }

        public float LetterRotVD
        {
            get
            {
                return letterRotVD;
            }

            set
            {
                letterRotVD = value;
            }
        }

        public Vector3 LetterRotHRAxis
        {
            get
            {
                return letterRotHRAxis;
            }

            set
            {
                letterRotHRAxis = value;
            }
        }

        public Vector3 LetterRotHLAxis
        {
            get
            {
                return letterRotHLAxis;
            }

            set
            {
                letterRotHLAxis = value;
            }
        }

        public Vector3 LetterRotVUAxis
        {
            get
            {
                return letterRotVUAxis;
            }

            set
            {
                letterRotVUAxis = value;
            }
        }

        public Vector3 LetterRotVDAxis
        {
            get
            {
                return letterRotVDAxis;
            }

            set
            {
                letterRotVDAxis = value;
            }
        }

        public Vector3 LetterRotHRCAxis
        {
            get
            {
                return letterRotHRCAxis;
            }

            set
            {
                letterRotHRCAxis = value;
            }
        }

        public Vector3 LetterRotHLCAxis
        {
            get
            {
                return letterRotHLCAxis;
            }

            set
            {
                letterRotHLCAxis = value;
            }
        }

        public Vector3 LetterRotVUCAxis
        {
            get
            {
                return letterRotVUCAxis;
            }

            set
            {
                letterRotVUCAxis = value;
            }
        }

        public Vector3 LetterRotVDCAxis
        {
            get
            {
                return letterRotVDCAxis;
            }

            set
            {
                letterRotVDCAxis = value;
            }
        }

        public bool MusicHolderRole
        {
            get
            {
                return musicHolder;
            }

            set
            {
                musicHolder = value;
            }
        }

        public SpellInfo SpellInfo { get; internal set; }

        public GameObject LetterBlockObj
        {
            get
            {
                return LetTF.GetChild(0).gameObject;
            }
        }

        public Rigidbody rigidbody
        {
            get
            {
                return (LetTF.GetComponent(typeof(Rigidbody)) as Rigidbody);
                //return (Tf.GetChild(0).GetComponent(typeof(Rigidbody)) as Rigidbody);
            }
        }

        public Tile TileScript
        {
            get
            {
                return (Tile)LetTF.GetComponent(typeof(Tile));
            }
        }

        public TileAnim TileBlockScript
        {
            get
            {
                return (TileAnim)LetTF.GetChild(0).transform.GetComponent(typeof(TileAnim));
            }
        }

        #endregion Properties


        public enum TileTypes
        {
            WordDouble,
            LetterDouble,
            WordTriple,
            LetterTriple,
            Burning,
            Manna,
            Speaker,
            Normal,
        };

        struct TileTypeProperties
        {
            public double probability;
            public int prob2;
            public Color foreground;
            public Color background;
            public double depthmod;
            public int level;
            public double levelmod;
        }

        struct TileProb
        {
            public int probability;
            public TileTypes tt;
        }

        static Dictionary<TileTypes, TileTypeProperties> TileTypeProp = new Dictionary<TileTypes, TileTypeProperties>
        {
            {TileTypes.Normal, new TileTypeProperties { prob2 = 150, probability = 1.0, background = Color.black, foreground = Color.grey, depthmod = 0.01, level = 0, levelmod = 0.0 } },
            {TileTypes.LetterDouble, new TileTypeProperties { prob2 = 10, probability = 0.08, foreground = Color.black, background = Color.magenta, depthmod = 0.01, level = 2, levelmod = 0.2 }},
            {TileTypes.WordDouble, new TileTypeProperties { prob2 = 8, probability = 0.05, foreground = Color.black, background = Color.cyan, depthmod = 0.01, level = 4, levelmod = 0.2 }},
            {TileTypes.LetterTriple, new TileTypeProperties { prob2 = 5, probability = 0.04, foreground = Color.black, background = Color.magenta, depthmod = 0.01, level = 7, levelmod = 0.2 }},
            {TileTypes.WordTriple, new TileTypeProperties { prob2 = 4, probability = 0.03, foreground = Color.black, background = Color.green, depthmod = 0.01, level = 6, levelmod = 0.2 }},
            {TileTypes.Burning, new TileTypeProperties{ prob2 = 10, probability = 0.12, background = Color.black, foreground = Color.red, depthmod = 1.0, level = 3, levelmod = 0.9 }},
            {TileTypes.Manna, new TileTypeProperties{ prob2 = 8, probability = 0.15, foreground = Color.yellow, background = Color.blue, depthmod = 1.0, level = 5, levelmod = 0.4 }},
            {TileTypes.Speaker, new TileTypeProperties{ prob2 = 0, probability = 0.0, foreground = Color.yellow, background = Color.blue, depthmod = 1.0, level = 30000, levelmod = 0.4 }},
        };

        static Material NormalMat;
        static Material LetterDoubleMat;
        static Material LetterTripleMat;
        static Material WordDoubleMat;
        static Material WordTripleMat;
        static Material ManaMat;
        static Material BurningMat;
        //static Material Jeans;

        internal GameObject SelectorObject;
        static GameObject LavaLight;

        static private Board boardScript;
        private bool musicHolder;

        #region Init

        public static void LoadMaterials()
        {
            NormalMat = Resources.Load("Normal") as Material;
            LetterDoubleMat = Resources.Load("Double Letter") as Material;
            LetterTripleMat = Resources.Load("LavenderGem") as Material;
            WordDoubleMat = Resources.Load("Double Word") as Material;
            WordTripleMat = Resources.Load("PurpleGem") as Material;
            ManaMat = Resources.Load("TurquoiseGem") as Material;
            BurningMat = Resources.Load("Burnt") as Material;
            //Jeans = (Material)Resources.Load("Jeans");

            LavaLight = (GameObject)Resources.Load("LavalLight");
        }

        public static void InitLetterPropertyList(Board _boardScript)
        {
            // We'll need this to instantiate objects
            boardScript = _boardScript;

            // Load Materials
            LoadMaterials();
       
            foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp.OrderBy(key => key.Value.probability))
            {
                SortedTiles.Add(pair.Key, pair.Value);
            }
        }

        public static void InitProbability(int level)
        {
            prob_total = 0;
            TilesForLevel.Clear();

            foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp)
            {
                if(level >= pair.Value.level)
                {
                    TileProb tp = new TileProb();
                    tp.probability = (pair.Value.prob2 + (int)(pair.Value.levelmod * level));
                    tp.tt = pair.Key;
                    prob_total += tp.probability;
                    TilesForLevel.Add(tp);
                }
            }
        }

        public LetterProp(byte _letter, TileTypes _tt, int _i, int _j)
        {
            I = _i;
            J = _j;
            tt = _tt;

            letter = _letter;
        }

        public LetterProp(int level, bool levelup, int _i, int _j, Transform _tf)
        {
            LetTF = _tf;
            tt = CreateNewTile(level, levelup);

            LetterAnimator = LetterBlockObj.GetComponent<Animator>();
            //TileScript = (Tile)LetterBlockObj.GetComponent(typeof(Tile));

            //TileScript.SetPos(I, J, this);

            letter = EngLetterScoring.GetRandomLetter(IsBurning(), WSGameState.GetFortune());

            I = _i;
            J = _j;

            UpdateLetterDisplay();
            UpdateMaterial();
            if(tt == TileTypes.Speaker)
            {
                Debug.Log("Speaker tile shoudl be zero percent.  What happened?");
            }
        }

        public LetterProp(int level, bool levelup, int _i, int _j)
        {
            tt = CreateNewTile(level, levelup);

            letter = EngLetterScoring.GetRandomLetter(IsBurning(), WSGameState.GetFortune());

            I = _i;
            J = _j;
        }

        public void SetTransform(Transform _tf)
        {
            LetTF = _tf;

            LetterAnimator = LetterBlockObj.GetComponent<Animator>();

            //TileScript = (Tile)LetterBlockObj.GetComponent(typeof(Tile));

            TileScript.AttachLetterProp(this);

            UpdateLetterDisplay();
            UpdateMaterial();
        }

        #endregion Init

        public void PlayBackgroundMusic()
        {
            TileType = LetterProp.TileTypes.Speaker;

            MusicHolderRole = true;

            TileBlockScript.NewSong();
        }

        public void StopBackgroundMusic()
        {
            TileType = LetterProp.TileTypes.Speaker;

            MusicHolderRole = false;

            TileBlockScript.StopSong();
        }

        public void UpdateMaterial()
        {
            switch (tt)
            {
                case TileTypes.Burning:
                    boardScript.PlayLavaSound();
                    BurnTile();
                    break;
                case TileTypes.Normal:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = NormalMat;
                    break;
                case TileTypes.WordDouble:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = WordDoubleMat;
                    break;
                case TileTypes.WordTriple:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = WordTripleMat;
                    break;
                case TileTypes.LetterDouble:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = LetterDoubleMat;
                    break;
                case TileTypes.LetterTriple:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = LetterTripleMat;
                    break;
                case TileTypes.Manna:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = ManaMat;
                    break;
                case TileTypes.Speaker:
                    break;
                default:
                    LetterBlockObj.GetComponent<MeshRenderer>().material = NormalMat;
                    break;
            }
        }

        private void BurnTile()
        {
            LetterBlockObj.GetComponent<MeshRenderer>().material = BurningMat;
            Transform ll = boardScript.NewLavaLight();
            ll.gameObject.SetActive(true);
            ll.SetParent(LetterBlockObj.transform, false);
            ll.name = "Point light";

            LetterAnimator.gameObject.SetActive(true);
            LetterAnimator.SetTrigger(Burning);
        }

        public void FlipTileBack()
        {
            LetterAnimator.SetTrigger(FlipBack);
        }

        public void FlipTileForward()
        {
            LetterAnimator.SetTrigger(FlipForward);
        }

        public void TileIdle()
        {
            LetterAnimator.SetTrigger(Idle);
        }

        public void BlowupTile()
        {
            LetterAnimator.SetTrigger(Bomb);
        }

        public void UpdateLetterDisplay()
        {
            //GameObject text = Tf.GetChild(0).GetChild(0).gameObject;
            GameObject text = LetterBlockObj.transform.GetChild(0).gameObject;

            TextMesh tm = text.GetComponent(typeof(TextMesh)) as TextMesh;
            if (tm != null)
            {
                tm.text = this.ASCIIString;
            }

            // Adjustments for letter widths so they are centered
            switch(this.ASCIIChar)
            {
                case 'W':
                case 'M':
                    //text.transform.localPosition = new Vector3(-0.3f, 0.3f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.35f, 0.49f, -0.06f);
                    break;
                case 'G':
                case 'Q':
                case 'O':
                case 'C':
                    //text.transform.localPosition = new Vector3(-0.3f, 0.3f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.33f, 0.49f, -0.06f);
                    break;
                case 'L':
                case 'T':
                    //text.transform.localPosition = new Vector3(-0.3f, 0.3f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.16f, 0.49f, -0.06f);
                    break;
                case 'S':
                case 'Z':
                case 'J':
                    //text.transform.localPosition = new Vector3(-0.3f, 0.3f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.2f, 0.49f, -0.06f);
                    break;
                case 'I':
                    //text.transform.localPosition = new Vector3(-0.05f, 0.3f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.1f, 0.49f, -0.06f);
                    break;
                default:
                    //text.transform.localPosition = new Vector3(-0.17f, 0.3f, -0.06f);
                    //text.transform.localPosition = new Vector3(-0.22f, 0.6f, -0.06f);
                    text.transform.localPosition = new Vector3(-0.26f, 0.49f, -0.06f);
                    break;
            }
        }

        public void SetSelected(bool _selected)
        {
            if(IsSelected != _selected)
            {
                //Animator anim = LetterBlockObj.GetComponent<Animator>();
                
                LetterAnimator.SetTrigger(Selected);

            }
            IsSelected = _selected;

        }

        private TileTypes CreateNewTile(int CurrentLevel, bool levelup)
        {
            if (levelup)
            {
                foreach (KeyValuePair<TileTypes, TileTypeProperties> pair in TileTypeProp)
                {
                    if(pair.Value.level == CurrentLevel)
                    {
                        return pair.Key;
                    }
                }
            }

            int index = r.Next(prob_total);
            int sum = 0;
            int i = 0;

            while (sum < index)
            {                
                sum = sum + TilesForLevel[i++].probability;
            }
            return TilesForLevel[Math.Max(0, i - 1)].tt;
        }

        internal bool IsBurning()
        {
            if(tt == TileTypes.Burning)
            {
                return true;
            }
            return false;
        }

        public string LetterPopup()
        {
            string ret = Convert.ToChar(letter).ToString() + "=" + EngLetterScoring.LetterValue(letter).ToString();
            if (GetLetterMult() > 1)
            {
                ret += " Letter x" + GetLetterMult().ToString();
            } else if(GetWordMult() >= 1)
            {
                ret += " Word x" + (GetWordMult()+1).ToString();
            }

            return ret;
        }

        internal int GetLetterMult()
        {
            if (tt == TileTypes.LetterDouble)
            {
                return 2;
            }
            else if (tt == TileTypes.LetterTriple)
            {
                return 3;
            }

            return 1;
        }

        internal int GetWordMult()
        {
            if (tt == TileTypes.WordDouble)
            {
                return 2;
            } else if(tt == TileTypes.WordTriple)
            {
                return 3;
            }

            return 0;
        }

        internal bool IsManna()
        {
            if (tt == TileTypes.Manna)
            {
                return true;
            }
            return false;
        }

        internal void ChangeTileTo(TileTypes _tt)
        {
            switch(_tt)
            {
                case TileTypes.Burning:
                    tt = _tt;
                    BurnTile();
                    break;
            }
        }
    }
}

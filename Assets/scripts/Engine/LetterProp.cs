using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordSpell
{
    public class LetterProp
    {
        int i, j;

        Tile tileScript;
        private TileTypes tt;

        public Transform Tf { get; private set; }
        private Animator LetterAnimator;

        private byte _letter;
        static private int prob_total;
        bool IsSelected = false;
        int Burning = Animator.StringToHash("Burning");
        int Selected = Animator.StringToHash("StartSel");
        int LetterFall = Animator.StringToHash("LetterFall");
        int LetterFallObj = Animator.StringToHash("LetterFallObj");
        int Idle = Animator.StringToHash("Idle");
        int FlipBack = Animator.StringToHash("FlipBack");
        int FlipForward = Animator.StringToHash("FlipForward");
        int Bomb = Animator.StringToHash("Bomb");

        float letterDCount = 0.0f;
        float letterUCount = 0.0f;
        float letterRCount = 0.0f;
        float letterLCount = 0.0f;

        public TileTypes TileType
        {
            get { return tt;  }
        }

        static Dictionary<TileTypes, TileTypeProperties> SortedTiles = new Dictionary<TileTypes, TileTypeProperties>();
        static List<TileProb> TilesForLevel = new List<TileProb>();

        static System.Random r = new System.Random();

        public byte letter
        {
            get
            {
                return _letter;
            }
            set
            {
                _letter = value;
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
                tileScript.SetPos(i, j); 
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
                tileScript.SetPos(i, j);
            }
        }

        public SpellInfo SpellInfo { get; internal set; }

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

        public enum TileTypes
        {
            WordDouble,
            LetterDouble,
            WordTriple,
            LetterTriple,
            Burning,
            Manna,
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
            {TileTypes.Burning, new TileTypeProperties{ prob2 = 10, probability = 0.12, background = Color.black, foreground = Color.red, depthmod = 1.0, level = 3, levelmod = 0.5 }},
            {TileTypes.Manna, new TileTypeProperties{ prob2 = 8, probability = 0.15, foreground = Color.yellow, background = Color.blue, depthmod = 1.0, level = 5, levelmod = 0.4 }},
        };

        static Material NoramlMat;
        static Material LetterDoubleMat;
        static Material LetterTripleMat;
        static Material WordDoubleMat;
        static Material WordTripleMat;
        static Material ManaMat;
        static Material BurningMat;

        internal GameObject SelectorObject;
        static GameObject LavaLight;

        static private Board boardScript;

        public static void LoadMaterials()
        {
            NoramlMat = (Material)Resources.Load("Normal");
            LetterDoubleMat = (Material)Resources.Load("Double Letter");
            LetterTripleMat = (Material)Resources.Load("Triple Letter");
            WordDoubleMat = (Material)Resources.Load("Double Word");
            WordTripleMat = (Material)Resources.Load("Triple Word");
            ManaMat = (Material)Resources.Load("Mana");
            BurningMat = (Material)Resources.Load("Burnt");

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
            tt = CreateNewTile(level, levelup);
            Tf = _tf;

            LetterAnimator = Tf.gameObject.GetComponent<Animator>();

            tileScript = (Tile)Tf.gameObject.GetComponent(typeof(Tile));

            tileScript.SetPos(I, J, this);

            letter = EngLetterScoring.GetRandomLetter(IsBurning(), WSGameState.GetFortune());

            I = _i;
            J = _j;

            UpdateLetterDisplay();
            UpdateMaterial();
        }

        public void UpdateMaterial()
        {
            switch(tt)
            {
                case TileTypes.Burning:
                    BurnTile();
                    break;
                case TileTypes.Normal:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = NoramlMat;
                    break;
                case TileTypes.WordDouble:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = WordDoubleMat;
                    break;
                case TileTypes.WordTriple:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = WordTripleMat;
                    break;
                case TileTypes.LetterDouble:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = LetterDoubleMat;
                    break;
                case TileTypes.LetterTriple:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = LetterTripleMat;
                    break;
                case TileTypes.Manna:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = ManaMat;
                    break;
                default:
                    Tf.gameObject.GetComponent<MeshRenderer>().material = NoramlMat;
                    break;
            }
        }

        private void BurnTile()
        {
            //Tf.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = BurningMat;
            Tf.gameObject.GetComponent<MeshRenderer>().material = BurningMat;
            Transform ll = boardScript.NewLavaLight();
            ll.gameObject.SetActive(true);
            ll.SetParent(Tf, false);
            ll.name = "Point light";
            // Need a point light
            //Tf.GetChild(0).gameObject.SetActive(true);
            //GameObject t = (GameObject)Instantiate(LavaLight, new Vector3(0, 0, 0), Quaternion.identity);
            //t.transform.SetParent(Tf);
            //                    Object.Instantiate();
            //Tf.GetChild(0).gameObject.SetActive(true);
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
            GameObject text = Tf.GetChild(0).gameObject;

            TextMesh tm = text.GetComponent(typeof(TextMesh)) as TextMesh;
            if (tm != null)
            {
                tm.text = this.ASCIIString;
            }

            // Adjustments for letter widths so they are centered
            if(this.ASCIIChar == 'W')
            {
                text.transform.position -= new Vector3(.15f, 0, 0);
            }
        }

        public void SetSelected(bool _selected)
        {
            if(IsSelected != _selected)
            {
                //Animator anim = Tf.gameObject.GetComponent<Animator>();
                
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

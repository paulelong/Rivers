using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WordSpell 
{
    public class WSListBox : ScriptableObject 
    {
        GameObject list;
        Transform prefab;
        public string dbg = "";

        public Transform Prefab
        {
            get
            {
                return prefab;
            }

            set
            {
                prefab = value;
            }
        }

        public void InitWSListBox(GameObject _list, Transform _prefab)
        {
            list = _list;
            Prefab = _prefab;
        }

        public Transform Add()
        {
            Transform listItem = Instantiate(Prefab, new Vector3(0, 0, 0), Quaternion.identity);

            LayoutGroup lg = list.GetComponent(typeof(LayoutGroup)) as LayoutGroup;
            listItem.SetParent(lg.transform);

            listItem.localScale = new Vector3(1, 1, 1);

            return (listItem);
        }

        public void AddText(string s)
        {
            Transform item = Add();

            UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

            t.text = s;
        }

        public void InsertText(string s)
        {
            Transform item = Add();
            item.SetSiblingIndex(0);

            UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

            t.text = s;
        }

        public void CreateList(List<string> l, bool addSpace = false)
        {
            Clear();
            Logging.StartDbg("cl0");
            int debugcnt = 0;

            foreach (string s in l)
            {
                if (debugcnt < 2) { Logging.StartDbg("cl1"); }

                Transform item = Add();
                if (debugcnt < 2) { Logging.StartDbg("cl2(" + s.Length + ")"); }

                UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();
                if (debugcnt < 2) { Logging.StartDbg("cl3"); }

                if (addSpace)
                {
                    t.text = " " + s;
                }
                else
                {
                    t.text = s;
                }
                debugcnt++;
            }
            Logging.StartDbg("clx");
        }

        public List<Transform> ListboxObjects
        {
            get
            {
                List<Transform> l = new List<Transform>();

                LayoutGroup lg = list.transform.GetComponent<LayoutGroup>() as LayoutGroup;
                if (lg != null)
                {
                    //LayoutGroup lg = list.GetComponent(typeof(Layout)) as LayoutGroup;
                    foreach (Transform t in lg.transform)
                    {
                        l.Add(t);
                    }
                }

                return l;
            }
        }

        public void Clear()
        {
            LayoutGroup lg = list.transform.GetComponent<LayoutGroup>() as LayoutGroup;
            if (lg != null)
            {
                //LayoutGroup lg = list.GetComponent(typeof(Layout)) as LayoutGroup;
                foreach (Transform t in lg.transform)
                {
                    Destroy(t.gameObject);
                }
            }
            else
            {
                    Logging.PlayDbg("lb.c!");
            }
        }
    }
}

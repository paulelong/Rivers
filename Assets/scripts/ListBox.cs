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

        public void InitWSListBox(GameObject _list, Transform _prefab)
        {
            list = _list;
            prefab = _prefab;
        }

        public Transform Add()
        {
            Transform listItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

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
            WSGameState.boardScript.StartDbg("cl0");
            int debugcnt = 0;

            foreach(string s in l)
            {
                if (debugcnt < 2) { WSGameState.boardScript.StartDbg("cl1"); }

                Transform item = Add();
                if(debugcnt < 2){ WSGameState.boardScript.StartDbg("cl2(" + s.Length + ")");  }

                UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();
                if (debugcnt < 2) { WSGameState.boardScript.StartDbg("cl3"); }

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
            WSGameState.boardScript.StartDbg("clx");
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
                WSGameState.boardScript.PlayDbg("lb.c!");
            }
        }
    }
}

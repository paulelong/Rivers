using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WordSpell 
{
    public class ListBox<Layout> : ScriptableObject where Layout : class //: MonoBehaviour
    {
        GameObject list;
        Transform prefab;

        public ListBox(GameObject _list, Transform _prefab)
        {
            list = _list;
            prefab = _prefab;
        }

        public Transform Add()
        {
            Layout l = list.GetComponent(typeof(Layout)) as Layout;

            Transform listItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

            LayoutGroup lg = l as LayoutGroup;
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

            foreach(string s in l)
            {
                Transform item = Add();

                UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

                if (addSpace)
                {
                    t.text = " " + s;
                }
                else
                {
                    t.text = s;
                }
            }
        }

        public void Clear()
        {
            //UnityEngine.UI.VerticalLayoutGroup l = list.GetComponent(typeof(UnityEngine.UI.VerticalLayoutGroup)) as UnityEngine.UI.VerticalLayoutGroup;
            Layout l = list.GetComponent(typeof(Layout)) as Layout;
            LayoutGroup lg = l as LayoutGroup;
            foreach (Transform t in lg.transform)
            {
                Destroy(t.gameObject);
            }
        }
    }
}

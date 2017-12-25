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

        public WSListBox(GameObject _list, Transform _prefab)
        {
            list = _list;
            prefab = _prefab;
        }

        public Transform Add<Layout>()
        {
            Transform listItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

            LayoutGroup lg = list.GetComponent(typeof(Layout)) as LayoutGroup;
            listItem.SetParent(lg.transform);

            listItem.localScale = new Vector3(1, 1, 1);

            return (listItem);
        }

        public void AddText<Layout>(string s)
        {
            Transform item = Add<Layout>();

            UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

            t.text = s;
        }

        public void InsertText<Layout>(string s)
        {
            Transform item = Add<Layout>();
            item.SetSiblingIndex(0);

            UnityEngine.UI.Text t = item.GetComponent<UnityEngine.UI.Text>();

            t.text = s;
        }

        public void CreateList<Layout>(List<string> l, bool addSpace = false)
        {
            Clear<Layout>();

            foreach(string s in l)
            {
                Transform item = Add<Layout>();

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

        public void Clear<Layout>()
        {
            LayoutGroup lg = list.transform.GetComponent<LayoutGroup>() as LayoutGroup;
            //LayoutGroup lg = list.GetComponent(typeof(Layout)) as LayoutGroup;
            foreach (Transform t in lg.transform)
            {
                Destroy(t.gameObject);
            }
        }
    }
}

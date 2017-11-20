using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WordSpell 
{
    class ListBox<Layout> : ScriptableObject where Layout : class //: MonoBehaviour
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
            //UnityEngine.UI.VerticalLayoutGroup l = list.GetComponent(typeof(UnityEngine.UI.VerticalLayoutGroup)) as UnityEngine.UI.VerticalLayoutGroup;
            Layout l = list.GetComponent(typeof(Layout)) as Layout;

            Transform listItem = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

            LayoutGroup lg = l as LayoutGroup;
            listItem.SetParent(lg.transform);

            listItem.localScale = new Vector3(1, 1, 1);

            return (listItem);
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

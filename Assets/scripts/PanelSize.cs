using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSize : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        float ratio = (float)Screen.width / (float)Screen.height;

        if(ratio < (.6f))
        {
            float newwidth = ratio * 800f;

            gameObject.GetComponent<LayoutElement>().preferredWidth = newwidth;
        }
		
	}
	
}

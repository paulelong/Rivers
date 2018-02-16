using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordSpell;

public class TimerSpin : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(Vector3.down, 100f * Time.deltaTime);
        Logging.StartDbg("tic " + transform.rotation.ToString(), timestamp:true);
    }
}

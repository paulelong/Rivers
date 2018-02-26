using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WordSpell;

public class StartupManager : MonoBehaviour
{

    // Use this for initialization
    private IEnumerator Start()
    {
        while (!LocalizationManager.instance.GetIsReady())
        {
            yield return null;
        }

        Logging.StartDbg("SM0", timestamp: true);
        SceneManager.LoadScene("WordSpell");
        Logging.StartDbg("SMx", timestamp: true);
    }
}

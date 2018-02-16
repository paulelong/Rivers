using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using WordSpell;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager instance;

    private Dictionary<string, string> localizedText;
    private bool isReady = false;
    private string missingTextString = "Localized text not found";

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Logging.StartDbg("w1", timestamp: true);

        //StartCoroutine(Example());

        Logging.StartDbg("w2", timestamp:true);

        instance.LoadLocalizedText("EngLocale.xml");

        EngLetterScoring.LoadDictionary();
    }

    IEnumerator Example()
    {
        Logging.StartDbg("wait1", timestamp: true);
        yield return new WaitForSecondsRealtime(5);
        Logging.StartDbg("waitx", timestamp: true);
    }


    public void LoadLocalizedText(string fileName)
    {
        Logging.StartDbg("Dl0");

        localizedText = new Dictionary<string, string>();

#if  UNITY_EDITOR
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_ANDROID
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#else
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#endif

        Logging.StartDbg("Dl1:" + filePath);

        try
        {
            Logging.StartDbg("Dl2");
            XmlSerializer xs = new XmlSerializer(typeof(LocalizationData));
            LocalizationData ld;

            Logging.StartDbg("Dl3");
            if (filePath.Contains("://"))
            {
                Logging.StartDbg("Dl3.1");
                WWW www = new WWW(filePath);

                if(string.IsNullOrEmpty(www.error))
                {
                    using (TextReader textReader = new StringReader(www.text))
                    {
                        Logging.StartDbg("Dl3.2");
                        ld = xs.Deserialize(textReader) as LocalizationData;
                    }
                    Logging.StartDbg("Dl3.3");

                    foreach (LocalizationItem li in ld.items)
                    {
                        localizedText.Add(li.key, li.value);
                    }
                }
                else
                {
                    Logging.StartDbg("Dl3.4");
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        ld = (LocalizationData)xs.Deserialize(fs);
                    }

                    foreach (LocalizationItem li in ld.items)
                    {
                        localizedText.Add(li.key, li.value);
                    }
                }
                else
                {
                    Logging.StartDbg("Dl9");
                }

            }

            Logging.StartDbg("Dl=" + localizedText.Count);
        }
        catch (Exception e)
        {
            Logging.StartDbg("Dl! " + e.Message);
            Debug.Log("Localization!" + e.Message);
        }

        isReady = true;
        Logging.StartDbg("Dlx");
    }


    //IEnumerator ParseXML(string filePath)
    //{
    //    if (filePath.Contains("://"))
    //    {
    //        WWW www = new WWW(filePath);
    //        yield return www;

    //        XmlData = DataCollection.DeSerialize(www);
    //    }
    //    else
    //    {
    //        XmlData = DataCollection.DeSerialize(filePath);
    //    }
    //}

    //public static LocalizationData DeSerialize(WWW www)
    //{
    //    using (TextReader textReader = new StringReader(www.text))
    //    {
    //        XmlSerializer serializer = new XmlSerializer(typeof(LocalizationData));

    //        LocalizationData XmlData = serializer.Deserialize(textReader) as LocalizationData;

    //        return XmlData;
    //    }
    //}

    public string GetLocalizedValue(string key)
    {
        string result = missingTextString;

        if (localizedText.ContainsKey(key))
        {
            result = localizedText[key];
        }

        return result;
    }

    public bool GetIsReady()
    {
        return isReady;
    }

}
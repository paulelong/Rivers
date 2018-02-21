using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml.Serialization;
using WordSpell;
using System.Text;

public class LocalizationManager : MonoBehaviour
{
    public GameObject StatusText;
    public GameObject Version;

    public delegate void LoadDataCallback(WWW www);
    public delegate void StoreDataCallback(WWW www);

    public static LocalizationManager instance;
    private static LocalizationData localizationData;

    private static Dictionary<string, string> localizedText;
    private static bool XMLisReady = false;

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

        StartCoroutine(LoadFileAsync(Path.Combine(Application.streamingAssetsPath, "EngLocale.xml"), LoadLocalizationData));
        StartCoroutine(LoadFileAsync(Path.Combine(Application.streamingAssetsPath, "EngDictACache.xml"), EngLetterScoring.LoadDictionaryData));
        //StartCoroutine(LoadFileAsync(EngLetterScoring.DictionaryCachePath, EngLetterScoring.LoadDictionaryData));
        StartCoroutine(LoadFileAsync(EngLetterScoring.PartialLookupCachePath, EngLetterScoring.PartialLookupData));
        //StartCoroutine(LoadFileAsync(EngLetterScoring.DictionaryTextPath, EngLetterScoring.DictionaryTextData));

        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        UpdateVersion(Application.version);

        Logging.StartDbg("w1", timestamp: true);
        Logging.StartDbg("w2");

        UpdateStatus("Loading localization data...");

        while (!instance.GetIsXMLReady())
        {
            yield return null;
        }
        Logging.StartDbg("w3");

        UpdateStatus("Loading dictionary cache...");

        while (!EngLetterScoring.DictionaryCacheReady)
        {
            yield return null;
        }
        Logging.StartDbg("w4");

        UpdateStatus("Loading partial lookup cache...");

        //while (!EngLetterScoring.DictionharyPartialCacheReady)
        //{
        //    yield return null;
        //}
        //Logging.StartDbg("w5");

        //UpdateStatus("Checking dictionary...");

        //EngLetterScoring.ReloadDictionary();

        UpdateStatus("Ready!");

        isReady = true;

        Logging.StartDbg("wx", timestamp:true);
    }

    private void UpdateStatus(string status)
    {
        UnityEngine.UI.Text t = StatusText.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = status;
    }

    private void UpdateVersion(string status)
    {
        UnityEngine.UI.Text t = Version.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
        t.text = status;
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

    static IEnumerator LoadFileAsync(string filePath, LoadDataCallback ld)
    {
        Logging.StartDbg("lfa1");

        using (WWW www = new WWW(filePath))
        {
            yield return www;
            Logging.StartDbg("lf2");
            ld(www);
        }

        Logging.StartDbg("lfax");
    }

    static IEnumerator StoreFileAsync(string filePath, string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(data);

        using (WWW www = new WWW(filePath, bytes))
        {
            yield return www;
            Logging.StartDbg("sfax");
        }

    }

    public static void StoreFile(string filePath, string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(data);

        using (WWW www = new WWW(filePath, bytes))
        {
            Logging.StartDbg("sf1");
        }
        Logging.StartDbg("sfx");
    }

    void LoadLocalizationData(WWW www)
    {
        if (string.IsNullOrEmpty(www.error))
        {
            localizationData = XmlDeserializeFromWWW<LocalizationData>(www);

            localizedText = new Dictionary<string, string>();

            foreach (LocalizationItem li in localizationData.items)
            {
                localizedText.Add(li.key, li.value);
            }
        }
        else
        {
            Logging.StartDbg("lld!" + www.url + ":::" + www.error);
        }

        XMLisReady = true;
    }
    
    public static datatype XmlDeserializeFromWWW<datatype>(WWW www)
    {
        using (TextReader textReader = new StringReader(www.text))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(datatype));

            datatype XmlData = (datatype)serializer.Deserialize(textReader);

            return XmlData;
        }
    }

    public static string XmlSerializeToString<datatype>(datatype dt)
    {
        var serializer = new XmlSerializer(typeof(datatype));
        var sb = new StringBuilder();

        using (TextWriter writer = new StringWriter(sb))
        {
            serializer.Serialize(writer, dt);
        }

        return sb.ToString();
    }

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

    public bool GetIsXMLReady()
    {
        return XMLisReady;
    }
}
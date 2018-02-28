using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Xml.Serialization;
using WordSpell;
using System.Text;

public class LocalizationManager : MonoBehaviour
{
    public GameObject StatusText;
    public GameObject Version;

    public delegate void LoadDataCallback(string text);
    public delegate void StoreDataCallback(WWW www);

    public static string[] AsyncStringSlots = new string[3];
    public static bool[] AsyncCompleteSlots = new bool[3];

    public static LocalizationManager instance;
    private static LocalizationData localizationData;

    private static Dictionary<string, string> localizedText;
    private static bool XMLisReady = false;

    private bool isReady = false;
    private string missingTextString = "Localized text not found";
    private static bool MusicLoadingDone = false;

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

        Logging.StartDbg("lma1", timestamp: true);
        StartCoroutine(LoadFileAsync(Path.Combine(Application.streamingAssetsPath, "EngLocale.xml"), LoadLocalizationData, 0));
        Logging.StartDbg("lma2", timestamp: true);
        StartCoroutine(LoadFileAsync(Path.Combine(Application.streamingAssetsPath, "EngDictACache.xml"), EngLetterScoring.LoadDictionaryData,1));
        //StartCoroutine(LoadFileAsync(EngLetterScoring.DictionaryCachePath, EngLetterScoring.LoadDictionaryData));
        Logging.StartDbg("lma3", timestamp: true);
        StartCoroutine(LoadFileAsync(EngLetterScoring.PartialLookupCachePath, EngLetterScoring.PartialLookupData,2));
        Logging.StartDbg("lma4", timestamp: true);
        StartCoroutine(LoadMusicAsync());
        Logging.StartDbg("lma5", timestamp: true);

        DontDestroyOnLoad(gameObject);

        Logging.StartDbg("lmax", timestamp: true);
    }

    private void Start()
    {
        UpdateVersion(Application.version);

        Logging.StartDbg("w1", timestamp: true);

        UpdateStatus("Loading localization data...");

        Logging.StartDbg("wx", timestamp:true);
    }


    public void Update()
    {
        if (XMLisReady && EngLetterScoring.DictionaryCacheReady && EngLetterScoring.DictionaryPartialCacheReady && MusicLoadingDone)
        {
            isReady = true;
        }
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

    static IEnumerator LoadFileAsync(string filePath, LoadDataCallback ld, int slot)
    {
        Logging.StartDbg("lfa1");

        string result = "";

        if (filePath.Contains("://"))
        {
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            if (string.IsNullOrEmpty(www.error))
            {
                result = www.downloadHandler.text;
            }
            else
            {
                Logging.StartDbg("lfa1!" + www.error + "\n" + filePath + "\n" + www.responseCode + "\n");
            }
        }
        else
        {
            result = System.IO.File.ReadAllText(filePath);
        }

        AsyncStringSlots[slot] = result;
        AsyncCompleteSlots[slot] = true;
        ld(result);

        //using (WWW www = new WWW(filePath))
        //{
        //    yield return www;
        //    Logging.StartDbg("lf2");
        //    ld(www);
        //}

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

    static IEnumerator LoadMusicAsync()
    {
        Logging.StartDbg("lmaa0");

        foreach(string songname in Songs.SongNames)
        {
            ResourceRequest loadAsync = Resources.LoadAsync<AudioClip>("Songs/" + songname);
            while (!loadAsync.isDone)
            {
                Debug.Log("Load Progress: " + loadAsync.progress);
                yield return null;
            }

            Songs.AddSong(loadAsync.asset as AudioClip);
        }

        MusicLoadingDone = true;

        Logging.StartDbg("lmaa=" + Songs.SongNames.Length + "," + Songs.Count);
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

    void LoadLocalizationData(string text)
    {
        localizationData = XmlDeserializeFromText<LocalizationData>(text);

        localizedText = new Dictionary<string, string>();

        Logging.StartDbg("lld1", timestamp: true);
        foreach (LocalizationItem li in localizationData.items)
        {
            localizedText.Add(li.key, li.value);
        }
        Logging.StartDbg("lldx", timestamp: true);

        XMLisReady = true;
    }
    
    public static datatype XmlDeserializeFromText<datatype>(string text)
    {
        Logging.StartDbg("xd1", timestamp: true);

        using (TextReader textReader = new StringReader(text))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(datatype));

            datatype XmlData = (datatype)serializer.Deserialize(textReader);

            return XmlData;
        }

        Logging.StartDbg("xdx", timestamp: true);
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

    public string GetLocalizedValuesByindex(string key, int index)
    {
        string[] result = null;

        if (localizedText.ContainsKey(key))
        {
            result = localizedText[key].Split('\n');
        }
        else
        {
            return "Error: Can't find strings";
        }

        if (result == null || result.Length <= 0)
        {
            return "Error: String list empty";
        }

        if(index >= result.Length)
        {
            return "";
        }

        return result[index];
    }


    public string GetLocalizedValueRandom(string key)
    {
        string[] result = null;

        if (localizedText.ContainsKey(key))
        {
             result = localizedText[key].Split('\n');
        }
        else
        {
            return "Error: Can't find strings";
        }

        if(result == null || result.Length <= 0)
        {
            return "Error: String list empty";
        }

        int v = WSGameState.Rnd.Next(result.Length);

        return result[v];
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
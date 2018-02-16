using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;


namespace GenLocaleXML
{
    class Program
    {
        static void Main(string[] args)
        {
            LocalizationData ld = new LocalizationData();

            LocalizationItem li = new LocalizationItem();

            li.key = "text_item1";
            li.value = "val1";

            ld.items.Add(li);

            li.key = "text_item2";
            li.value = "val2";

            ld.items.Add(li);

            SaveLocaleTemplate("EngLocale.xml", ld);
        }

        internal static void SaveLocaleTemplate(string filePath, LocalizationData ld)
        {
            //File.Delete(Application.persistentDataPath + "/" + SaveGamePath);
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(LocalizationData));

                using (FileStream tw = new FileStream(filePath, FileMode.Create))
                {
                    xs.Serialize(tw, ld);
                }
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }
    }
}

using System;
using System.IO;
using System.Xml.Serialization;
using ColossalFramework.IO;
using UnityEngine;

namespace NoProblem
{
    [XmlRoot("NoProblemOptions")]
    public class Options
    {
        // (get) Token: 0x06000013 RID: 19 RVA: 0x000024BE File Offset: 0x000006BE
        public static string ConfigurationPath
        {
            get
            {
                return Options.configurationPath;
            }
        }

        public void OnPreSerialize()
        {
        }

        public void OnPostDeserialize()
        {
        }

        public void Save()
        {
            string path = Options.ConfigurationPath;
            Options options = ModInfo.Options;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                options.OnPreSerialize();
                xmlSerializer.Serialize(streamWriter, options);
            }
        }

        public static Options Load()
        {
            string text = Options.ConfigurationPath;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));
            Options result;
            try
            {
                using (StreamReader streamReader = new StreamReader(text))
                {
                    result = (xmlSerializer.Deserialize(streamReader) as Options);
                }
            }
            catch (Exception arg)
            {
                Debug.Log(string.Format("[{0}]: Error Parsing {1}: {2}", ModInfo.m_name, text, arg));
                result = null;
            }
            return result;
        }

        [XmlIgnore]
        private static readonly string configurationPath = Path.Combine(DataLocation.localApplicationData, "NoProblemOptions.xml");

        public bool RemoveExistingProblems;

        public bool[] ProblemOptions = new bool[50];
    }
}

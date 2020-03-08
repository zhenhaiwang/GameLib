using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;

namespace CE
{
    public enum CEKeyType
    {
        Int = 0,
        String,
    }

    public sealed class CEManager : Singleton<CEManager>
    {
        private Dictionary<string, Dictionary<string, ICELoader>> m_SheetStringDict = new Dictionary<string, Dictionary<string, ICELoader>>();
        private Dictionary<string, Dictionary<int, ICELoader>> m_SheetIntDict = new Dictionary<string, Dictionary<int, ICELoader>>();

        private const string JSON_PATH = "CEJson/";

        protected override void OnInit()
        {
            Clear();
            AutoLoad();
        }

        public void Reload()
        {
#if UNITY_EDITOR
            Clear();
            AutoLoad();
#endif
        }

        public bool Load(string sheetName, CEKeyType keyType = CEKeyType.Int)
        {
            bool needLoad = true;

            if ((keyType == CEKeyType.String && m_SheetStringDict.ContainsKey(sheetName)) ||
                (keyType == CEKeyType.Int && m_SheetIntDict.ContainsKey(sheetName)))
            {
                needLoad = false;
            }

            if (needLoad)
            {
                return Parse(sheetName, keyType);
            }

            Log.DebugFormat("[CEManager] load warning: sheet {0} already loaded", sheetName);

            return false;
        }

        public ICELoader GetElementString(string sheetName, string elementKey)
        {
            if (m_SheetStringDict.ContainsKey(sheetName))
            {
                var sheetDict = m_SheetStringDict[sheetName];

                if (sheetDict.ContainsKey(elementKey))
                {
                    return sheetDict[elementKey];
                }
            }

            return null;
        }

        public ICELoader GetElementInt(string sheetName, int elementKey)
        {
            if (m_SheetIntDict.ContainsKey(sheetName))
            {
                var sheetDict = m_SheetIntDict[sheetName];

                if (sheetDict.ContainsKey(elementKey))
                {
                    return sheetDict[elementKey];
                }
            }

            return null;
        }

        public Dictionary<string, ICELoader> GetDictString(string sheetName)
        {
            if (m_SheetStringDict.ContainsKey(sheetName))
            {
                return m_SheetStringDict[sheetName];
            }

            return null;
        }

        public Dictionary<int, ICELoader> GetDictInt(string sheetName)
        {
            if (m_SheetIntDict.ContainsKey(sheetName))
            {
                return m_SheetIntDict[sheetName];
            }

            return null;
        }

        private void Clear()
        {
            m_SheetStringDict.Clear();
            m_SheetIntDict.Clear();
        }

        private bool AutoLoad()
        {
            bool success = Parse(CEAutoLoad.CEName, CEKeyType.String);

            var sheetDict = CEAutoLoad.GetElementDict();

            foreach (KeyValuePair<string, ICELoader> kvp in sheetDict.CheckNull())
            {
                success &= Parse(kvp.Value as CEAutoLoad);
            }

            return success;
        }

        private uint BKDRHash(string str)
        {
            uint seed = 123;
            uint hash = 0;
            char[] seqs = str.ToCharArray();

            for (int i = 0; i < seqs.Length(); i++)
            {
                hash = (hash * seed + seqs[i]) & 0x7FFFFFFF;
            }

            return hash & 0x7FFFFFFF;
        }

        private bool Parse(CEAutoLoad sheetAutoLoad)
        {
            return Parse(sheetAutoLoad.SheetName, (CEKeyType)sheetAutoLoad.KeyType);
        }

        private bool Parse(string sheetName, CEKeyType keyType)
        {
            uint hash = BKDRHash(sheetName);

            try
            {
                var jsonText = Resources.Load(JSON_PATH + sheetName) as TextAsset;
                var jsonHt = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(jsonText.text);

                switch (keyType)
                {
                    case CEKeyType.Int:
                        {
                            m_SheetIntDict.Remove(sheetName);

                            var intDict = new Dictionary<int, ICELoader>();

                            foreach (object jsonKey in jsonHt.Keys)
                            {
                                string key = jsonKey as string;
                                string value = jsonHt[key].ToString();
                                var ht = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(value);

                                var loader = CEHashHelper.CreateLoaderFromHash(hash);
                                loader.Load(ht);

                                intDict.Add(int.Parse(key), loader);
                            }

                            m_SheetIntDict.Add(sheetName, intDict);
                        }
                        break;
                    case CEKeyType.String:
                        {
                            m_SheetStringDict.Remove(sheetName);

                            var stringDict = new Dictionary<string, ICELoader>();

                            foreach (object jsonKey in jsonHt.Keys)
                            {
                                string key = jsonKey as string;
                                string value = jsonHt[key].ToString();
                                var ht = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(value);

                                var loader = CEHashHelper.CreateLoaderFromHash(hash);
                                loader.Load(ht);

                                stringDict.Add(key, loader);
                            }

                            m_SheetStringDict.Add(sheetName, stringDict);
                        }
                        break;
                    default:
                        {
                            Log.Error("[CEManager] parse json error: unknown key type");
                        }
                        return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error("[CEManager] parse json exception: " + ex);
            }

            return false;
        }
    }
}
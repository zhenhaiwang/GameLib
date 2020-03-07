using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace GameLib
{
    public static class SerializeUtil
    {
        #region private

        private static bool CreateDirectoryIfNotExist(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetPath(string name)
        {
            string path = GetRoot() + "/ServerName/Uin/";
            CreateDirectoryIfNotExist(path);
            return path + name + ".sf";
        }

        private static string GetRoot()
        {
            string rootPath = string.Empty;

#if UNITY_EDITOR
            rootPath = Application.dataPath + "/../SerializedFiles/";
#elif UNITY_ANDROID
            if (string.IsNullOrEmpty(Application.persistentDataPath))
                rootPath = "/sdcard/Android/data/com.gamelib.serialized/files/";
            else
                rootPath = Application.persistentDataPath + "/";
#elif UNITY_IPHONE
            rootPath = Application.temporaryCachePath + "/"; // Application.persistentDataPath
#endif
            CreateDirectoryIfNotExist(rootPath);

            return rootPath;
        }

        #endregion

        public static bool Serialize(object data, string key)
        {
            try
            {
                if (data != null && !string.IsNullOrEmpty(key))
                {
                    File.WriteAllText(GetPath(key), JsonConvert.SerializeObject(data));

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Serialize(int data, string key)
        {
            return Serialize(data.ToString(), key);
        }

        public static bool Serialize(byte[] data, string key)
        {
            try
            {
                using (var writer = new BinaryWriter(File.Open(GetPath(key), FileMode.Create)))
                {
                    writer.Write(data);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T Deserialize<T>(string key) where T : class
        {
            try
            {
                string path = GetPath(key);

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);

                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Deserialize(string key, out int result)
        {
            return int.TryParse(Deserialize<string>(key), out result);
        }

        public static byte[] Deserialize(string key)
        {
            try
            {
                string path = GetPath(key);

                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public abstract class Serializable<T> where T : Serializable<T>, new()
    {
        public string key { get; set; }

        public static T Load(string loadKey)
        {
            T local = SerializeUtil.Deserialize<T>(loadKey);

            if (local == null)
            {
                local = new T();
                local.key = loadKey;
                local.OnInit(false);
            }
            else
            {
                local.key = loadKey;
                local.OnInit(true);
            }

            return local;
        }

        public virtual bool Save(string saveKey = null)
        {
            return string.IsNullOrEmpty(saveKey) ?
                SerializeUtil.Serialize(this, key) :
                SerializeUtil.Serialize(this, saveKey);
        }

        public void Clear()
        {
            OnClear();
        }

        protected virtual void OnInit(bool loadFromFile) { }

        protected virtual void OnClear() { }
    }
}
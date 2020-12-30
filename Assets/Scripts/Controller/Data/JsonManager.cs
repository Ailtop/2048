using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public static class JsonManager
{
    private static bool initialized;
    private static Dictionary<string, object> jsonDataFiles;

    private static string FileDirectory()
    {
#if UNITY_EDITOR
        return $"{Application.dataPath}/StoredData";
#else
        return $"{Application.persistentDataPath}/StoredData";
#endif
    }

    private static string FilePath(string fileName) => FileDirectory() + $"/{fileName}.json";

    private static void Init()
    {
        if (initialized) return;
        initialized = true;
        CheckDataPath();
        jsonDataFiles = new Dictionary<string, object>();
    }

    private static void CheckDataPath()
    {
        var path = FileDirectory();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void SaveFile<T>(string fileName, T data) where T : new()
    {
        Init();
        jsonDataFiles[fileName] = data;
        var serializeObject = JsonConvert.SerializeObject(data, Formatting.None);
        var dataPath = FilePath(fileName);
        File.WriteAllText(dataPath, serializeObject);
    }

    public static T LoadFile<T>(string fileName) where T : new()
    {
        Init();
        if (jsonDataFiles.TryGetValue(fileName, out var value))
        {
            return value is T t ? t : new T();
        }
        T data;
        var dataPath = FilePath(fileName);
        if (!File.Exists(dataPath))
        {
            data = new T();
            SaveFile(fileName, data);
        }
        else
        {
            try
            {
                var readText = File.ReadAllText(dataPath);
                data = JsonConvert.DeserializeObject<T>(readText);
            }
            catch
            {
                data = new T();
            }
            jsonDataFiles.Add(fileName, data);
        }
        return data;
    }
}
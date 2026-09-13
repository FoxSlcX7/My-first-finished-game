using System.IO;
using UnityEngine;

public class JsonSaveSerializer : ISaveSerializer
{
    public string FileExtension => "json";

    public void Save<T>(string filePath, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    public T Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"[JsonSaveSerializer] Файл не найден: {filePath}");
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<T>(json);
    }
}
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static SaveData Data { get; private set; }

    // Путь к файлу сохранения (универсальный для всех платформ)
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    // Загружаем сохранение ДО старта сцены
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Load();
    }

    public static void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("💾 SaveSystem: сохранение загружено");
            }
            catch (System.Exception e)
            {
                // Edge case из плана: повреждённое сохранение не должно ронять игру
                Debug.LogError($"💾 Сохранение повреждено, создаю новое: {e.Message}");
                Data = new SaveData();
            }
        }
        else
        {
            Data = new SaveData();
            Debug.Log("💾 SaveSystem: сохранения нет, создано новое");
        }
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(Data, true); // true = красивый формат
        File.WriteAllText(SavePath, json);
        Debug.Log("💾 SaveSystem: игра сохранена");
    }

    public static void UnlockSpell(string spellId)
    {
        if (!Data.unlockedSpells.Contains(spellId))
        {
            Data.unlockedSpells.Add(spellId);
        }
    }
}
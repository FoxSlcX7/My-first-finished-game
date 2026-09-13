using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static SaveData Data { get; private set; }

    // Стратегия сериализации по умолчанию (JSON).
    // При переходе на бинарный формат достаточно сменить реализацию здесь.
    private static ISaveSerializer _serializer = new JsonSaveSerializer();

    private static string SavePath => Path.Combine(Application.persistentDataPath, $"save.{_serializer.FileExtension}");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Load();
    }

    public static void SetSerializer(ISaveSerializer serializer)
    {
        if (serializer == null) return;
        _serializer = serializer;
    }

    public static void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                Data = _serializer.Load<SaveData>(SavePath);
                Debug.Log($"💾 SaveSystem: сохранение загружено ({_serializer.FileExtension})");
            }
            catch (System.Exception e)
            {
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
        try
        {
            _serializer.Save(SavePath, Data);
            Debug.Log($"💾 SaveSystem: игра сохранена ({_serializer.FileExtension})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"💾 Ошибка при сохранении игры: {e.Message}");
        }
    }

    public static void UnlockSpell(string spellId)
    {
        if (!Data.unlockedSpells.Contains(spellId))
        {
            Data.unlockedSpells.Add(spellId);
        }
    }
}
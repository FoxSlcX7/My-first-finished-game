using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    // Событие, на которое будут подписываться все тексты на сцене
    public event Action OnLanguageChanged;

    private Dictionary<string, string> _localizedText;

    [Header("Settings")]
    [SerializeField] private string _currentLanguage = "ru";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(_currentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLanguage(string langCode)
    {
        _currentLanguage = langCode;
        _localizedText = new Dictionary<string, string>();

        // Загружаем JSON из папки Resources
        TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{langCode}");
        if (textAsset != null)
        {
            LocalizationData data = JsonUtility.FromJson<LocalizationData>(textAsset.text);
            foreach (var item in data.items)
            {
                _localizedText[item.key] = item.value;
            }
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"[LocalizationManager] Файл локализации не найден для языка: {langCode}");
        }
    }

    public string GetText(string key)
    {
        if (_localizedText != null && _localizedText.TryGetValue(key, out string value))
        {
            return value;
        }
        return $"[{key}]"; // Если ключа нет, возвращаем сам ключ в скобках для отладки
    }
}

// Обертки для правильной сериализации JSON в Unity
[System.Serializable]
public class LocalizationData
{
    public List<LocalizationItem> items;
}

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("Ключ из JSON файла (например: menu_play)")]
    [SerializeField] private string _localizationKey;

    private TextMeshProUGUI _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
        }
        else
        {
            Debug.LogWarning("[LocalizedText] LocalizationManager не найден на сцене!");
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    private void UpdateText()
    {
        _textComponent.text = LocalizationManager.Instance.GetText(_localizationKey);
    }
}
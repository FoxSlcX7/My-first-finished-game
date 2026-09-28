using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения итогов забега на GameOverPanel и VictoryPanel.
/// </summary>
public class UIRunSummary : MonoBehaviour
{
    [Header("Текстовые поля статистики")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI runesText;

    [Header("Кнопки")]
    [SerializeField] private Button hubButton;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if (hubButton != null)
            hubButton.onClick.AddListener(() => GameManager.Instance?.ReturnToHub());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.Restart());
    }

    private void OnEnable()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (floorText != null)
            floorText.text = $"Этаж: {StatsTracker.LastRunFloor} / 10";

        if (killsText != null)
            killsText.text = $"Врагов уничтожено: {StatsTracker.LastRunKills}";

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(StatsTracker.LastRunDuration / 60f);
            int seconds = Mathf.FloorToInt(StatsTracker.LastRunDuration % 60f);
            timeText.text = $"Время забега: {minutes:00}:{seconds:00}";
        }

        if (runesText != null)
        {
            int runes = StatsTracker.LastRunEarnedStones;
            runesText.text = runes > 0 ? $"+{runes} Рунных Камней" : "0 Рунных Камней (быстрый выход)";
        }
    }
}
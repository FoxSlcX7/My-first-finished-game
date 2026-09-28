using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIPausePanel : MonoBehaviour
{
    public static UIPausePanel Instance { get; private set; }
    public bool IsPaused { get; private set; }

    [Header("UI элементы")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button hubButton;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                GameManager.Instance?.Restart();
            });

        if (hubButton != null)
            hubButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                GameManager.Instance?.ReturnToHub();
            });

        // Скрываем панель на старте, но объект оставляем активным для Update()
        SetVisible(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!IsPaused && CanOpenPause())
            {
                Pause();
            }
            else if (IsPaused)
            {
                Resume();
            }
        }
    }

    private bool CanOpenPause()
    {
        // Не открываем поверх драфта апгрейдов или меню алтаря
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.IsShowing) return false;
        if (UIAltarChoice.IsShowing) return false;

        // Если игра уже остановлена (GameOver / Victory), паузу не вызываем
        if (Time.timeScale <= 0f) return false;

        return true;
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetVisible(true);
    }

    public void Resume()
    {
        IsPaused = false;
        SetVisible(false);
        Time.timeScale = 1f;
    }

    private void SetVisible(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = visible;
        _canvasGroup.interactable = visible;
    }
}
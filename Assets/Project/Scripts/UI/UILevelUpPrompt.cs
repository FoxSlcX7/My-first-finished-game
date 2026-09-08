using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD-индикатор непотраченных уровней: пульсирующая кнопка.
/// Данные — только через глобальную шину (без опроса в Update).
/// </summary>
public class UILevelUpPrompt : MonoBehaviour
{
    [SerializeField] private Button openButton;
    [SerializeField] private TextMeshProUGUI countLabel;
    [SerializeField] private float pulseSpeed = 4f;

    private CanvasGroup _canvasGroup;
    private int _pending;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (openButton != null)
            openButton.onClick.AddListener(OnClicked);

        SetVisible(false);
    }

    private void OnEnable()
    {
        GameEvents.OnLevelUpPending?.AddListener(HandlePending);
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUpPending?.RemoveListener(HandlePending);
    }

    private void HandlePending(int pending)
    {
        _pending = pending;
        SetVisible(pending > 0);
        if (countLabel != null)
            countLabel.text = pending > 1 ? $"x{pending}" : "";
    }

    private void OnClicked() => UpgradeManager.Instance?.OpenUpgradePanel();

    private void Update()
    {
        if (_pending <= 0) return;
        // Пульс — только анимация привлечения внимания, не опрос данных
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        _canvasGroup.alpha = Mathf.Lerp(0.6f, 1f, t);
    }

    private void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = visible;
        _canvasGroup.interactable = visible;
    }
}
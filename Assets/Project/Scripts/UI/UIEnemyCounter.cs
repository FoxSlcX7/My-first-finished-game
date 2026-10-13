using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Плашка счетчика оставшихся врагов на боевой арене.
/// Автоматически появляется при начале боя в комнате и скрывается при победе.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIEnemyCounter : MonoBehaviour
{
    #region Inspector Settings

    [Header("UI Ссылки")]
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private string prefix = "Враги: ";

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.35f;

    #endregion

    #region Private Fields

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeRoutine;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        SetAlpha(0f); // Скрыт на старте
    }

    private void OnEnable()
    {
        RoomController.OnWaveEnemiesCountChanged += HandleEnemiesCountChanged;
    }

    private void OnDisable()
    {
        RoomController.OnWaveEnemiesCountChanged -= HandleEnemiesCountChanged;
    }

    #endregion

    #region Logic & Animation

    private void HandleEnemiesCountChanged(int remaining)
    {
        if (remaining > 0)
        {
            if (counterText != null)
            {
                counterText.text = $"{prefix}{remaining}";
            }
            FadeTo(1f);
        }
        else
        {
            FadeTo(0f);
        }
    }

    private void FadeTo(float targetAlpha)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.blocksRaycasts = alpha > 0.01f;
    }

    #endregion
}
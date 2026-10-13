using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class UIControlsHint : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Через сколько секунд подсказка начнет плавно исчезать")]
    [SerializeField] private float autoHideDelay = 7f;
    [Tooltip("Длительность плавного затухания")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeRoutine;
    private bool _isVisible = true;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Запускаем таймер автоматического скрытия на старте забега
        if (autoHideDelay > 0f)
        {
            _fadeRoutine = StartCoroutine(AutoHideRoutine());
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // По клавише H можно скрыть или снова показать подсказку
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            ToggleHint();
        }
    }

    public void ToggleHint()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _isVisible = !_isVisible;
        _fadeRoutine = StartCoroutine(FadeRoutine(_isVisible ? 1f : 0f));
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(autoHideDelay);
        _isVisible = false;
        yield return FadeRoutine(0f);
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        _canvasGroup.blocksRaycasts = targetAlpha > 0.01f;
    }
}
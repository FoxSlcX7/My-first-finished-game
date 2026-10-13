using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    #region Serialized Fields

    [Header("Visual Components")]
    [Tooltip("Ссылка на SpriteRenderer сундука")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation Settings")]
    [Tooltip("Кадры покадровой анимации открытия сундука")]
    [SerializeField] private Sprite[] openAnimationFrames;

    [Tooltip("Скорость анимации открытия (кадров в секунду)")]
    [SerializeField] private float frameRate = 10f;

    [Header("Bounce Effect")]
    [Tooltip("Длительность bounce-эффекта масштабирования (в секундах)")]
    [SerializeField] private float bounceDuration = 0.35f;

    [Tooltip("Коэффициент увеличения размера при bounce-эффекте (0.18 = 18%)")]
    [SerializeField] private float bounceScaleMultiplier = 0.18f;

    [Header("Rewards")]
    [Tooltip("Количество сфер опыта, выпадающих из сундука")]
    [SerializeField] private int xpOrbsCount = 5;

    [Tooltip("Радиус разброса сфер опыта вокруг сундука")]
    [SerializeField] private float xpDropRadius = 0.8f;

    #endregion

    #region State

    private bool _opened;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened || !other.CompareTag("Player")) return;

        OpenChest();
    }

    #endregion

    #region Chest Opening Logic

    private void OpenChest()
    {
        _opened = true;

        // Отключаем коллайдер, чтобы исключить повторное срабатывание
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(OpenChestRoutine());
    }

    private IEnumerator OpenChestRoutine()
    {
        // Запуск bounce-эффекта
        Coroutine bounceCoroutine = StartCoroutine(BounceRoutine());

        // Покадровая анимация открытия
        if (openAnimationFrames != null && openAnimationFrames.Length > 0)
        {
            float frameInterval = frameRate > 0f ? 1f / frameRate : 0.1f;
            for (int i = 0; i < openAnimationFrames.Length; i++)
            {
                if (spriteRenderer != null && openAnimationFrames[i] != null)
                {
                    spriteRenderer.sprite = openAnimationFrames[i];
                }
                yield return new WaitForSeconds(frameInterval);
            }
        }
        else if (spriteRenderer != null)
        {
            // Фолбэк на цвет, если спрайты анимации не назначены
            spriteRenderer.color = Color.gray;
        }

        // Ожидаем завершения bounce-эффекта
        if (bounceCoroutine != null)
        {
            yield return bounceCoroutine;
        }

        // Выдача наград после завершения анимации
        GrantRewards();
    }

    private IEnumerator BounceRoutine()
    {
        Vector3 initialScale = transform.localScale;
        Vector3 peakScale = initialScale * (1f + bounceScaleMultiplier);
        float halfDuration = bounceDuration * 0.5f;

        if (halfDuration <= 0f)
        {
            yield break;
        }

        // Увеличение масштаба (+18%)
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(initialScale, peakScale, smoothT);
            yield return null;
        }

        // Плавный возврат к исходному масштабу
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(peakScale, initialScale, smoothT);
            yield return null;
        }

        transform.localScale = initialScale;
    }

    #endregion

    #region Rewards Logic

    private void GrantRewards()
    {
        UpgradeManager.Instance?.GrantUpgradeOffer();
        SpawnExperienceOrbs();

        Debug.Log("[Chest] Открыт: начислен выбор апгрейда и опыт.");
    }

    private void SpawnExperienceOrbs()
    {
        if (XPDropper.Instance == null) return;

        for (int i = 0; i < xpOrbsCount; i++)
        {
            float angle = (i * (360f / xpOrbsCount) + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(xpDropRadius * 0.6f, xpDropRadius);
            Vector2 spawnPosition = (Vector2)transform.position + offset;
            XPDropper.Instance.Drop(spawnPosition);
        }
    }

    #endregion
}

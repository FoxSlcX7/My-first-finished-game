using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossHealthBar : MonoBehaviour
{
    [Header("Элементы UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Health _trackedBossHealth;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (healthSlider != null)
        {
            healthSlider.interactable = false;
            Navigation nav = healthSlider.navigation;
            nav.mode = Navigation.Mode.None;
            healthSlider.navigation = nav;
        }

        SetVisibleInstant(false);
    }

    private void OnEnable()
    {
        BossEvents.OnBossSpawned += HandleBossSpawned;
        BossEvents.OnBossPhaseChanged += HandlePhaseChanged;
        BossEvents.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BossEvents.OnBossSpawned -= HandleBossSpawned;
        BossEvents.OnBossPhaseChanged -= HandlePhaseChanged;
        BossEvents.OnBossDefeated -= HandleBossDefeated;

        if (_trackedBossHealth != null)
        {
            _trackedBossHealth.OnDamaged -= UpdateHealth;
        }
    }

    private void HandleBossSpawned(string bossName, Health health)
    {
        _trackedBossHealth = health;
        _trackedBossHealth.OnDamaged += UpdateHealth;

        if (bossNameText != null) bossNameText.text = bossName;
        if (healthSlider != null)
        {
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.CurrentHealth;
        }

        FadeTo(1f);
    }

    private void UpdateHealth(int damage)
    {
        if (_trackedBossHealth != null && healthSlider != null)
        {
            healthSlider.value = _trackedBossHealth.CurrentHealth;
        }
    }

    private void HandlePhaseChanged(int phase)
    {
        if (phaseText != null)
        {
            phaseText.text = phase == 3 ? "ФАЗА: ЯРОСТЬ" : $"ФАЗА {phase}";
            phaseText.color = phase == 3 ? Color.red : Color.yellow;
        }
    }

    private void HandleBossDefeated()
    {
        if (_trackedBossHealth != null)
        {
            _trackedBossHealth.OnDamaged -= UpdateHealth;
            _trackedBossHealth = null;
        }

        FadeTo(0f);
    }

    private void FadeTo(float targetAlpha)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target > 0.01f;
    }

    private void SetVisibleInstant(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
    }
}
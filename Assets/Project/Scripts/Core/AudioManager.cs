using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Фоновая музыка")]
    [SerializeField] private AudioClip hubMusic;
    [SerializeField] private AudioClip dungeonMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Звуки игрока и магии (SFX)")]
    [SerializeField] private AudioClip spellCastSFX;
    [SerializeField] private AudioClip comboCastSFX;
    [SerializeField] private AudioClip playerHurtSFX;
    [SerializeField] private AudioClip playerDeathSFX;
    [SerializeField] private AudioClip levelUpSFX;

    [Header("Окружение и враги (SFX)")]
    [SerializeField] private AudioClip crateBreakSFX;
    [SerializeField] private AudioClip enemyHitSFX;
    [SerializeField] private AudioClip enemyDeathSFX;
    [SerializeField] private AudioClip bossDefeatedSFX;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Автоматически создаем источники звука, если они не назначены вручную
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        // Подписка на глобальные события
        GameEvents.OnSpellCast?.AddListener(HandleSpellCast);
        GameEvents.OnComboCast?.AddListener(HandleComboCast);
        GameEvents.OnPlayerDamaged?.AddListener(HandlePlayerDamaged);
        GameEvents.OnPlayerDied?.AddListener(HandlePlayerDied);
        GameEvents.OnLevelUp?.AddListener(HandleLevelUp);

        BossEvents.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        GameEvents.OnSpellCast?.RemoveListener(HandleSpellCast);
        GameEvents.OnComboCast?.RemoveListener(HandleComboCast);
        GameEvents.OnPlayerDamaged?.RemoveListener(HandlePlayerDamaged);
        GameEvents.OnPlayerDied?.RemoveListener(HandlePlayerDied);
        GameEvents.OnLevelUp?.RemoveListener(HandleLevelUp);

        BossEvents.OnBossDefeated -= HandleBossDefeated;
    }

    // ═══════════════════════════════════════
    // Обработчики событий
    // ═══════════════════════════════════════

    private void HandleSpellCast(SpellSO spell) => PlaySFX(spellCastSFX, pitchVariation: 0.1f);
    private void HandleComboCast(SpellComboSO combo) => PlaySFX(comboCastSFX, volume: 1.2f);
    private void HandlePlayerDamaged(int damage) => PlaySFX(playerHurtSFX);
    private void HandlePlayerDied() => PlaySFX(playerDeathSFX);
    private void HandleLevelUp() => PlaySFX(levelUpSFX);
    private void HandleBossDefeated() => PlaySFX(bossDefeatedSFX);

    // ═══════════════════════════════════════
    // Публичные методы воспроизведения
    // ═══════════════════════════════════════

    /// <summary>
    /// Воспроизведение звукового эффекта с вариацией тона (pitch), чтобы звуки не звучали монотонно.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariation = 0.05f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayCrateBreakSFX() => PlaySFX(crateBreakSFX, pitchVariation: 0.15f);
    public void PlayEnemyHitSFX() => PlaySFX(enemyHitSFX, volume: 0.8f, pitchVariation: 0.1f);

    public void PlayMusic(AudioClip clip, bool restartIfSame = false)
    {
        if (clip == null || musicSource == null) return;
        if (!restartIfSame && musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMasterVolume(float volume) => AudioListener.volume = Mathf.Clamp01(volume);
}
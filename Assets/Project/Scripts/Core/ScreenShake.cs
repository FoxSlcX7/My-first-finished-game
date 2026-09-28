using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [SerializeField] private float defaultAmplitude = 1.5f;
    [SerializeField] private float defaultDuration = 0.2f;

    private CinemachineBasicMultiChannelPerlin _noise;
    private float _timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (_noise != null)
        {
            _noise.AmplitudeGain = 0f;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDamaged?.AddListener(HandlePlayerDamaged);
        GameEvents.OnComboCast?.AddListener(HandleComboCast);
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDamaged?.RemoveListener(HandlePlayerDamaged);
        GameEvents.OnComboCast?.RemoveListener(HandleComboCast);
    }

    private void HandlePlayerDamaged(int damage)
    {
        Shake(defaultAmplitude, defaultDuration);
    }

    private void HandleComboCast(SpellComboSO combo)
    {
        // Мощный сочный импульс при разряде комбо
        Shake(2.5f, 0.35f);
    }

    public void Shake(float amplitude, float duration)
    {
        if (_noise == null) return;
        _noise.AmplitudeGain = amplitude;
        _timer = duration;
    }

    public void Shake()
    {
        Shake(defaultAmplitude, defaultDuration);
    }

    private void Update()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f && _noise != null)
            {
                _noise.AmplitudeGain = 0f;
            }
        }
    }
}
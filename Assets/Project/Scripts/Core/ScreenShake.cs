using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float defaultAmplitude = 1.5f;
    [SerializeField] private float defaultDuration = 0.2f;

    private CinemachineBasicMultiChannelPerlin _noise;
    private float _timer;

    private void Awake()
    {
        _noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (_noise != null)
        {
            _noise.AmplitudeGain = 0f;
        }
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
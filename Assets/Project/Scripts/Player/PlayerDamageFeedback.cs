using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [SerializeField] private ScreenShake screenShake;

    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(int damage)
    {
        if (screenShake != null)
        {
            screenShake.Shake();
        }
    }
}
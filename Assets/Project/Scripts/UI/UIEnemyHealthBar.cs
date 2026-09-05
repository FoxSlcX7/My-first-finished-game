using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Health _health;

    private void Awake()
    {
        _health = GetComponentInParent<Health>();

        if (_health != null)
        {
            _health.OnDamaged += HandleDamaged;
        }
    }

    private void Start()
    {
        UpdateBar(); // начальное состояние (полный бар)
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(int damage)
    {
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (_health == null) return;
        fillImage.fillAmount = Mathf.Clamp01((float)_health.CurrentHealth / _health.MaxHealth);
    }
}
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private float _flashTimer;
    private Health _health;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
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
        CallDamageFlash();
    }

    /// <summary>
    /// Публичный метод вызова вспышки (для разрушаемых объектов и внешних скриптов)
    /// </summary>
    public void CallDamageFlash()
    {
        _flashTimer = flashDuration;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = flashColor;
        }
    }

    private void Update()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f && _spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private float _flashTimer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(int damage)
    {
        _flashTimer = flashDuration;
        _spriteRenderer.color = flashColor;
    }

    private void Update()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}
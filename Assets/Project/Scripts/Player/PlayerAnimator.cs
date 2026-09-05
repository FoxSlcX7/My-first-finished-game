using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ссылка на SpellCaster. Если не указать, возьмет с этого же объекта.")]
    [SerializeField] private SpellCaster spellCaster;

    [Header("Animation Settings")]
    [SerializeField] private float speedSmoothTime = 0.1f;

    private Animator _animator;
    private Rigidbody2D _rb;
    private Health _health;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int CastHash = Animator.StringToHash("Cast");

    private float _speedSmoothVelocity;
    private bool _isDead = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();

        if (spellCaster == null)
            spellCaster = GetComponent<SpellCaster>();
    }

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDeath += HandleDeath;
        }

        if (spellCaster != null)
        {
            spellCaster.OnSpellCast += HandleSpellCast;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDeath -= HandleDeath;
        }

        if (spellCaster != null)
        {
            spellCaster.OnSpellCast -= HandleSpellCast;
        }
    }

    private void Update()
    {
        if (_isDead) return;

        float targetSpeed = _rb.linearVelocity.magnitude;
        float currentAnimatedSpeed = _animator.GetFloat(SpeedHash);
        float smoothedSpeed = Mathf.SmoothDamp(currentAnimatedSpeed, targetSpeed, ref _speedSmoothVelocity, speedSmoothTime);

        _animator.SetFloat(SpeedHash, smoothedSpeed);
    }

    private void HandleDamaged(int damage)
    {
        if (_isDead) return;
        _animator.SetTrigger(HitHash);
    }

    private void HandleDeath()
    {
        _isDead = true;
        _animator.SetBool(IsDeadHash, true);
        enabled = false;
    }

    private void HandleSpellCast()
    {
        if (_isDead) return;
        _animator.SetTrigger(CastHash);
    }
}
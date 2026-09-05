using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private SpellCaster spellCaster;

    [Header("Knockback")]
    [Range(0f, 1f)][SerializeField] private float knockbackResistance = 0.3f;
    [SerializeField] private float knockbackCooldown = 0.2f;
    private float _nextKnockbackTime;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _aimInput;
    private Camera _mainCamera;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        GameManager.Instance?.RegisterPlayer(transform);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        Vector2 screenPos = context.ReadValue<Vector2>();
        _aimInput = _mainCamera.ScreenToWorldPoint(screenPos);
    }

    public void OnCastSlot1(InputAction.CallbackContext context)
    {
        if (context.started) spellCaster?.CastSlot1();
    }

    public void OnCastSlot2(InputAction.CallbackContext context)
    {
        if (context.started) spellCaster?.CastSlot2();
    }

    public void OnCastCombo(InputAction.CallbackContext context)
    {
        if (context.started) spellCaster?.CastCombo();
    }

    /// <summary>
    /// Применяет отбрасывание к игроку (вызывается вражескими снарядами).
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (Time.time < _nextKnockbackTime) return;

        float actualForce = force * (1f - knockbackResistance);
        if (actualForce <= 0f) return;

        _rb.AddForce(direction.normalized * actualForce, ForceMode2D.Impulse);
        _nextKnockbackTime = Time.time + knockbackCooldown;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = _moveInput * moveSpeed;
        Vector2 velocityChange = targetVelocity - _rb.linearVelocity;
        _rb.AddForce(velocityChange * acceleration, ForceMode2D.Force);
    }

    private void Update()
    {
        Vector2 aimDirection = _aimInput - (Vector2)transform.position;
        if (aimDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
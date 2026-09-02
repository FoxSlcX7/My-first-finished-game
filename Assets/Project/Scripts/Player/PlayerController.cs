using UnityEngine;
using UnityEngine.InputSystem;

// Гарантирует, что на объекте всегда есть Rigidbody2D
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private SpellCaster spellCaster;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _aimInput;
    private Camera _mainCamera;

    // Кэшируем компоненты при старте, чтобы не искать каждый кадр
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
    }

    // Получаем вектор движения из Input System (WASD / стрелки)
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // Конвертируем позицию мыши из экранных координат в мировые
    public void OnAim(InputAction.CallbackContext context)
    {
        Vector2 screenPos = context.ReadValue<Vector2>();
        _aimInput = _mainCamera.ScreenToWorldPoint(screenPos);
    }

    public void OnCastSlot1(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            spellCaster?.CastSlot1();
        }
    }

    public void OnCastSlot2(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            spellCaster?.CastSlot2();
        }
    }

    // Физика движения — в FixedUpdate для стабильности
    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput * moveSpeed;
    }

    // Поворот спрайта в сторону курсора
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
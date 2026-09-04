using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] private float force = 15f;
    [SerializeField] private float duration = 0.15f;

    private Rigidbody2D _rb;
    private float _timer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 direction)
    {
        if (_rb == null) return;
        _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        _timer = duration;
    }

    private void FixedUpdate()
    {
        if (_timer > 0)
            _timer -= Time.fixedDeltaTime;
    }
}
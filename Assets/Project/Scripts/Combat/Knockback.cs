using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] private float force = 8f;
    [SerializeField] private float duration = 0.12f;

    private Vector2 _direction;
    private float _timer;

    public void Apply(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = duration;
    }

    private void Update()
    {
        if (_timer > 0f)
        {
            transform.position += (Vector3)(_direction * force * Time.deltaTime);
            _timer -= Time.deltaTime;
        }
    }
}
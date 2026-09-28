using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [SerializeField] private int xpValue = 1;
    [SerializeField] private float magnetRadius = 2.5f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float lifetime = 20f;

    private Transform _player;
    private float _timer;

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (_player == null) return;

        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        float sqrDist = toPlayer.sqrMagnitude;
        float sqrMagnetRadius = magnetRadius * magnetRadius;

        if (sqrDist < sqrMagnetRadius)
        {
            transform.position = Vector2.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<PlayerXP>()?.AddXP(xpValue);
        Destroy(gameObject);
    }

    public void MultiplyValue(float multiplier)
    {
        // Умножаем базовое значение префаба (например, 1) на множитель баланса
        xpValue = Mathf.Max(1, Mathf.RoundToInt(xpValue * multiplier));
    }
}
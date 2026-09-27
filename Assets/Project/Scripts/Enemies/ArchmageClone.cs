using UnityEngine;

[RequireComponent(typeof(Health))]
public class ArchmageClone : MonoBehaviour
{
    [SerializeField] private EnemyProjectile boltPrefab;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float shootDelay = 0.8f;
    [SerializeField] private Color illusionColor = new Color(0.6f, 0.4f, 1f, 0.65f);

    private Health _health;
    private Transform _player;
    private float _timer;
    private bool _hasShot;

    private void Awake()
    {
        _health = GetComponent<Health>();
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = illusionColor;
    }

    private void Start()
    {
        _player = GameManager.Instance?.PlayerTransform;
        _health.OnDeath += Dispel;
        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        if (_health != null) _health.OnDeath -= Dispel;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (!_hasShot && _timer >= shootDelay)
        {
            _hasShot = true;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (boltPrefab == null || _player == null) return;
        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        EnemyProjectile bolt = Instantiate(boltPrefab, transform.position, Quaternion.identity);
        bolt.Init(dir, 10);
    }

    private void Dispel()
    {
        // При смерти клона можно инстанциировать маленькую вспышку дыма
        Destroy(gameObject);
    }
}
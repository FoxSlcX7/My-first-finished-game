using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private int _projectilePoolSize = 50;

    private ObjectPool<Projectile> _projectilePool;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Transform poolParent = new GameObject("ProjectilePool").transform;
        poolParent.SetParent(transform);

        _projectilePool = new ObjectPool<Projectile>(_projectilePrefab, _projectilePoolSize, poolParent);
    }

    public Projectile GetProjectile()
    {
        return _projectilePool.Get();
    }

    public void ReturnProjectile(Projectile projectile)
    {
        _projectilePool.Return(projectile);
    }
}
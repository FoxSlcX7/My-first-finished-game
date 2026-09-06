using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Effect", menuName = "Arcane Arsenal/Spell Effect/Projectile")]
public class ProjectileSpellEffect : SpellEffectBase
{
    [Tooltip("Угол между снарядами при мультивыстреле (градусы).")]
    [SerializeField] private float spreadAngle = 12f;

    public override void Cast(Vector2 origin, Vector2 direction, SpellSO data)
    {
        int total = 1 + (PlayerStats.Instance != null ? PlayerStats.Instance.BonusProjectiles : 0);

        for (int i = 0; i < total; i++)
        {
            float offset = (i - (total - 1) * 0.5f) * spreadAngle;
            Vector2 dir = (Quaternion.Euler(0f, 0f, offset) * direction).normalized;
            SpawnProjectile(origin, dir, data);
        }
    }

    private void SpawnProjectile(Vector2 origin, Vector2 direction, SpellSO data)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        if (projectile == null) return;

        projectile.transform.position = origin;
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        projectile.Init(direction);
        projectile.SetStats(data.projectileSpeed, data.lifetime, PlayerStats.ScaleDamage(data.damage), data.knockbackForce);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = data.projectileColor;
        }
    }
}
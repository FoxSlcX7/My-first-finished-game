using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Effect", menuName = "Arcane Arsenal/Spell Effect/Projectile")]
public class ProjectileSpellEffect : SpellEffectBase
{
    public override void Cast(Vector2 origin, Vector2 direction, SpellSO data)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        if (projectile == null) return;

        projectile.transform.position = origin;
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        projectile.Init(direction);
        projectile.SetStats(data.projectileSpeed, data.lifetime, data.damage, data.knockbackForce);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = data.projectileColor;
        }
    }
}
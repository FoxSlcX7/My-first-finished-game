using UnityEngine;

public class RangedAttackState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public void Execute()
    {
        if (_enemy == null) return;

        // Гистерезис: выходим когда игрок ушёл на 120% от дистанции
        float exitRange = _enemy.Data.attackRange * 1.2f;
        if (_enemy.DistanceToPlayer() > exitRange)
        {
            _enemy.SetState(new ChaseState());
            return;
        }

        _enemy.StopMovement();

        // Пробуем выстрелить — кулдаун живёт в EnemyController
        if (_enemy.TryShoot())
        {
            Shoot();
        }
    }

    public void Exit() { }

    private void Shoot()
    {
        if (_enemy.Data.projectilePrefab == null) return;
        if (_enemy.Player == null) return;

        Vector2 direction = ((Vector2)_enemy.Player.position
                           - (Vector2)_enemy.transform.position).normalized;

        GameObject proj = Object.Instantiate(
            _enemy.Data.projectilePrefab,
            _enemy.transform.position,
            Quaternion.identity
        );

        EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Init(direction);
        }
    }
}
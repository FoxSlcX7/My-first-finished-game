public class ChaseState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public void Execute()
    {
        if (_enemy == null) return;

        if (_enemy.DistanceToPlayer() > _enemy.Data.detectionRange)
        {
            _enemy.SetState(new IdleState());
            return;
        }

        if (_enemy.DistanceToPlayer() <= _enemy.Data.attackRange)
        {
            _enemy.SetState(_enemy.GetAttackState()); // ← НОВОЕ
            return;
        }

        _enemy.MoveTowardsPlayer();
    }

    public void Exit()
    {
        _enemy?.StopMovement();
    }
}
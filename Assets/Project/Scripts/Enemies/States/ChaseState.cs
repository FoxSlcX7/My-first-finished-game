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

        // Игрок вышел из зоны — обратно в Idle
        if (_enemy.DistanceToPlayer() > _enemy.DetectionRange)
        {
            _enemy.SetState(new IdleState());
            return;
        }

        // Игрок в зоне атаки — атакуем
        if (_enemy.DistanceToPlayer() <= _enemy.AttackRange)
        {
            _enemy.SetState(new AttackState());
            return;
        }

        // Бежим к игроку
        _enemy.MoveTowardsPlayer();
    }

    public void Exit()
    {
        _enemy?.StopMovement();
    }
}
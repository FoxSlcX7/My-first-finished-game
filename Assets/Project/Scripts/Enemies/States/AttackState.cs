public class AttackState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public void Execute()
    {
        if (_enemy == null) return;
        if (_enemy.IsStaggered) return;

        if (_enemy.DistanceToPlayer() > _enemy.Data.attackRange)
        {
            _enemy.SetState(new ChaseState());
            return;
        }

        _enemy.TryDealContactDamage();
        _enemy.StopMovement();
    }

    public void Exit() { }
}
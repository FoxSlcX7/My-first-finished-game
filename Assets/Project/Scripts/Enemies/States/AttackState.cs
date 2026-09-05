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

        // Игрок отошёл из зоны атаки — преследуем
        if (_enemy.DistanceToPlayer() > _enemy.AttackRange)
        {
            _enemy.SetState(new ChaseState());
            return;
        }

        // Игрок вышел из зоны обнаружения — стоим
        if (_enemy.DistanceToPlayer() > _enemy.DetectionRange)
        {
            _enemy.SetState(new IdleState());
            return;
        }

        // Наносим контактный урон
        _enemy.TryDealContactDamage();

        // Стоим на месте при атаке (для melee)
        _enemy.StopMovement();
    }

    public void Exit() { }
}
public class IdleState : IEnemyState
{
    private EnemyController _enemy;
    private float _idleTimer;
    private float _idleDuration = 1f;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
        _idleTimer = _idleDuration;
    }

    public void Execute()
    {
        if (_enemy == null) return;

        _idleTimer -= UnityEngine.Time.deltaTime;

        // Если игрок рядом — переходим в Chase
        if (_enemy.DistanceToPlayer() <= _enemy.DetectionRange)
        {
            _enemy.SetState(new ChaseState());
            return;
        }

        // Если таймер кончился — просто стоим дальше (можно добавить патрулирование)
    }

    public void Exit() { }
}
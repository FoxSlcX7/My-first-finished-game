using UnityEngine;

public class IdleState : IEnemyState
{
    private EnemyController _enemy;
    private float _timer;
    private const float IDLE_DURATION = 1.5f;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
        _timer = IDLE_DURATION;
    }

    public void Execute()
    {
        if (_enemy == null) return;

        if (_enemy.DistanceToPlayer() <= _enemy.Data.detectionRange)
        {
            _enemy.SetState(new ChaseState());
            return;
        }

        _timer -= Time.deltaTime;
    }

    public void Exit() { }
}
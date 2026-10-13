using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
    }

    public void Execute()
    {
        if (_enemy == null || _enemy.Data == null) return;

        if (_enemy.Player == null)
        {
            _enemy.StopMovement();
            _enemy.SetState(new IdleState());
            return;
        }

        bool isInRoomArena = _enemy.transform.parent != null && _enemy.transform.parent.GetComponent<RoomController>() != null;

        if (!isInRoomArena && _enemy.DistanceToPlayer() > _enemy.Data.detectionRange)
        {
            _enemy.SetState(new IdleState());
            return;
        }

        if (_enemy.DistanceToPlayer() <= _enemy.Data.attackRange)
        {
            _enemy.SetState(_enemy.GetAttackState());
            return;
        }

        _enemy.MoveTowardsPlayer();
    }

    public void Exit()
    {
        _enemy?.StopMovement();
    }
}
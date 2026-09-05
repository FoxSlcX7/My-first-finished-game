public interface IEnemyState
{
    void Enter(EnemyController enemy);
    void Execute();
    void Exit();
}
public class DeadState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;

        // Отключаем физику и коллайдер
        if (_enemy.Rb != null)
        {
            _enemy.Rb.linearVelocity = UnityEngine.Vector2.zero;
            _enemy.Rb.angularVelocity = 0f;
            _enemy.Rb.simulated = false;
        }

        UnityEngine.Collider2D col = _enemy.GetComponent<UnityEngine.Collider2D>();
        if (col != null) col.enabled = false;

        // Отключаем все скрипты кроме Animator (для анимации смерти)
        UnityEngine.MonoBehaviour[] scripts = _enemy.GetComponents<UnityEngine.MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script is UnityEngine.Animator || script is Health) continue;
            script.enabled = false;
        }

        // Уничтожаем через задержку
        UnityEngine.Object.Destroy(_enemy.gameObject, 1.5f);
    }

    public void Execute() { }

    public void Exit() { }
}
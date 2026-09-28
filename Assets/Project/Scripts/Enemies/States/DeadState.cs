using System.Collections;
using UnityEngine;

public class DeadState : IEnemyState
{
    private EnemyController _enemy;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;

        // 1. Отключаем физику и коллайдер
        if (_enemy.Rb != null)
        {
            _enemy.Rb.linearVelocity = Vector2.zero;
            _enemy.Rb.angularVelocity = 0f;
            _enemy.Rb.simulated = false;
        }

        Collider2D col = _enemy.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. Отключаем все скрипты кроме Animator и Health
        MonoBehaviour[] scripts = _enemy.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script is Animator || script is Health) continue;
            script.enabled = false;
        }

        // 3. Запуск корутины растворения
        _enemy.StartCoroutine(DissolveRoutine());
    }

    public void Execute() { }
    public void Exit() { }

    private IEnumerator DissolveRoutine()
    {
        SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
        float duration = 0.6f;
        float elapsed = 0f;

        Color startColor = sr != null ? sr.color : Color.white;
        // Конечный цвет: пепельно-темный и полностью прозрачный
        Color targetColor = new Color(0.15f, 0.15f, 0.15f, 0f);

        // Небольшая задержка перед затуханием (чтобы сработала анимация/флеш урона)
        yield return new WaitForSeconds(0.15f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (sr != null)
            {
                sr.color = Color.Lerp(startColor, targetColor, t);
            }

            yield return null;
        }

        Object.Destroy(_enemy.gameObject);
    }
}
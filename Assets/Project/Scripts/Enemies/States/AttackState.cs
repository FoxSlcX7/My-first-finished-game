using System.Collections;
using UnityEngine;

public class AttackState : IEnemyState
{
    private EnemyController _enemy;
    private bool _isAttacking;
    private Coroutine _attackRoutine;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
        _isAttacking = false;
    }

    public void Execute()
    {
        if (_enemy == null || _enemy.Player == null) return;
        if (_enemy.IsStaggered)
        {
            CancelAttack();
            return;
        }

        if (!_isAttacking)
        {
            // Начинаем замах, только если игрок действительно близко
            if (_enemy.DistanceToPlayer() > _enemy.Data.attackRange * 1.15f)
            {
                _enemy.SetState(new ChaseState());
                return;
            }

            _attackRoutine = _enemy.StartCoroutine(TelegraphedAttackRoutine());
        }
    }

    public void Exit()
    {
        CancelAttack();
        _enemy?.StopMovement();
    }

    private IEnumerator TelegraphedAttackRoutine()
    {
        _isAttacking = true;
        _enemy.StopMovement();

        SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        Vector2 attackDirection = Vector2.right;
        if (_enemy.Player != null)
        {
            attackDirection = ((Vector2)_enemy.Player.position - (Vector2)_enemy.transform.position).normalized;
        }

        // 1. Замах: 0.32 сек моб краснеет
        float windupTime = 0.32f;
        float elapsed = 0f;

        while (elapsed < windupTime)
        {
            if (_enemy == null || _enemy.IsStaggered) yield break;

            elapsed += Time.deltaTime;
            if (sr != null)
            {
                sr.color = Color.Lerp(originalColor, new Color(1f, 0.25f, 0.25f, 1f), elapsed / windupTime);
            }
            yield return null;
        }

        if (sr != null) sr.color = originalColor;

        // 2. Короткий упругий выпад вперед (3f вместо прежних 5f)
        _enemy.Rb.AddForce(attackDirection * 3.0f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.06f);

        // ЧЕСТНЫЙ УРОН: достает только на 0.9 тайла прямо перед носом!
        _enemy.TryDealContactDamage(0.95f);

        // 3. Восстановление: моб открыт для ударов 0.28 сек
        _enemy.StopMovement();
        yield return new WaitForSeconds(0.28f);

        _isAttacking = false;
        _enemy.SetState(new ChaseState());
    }

    private void CancelAttack()
    {
        if (_attackRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        if (_enemy != null)
        {
            SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && _enemy.Data != null) sr.color = _enemy.Data.color;
        }

        _isAttacking = false;
    }
}
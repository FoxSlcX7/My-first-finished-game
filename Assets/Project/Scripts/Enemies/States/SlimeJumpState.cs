using System.Collections;
using UnityEngine;

public class SlimeJumpState : IEnemyState
{
    private EnemyController _enemy;
    private Coroutine _jumpRoutine;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
        _jumpRoutine = _enemy.StartCoroutine(SlimeAttackRoutine());
    }

    public void Execute()
    {
        if (_enemy == null || _enemy.IsStaggered)
        {
            CancelJump();
        }
    }

    public void Exit()
    {
        CancelJump();
        _enemy?.StopMovement();
    }

    private IEnumerator SlimeAttackRoutine()
    {
        _enemy.StopMovement();

        SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
        Transform spriteTransform = sr != null ? sr.transform : _enemy.transform;
        Vector3 baseScale = _enemy.Data.spriteScale;

        // Честный расчет направления от центра к центру с защитой от нулевого вектора
        Vector2 origin = _enemy.GetComponent<Collider2D>() != null
            ? (Vector2)_enemy.GetComponent<Collider2D>().bounds.center
            : (Vector2)_enemy.transform.position;

        Vector2 target = _enemy.Player != null
            ? (Vector2)_enemy.Player.position
            : origin;

        if (_enemy.Player != null && _enemy.Player.GetComponent<Collider2D>() != null)
        {
            target = _enemy.Player.GetComponent<Collider2D>().bounds.center;
        }

        Vector2 diff = target - origin;
        // Если игрок стоит прямо в слайме — прыгаем немного в сторону
        Vector2 jumpDir = diff.sqrMagnitude > 0.01f ? diff.normalized : Vector2.down;

        // 1. ФАЗА ПОДГОТОВКИ: сплющивание + предупреждающая подсветка
        float prepTime = 0.35f;
        float elapsed = 0f;
        Color originalColor = sr != null ? sr.color : Color.white;
        Color attackWarningColor = new Color(1f, 0.25f, 0.25f, 1f); // Красный накал перед атакой

        while (elapsed < prepTime)
        {
            if (_enemy == null || _enemy.IsStaggered) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / prepTime;

            spriteTransform.localScale = new Vector3(baseScale.x * (1f + t * 0.35f), baseScale.y * (1f - t * 0.35f), baseScale.z);
            if (sr != null)
            {
                sr.color = Color.Lerp(originalColor, attackWarningColor, t);
            }
            yield return null;
        }

        if (sr != null) sr.color = originalColor;

        // 2. ФАЗА ПРЫЖКА
        spriteTransform.localScale = new Vector3(baseScale.x * 0.85f, baseScale.y * 1.25f, baseScale.z);
        _enemy.Rb.linearVelocity = Vector2.zero;
        _enemy.Rb.AddForce(jumpDir * 9.5f, ForceMode2D.Impulse);

        float flightTime = 0.28f;
        float flightElapsed = 0f;
        bool dealtDamage = false;

        while (flightElapsed < flightTime)
        {
            if (_enemy == null || _enemy.IsStaggered) yield break;
            flightElapsed += Time.deltaTime;

            // Проверяем урон через край коллайдера, а не расстояние между центрами
            if (!dealtDamage)
            {
                dealtDamage = _enemy.TryDealContactDamage();
            }
            yield return null;
        }

        // 3. ПРИЗЕМЛЕНИЕ
        _enemy.StopMovement();
        spriteTransform.localScale = new Vector3(baseScale.x * 1.2f, baseScale.y * 0.8f, baseScale.z);

        if (!dealtDamage)
        {
            _enemy.TryDealContactDamage();
        }

        yield return new WaitForSeconds(0.1f);
        spriteTransform.localScale = baseScale;

        // 4. ПАУЗА ПОСЛЕ ПРЫЖКА: даем игроку окно для атаки и предотвращаем спам
        yield return new WaitForSeconds(0.4f);

        _enemy.SetState(new ChaseState());
    }

    private void CancelJump()
    {
        if (_jumpRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_jumpRoutine);
            _jumpRoutine = null;
        }

        if (_enemy != null && _enemy.Data != null)
        {
            SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.transform.localScale = _enemy.Data.spriteScale;
                sr.color = _enemy.Data.color;
            }
        }
    }
}
using System.Collections;
using UnityEngine;

public class RangedAttackState : IEnemyState
{
    private EnemyController _enemy;
    private bool _isChargingShot;
    private Coroutine _chargeRoutine;

    private const float RETREAT_DISTANCE = 3.5f;
    private const float OBSTACLE_PROBE_DIST = 1.0f;

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;
        _isChargingShot = false;
    }

    public void Execute()
    {
        if (_enemy == null || _enemy.Player == null) return;
        if (_enemy.IsStaggered)
        {
            CancelCharge();
            return;
        }

        float dist = _enemy.DistanceToPlayer();
        float exitRange = _enemy.Data.attackRange * 1.3f;

        // 1. Если игрок убежал далеко — догоняем
        if (dist > exitRange && !_isChargingShot)
        {
            CancelCharge();
            _enemy.SetState(new ChaseState());
            return;
        }

        // 2. Стрельба по кулдауну
        if (!_isChargingShot && _enemy.TryShoot())
        {
            _chargeRoutine = _enemy.StartCoroutine(TelegraphedShotRoutine());
            return;
        }

        // 3. Кайт / отступление, когда не заряжаем выстрел
        if (!_isChargingShot && dist < RETREAT_DISTANCE)
        {
            RetreatFromPlayer();
        }
        else if (!_isChargingShot)
        {
            _enemy.StopMovement();
        }
    }

    public void Exit()
    {
        CancelCharge();
        _enemy?.StopMovement();
    }

    private void RetreatFromPlayer()
    {
        Vector2 origin = _enemy.transform.position;
        Vector2 dirAway = (origin - (Vector2)_enemy.Player.position).normalized;

        Vector2 smartMoveDir = GetSmartRetreatDirection(origin, dirAway);

        // Если маг зажат со всех сторон — он просто останавливается и принимает бой
        if (smartMoveDir.sqrMagnitude < 0.01f)
        {
            _enemy.StopMovement();
            return;
        }

        // Двигаем мага плавно через AddForce физики, исключая дребезг
        _enemy.MoveInDirection(smartMoveDir, 0.85f);
    }

    private Vector2 GetSmartRetreatDirection(Vector2 origin, Vector2 dirAway)
    {
        // Проверяем ВСЕ твердые объекты (стены, ящики, вазы, колонны), исключая мобов и игрока
        int obstacleMask = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");

        RaycastHit2D hit = Physics2D.CircleCast(origin, 0.35f, dirAway, OBSTACLE_PROBE_DIST, obstacleMask);

        // Если сзади свободно — спокойно отходим прямо назад
        if (hit.collider == null || hit.collider.isTrigger)
        {
            return dirAway;
        }

        // Препятствие обнаружено (стена или ящик)
        Vector2 normal = hit.normal;
        Vector2 tangentRight = new Vector2(-normal.y, normal.x);
        Vector2 tangentLeft = -tangentRight;

        float spaceRight = ProbeSpace(origin, tangentRight, obstacleMask);
        float spaceLeft = ProbeSpace(origin, tangentLeft, obstacleMask);

        // Если оба пути заблокированы (тупик / угол) — стоим на месте
        if (spaceRight < 0.4f && spaceLeft < 0.4f)
        {
            return Vector2.zero;
        }

        Vector2 bestTangent = spaceRight >= spaceLeft ? tangentRight : tangentLeft;
        return (bestTangent * 0.85f + normal * 0.15f).normalized;
    }

    private float ProbeSpace(Vector2 origin, Vector2 dir, int mask)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 2.5f, mask);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            return hit.distance;
        }
        return 2.5f;
    }

    private IEnumerator TelegraphedShotRoutine()
    {
        _isChargingShot = true;
        _enemy.StopMovement();

        SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        float chargeTime = 0.3f;
        float elapsed = 0f;

        while (elapsed < chargeTime)
        {
            if (_enemy == null || _enemy.IsStaggered) yield break;

            elapsed += Time.deltaTime;
            if (sr != null)
            {
                sr.color = Color.Lerp(originalColor, new Color(1f, 0.9f, 0.2f, 1f), elapsed / chargeTime);
            }
            yield return null;
        }

        if (sr != null) sr.color = originalColor;

        Shoot();

        _isChargingShot = false;
    }

    private void CancelCharge()
    {
        if (_chargeRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_chargeRoutine);
            _chargeRoutine = null;
        }

        if (_enemy != null)
        {
            SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && _enemy.Data != null) sr.color = _enemy.Data.color;
        }

        _isChargingShot = false;
    }

    private void Shoot()
    {
        if (_enemy.Data.projectilePrefab == null || _enemy.Player == null) return;

        Vector2 direction = ((Vector2)_enemy.Player.position - (Vector2)_enemy.transform.position).normalized;

        GameObject proj = Object.Instantiate(
            _enemy.Data.projectilePrefab,
            _enemy.transform.position,
            Quaternion.identity
        );

        EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Init(direction, _enemy.Data.contactDamage);
        }
    }
}
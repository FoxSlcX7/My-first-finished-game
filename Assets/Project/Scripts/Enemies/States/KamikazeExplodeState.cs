using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Состояние врага-камикадзе: короткий отсчёт с пульсацией (тик бомбы),
/// затем взрыв по площади с уроном игроку/врагам/разрушаемым объектам и самоуничтожением.
/// </summary>
public class KamikazeExplodeState : IEnemyState
{
    #region Constants

    private const float EXPLOSION_RADIUS = 2.2f;
    private const float COUNTDOWN_DURATION = 1.1f;
    private const int FALLBACK_DAMAGE = 10;

    #endregion

    #region Fields

    private EnemyController _enemy;
    private Coroutine _explodeRoutine;

    #endregion

    #region IEnemyState

    public void Enter(EnemyController enemy)
    {
        _enemy = enemy;

        // Камикадзе останавливается и начинает отсчёт детонации
        _enemy?.StopMovement();
        PlayExplodeAnimation();
        _explodeRoutine = _enemy?.StartCoroutine(ExplodeCountdownRoutine());
    }

    public void Execute()
    {
        // Отсчёт идёт до конца независимо от стана — взрыв уже «предрешён».
        if (_enemy == null) return;
    }

    public void Exit()
    {
        CancelExplosion();
        _enemy?.StopMovement();
    }

    #endregion

    #region Animation

    /// <summary>
    /// Переключает аниматор на состояние взрыва (кадр 3) при старте детонации.
    /// </summary>
    private void PlayExplodeAnimation()
    {
        if (_enemy == null) return;

        Animator animator = _enemy.GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("Explode", 0, 0f);
        }
    }

    /// <summary>
    /// Возвращает анимацию бега (кадры 1-2), если отсчёт прерван и враг ещё жив.
    /// </summary>
    private void PlayRunAnimation()
    {
        if (_enemy == null) return;

        Animator animator = _enemy.GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("Run", 0, 0f);
        }
    }

    #endregion

    #region Countdown

    private IEnumerator ExplodeCountdownRoutine()
    {
        SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
        Transform spriteTransform = sr != null ? sr.transform : _enemy.transform;

        Color originalColor = sr != null ? sr.color : Color.white;
        Color explosionColor = new Color(1f, 0.15f, 0.15f, 1f); // Ярко-красный накал
        Vector3 baseScale = _enemy.Data != null ? _enemy.Data.spriteScale : Vector3.one;

        float elapsed = 0f;

        while (elapsed < COUNTDOWN_DURATION)
        {
            if (_enemy == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / COUNTDOWN_DURATION);

            // Пульсация цвета: с приближением к взрыву частота и яркость растут
            if (sr != null)
            {
                float pulse = Mathf.PingPong(elapsed * (3f + t * 9f), 1f);
                float intensity = Mathf.Clamp01(t * 0.5f + pulse * 0.6f);
                sr.color = Color.Lerp(originalColor, explosionColor, intensity);
            }

            // Пульсация масштаба: эффект «тикающей бомбы»
            float scalePulse = 1f + Mathf.Sin(elapsed * (7f + t * 12f)) * (0.05f + t * 0.12f);
            spriteTransform.localScale = baseScale * scalePulse;

            yield return null;
        }

        // Восстанавливаем визуал непосредственно перед детонацией
        if (sr != null) sr.color = originalColor;
        spriteTransform.localScale = baseScale;

        Explode();
    }

    #endregion

    #region Explosion

    private void Explode()
    {
        if (_enemy == null) return;

        Vector2 origin = _enemy.transform.position;
        int damage = _enemy.Data != null ? _enemy.Data.contactDamage : FALLBACK_DAMAGE;

        // 1. Тряска экрана
        ScreenShake.Instance?.Shake(1.0f, 0.2f);

        // 2. Дроп опыта
        XPDropper.Instance?.Drop(origin);

        // 3. Урон по площади
        // HashSet отслеживает цели, уже получившие урон, чтобы объект с несколькими
        // дочерними коллайдерами не получил урон дважды за один взрыв.
        HashSet<Component> damagedTargets = new HashSet<Component>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, EXPLOSION_RADIUS);
        foreach (Collider2D col in hits)
        {
            if (col == null) continue;

            // Пропускаем самого камикадзе (включая его дочерние коллайдеры)
            if (col.transform == _enemy.transform || col.transform.IsChildOf(_enemy.transform)) continue;

            Health health = col.GetComponentInParent<Health>();
            if (health != null)
            {
                if (damagedTargets.Add(health))
                {
                    health.TakeDamage(damage);
                }
                continue;
            }

            DestructibleObject destructible = col.GetComponentInParent<DestructibleObject>();
            if (destructible != null)
            {
                if (damagedTargets.Add(destructible))
                {
                    destructible.TakeDamage(damage);
                }
            }
        }

        // 4. Самоуничтожение
        Object.Destroy(_enemy.gameObject);
    }

    #endregion

    #region Cancellation

    private void CancelExplosion()
    {
        if (_explodeRoutine != null && _enemy != null)
        {
            _enemy.StopCoroutine(_explodeRoutine);
            _explodeRoutine = null;
        }

        // Восстанавливаем визуал, если враг ещё жив
        if (_enemy != null && _enemy.Data != null)
        {
            SpriteRenderer sr = _enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = _enemy.Data.color;
                sr.transform.localScale = _enemy.Data.spriteScale;
            }

            PlayRunAnimation();
        }
    }

    #endregion
}

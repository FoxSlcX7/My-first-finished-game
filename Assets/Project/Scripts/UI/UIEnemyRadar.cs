using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отображает индикаторы на краю экрана для врагов, находящихся вне зоны видимости камеры.
/// Оптимизирован для работы через статический реестр врагов без аллокаций.
/// </summary>
public class UIEnemyRadar : MonoBehaviour
{
    #region Inspector Settings

    [Header("Префаб стрелки")]
    [Tooltip("UI Image со спрайтом стрелочки")]
    [SerializeField] private Image arrowPrefab;

    [Tooltip("Отступ стрелок от края экрана в пикселях")]
    [SerializeField] private float edgeOffset = 45f;

    #endregion

    #region Private Fields

    private Camera _cam;
    private readonly List<Image> _arrowPool = new();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null || arrowPrefab == null) return;

        var enemies = EnemyController.ActiveEnemies;
        int activeArrowCount = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null || enemy.Health == null || enemy.Health.IsDead) continue;

            Vector3 viewportPos = _cam.WorldToViewportPoint(enemy.transform.position);

            // Проверяем, находится ли моб за пределами области видимости камеры (0..1)
            bool isOffscreen = viewportPos.x < 0f || viewportPos.x > 1f || viewportPos.y < 0f || viewportPos.y > 1f;

            if (isOffscreen)
            {
                Image arrow = GetArrow(activeArrowCount++);
                PositionArrowOnEdge(arrow, viewportPos);
            }
        }

        // Выключаем неиспользуемые стрелки из пула
        for (int i = activeArrowCount; i < _arrowPool.Count; i++)
        {
            _arrowPool[i].gameObject.SetActive(false);
        }
    }

    #endregion

    #region Arrow Management

    private Image GetArrow(int index)
    {
        if (index < _arrowPool.Count)
        {
            _arrowPool[index].gameObject.SetActive(true);
            return _arrowPool[index];
        }

        Image newArrow = Instantiate(arrowPrefab, transform);
        newArrow.gameObject.SetActive(true);
        _arrowPool.Add(newArrow);
        return newArrow;
    }

    private void PositionArrowOnEdge(Image arrow, Vector3 viewportPos)
    {
        // Вектор от центра экрана [-0.5; 0.5]
        Vector2 fromCenter = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);
        Vector2 dir = fromCenter.normalized;

        float screenW = Screen.width;
        float screenH = Screen.height;

        float clampX = Mathf.Clamp(viewportPos.x * screenW, edgeOffset, screenW - edgeOffset);
        float clampY = Mathf.Clamp(viewportPos.y * screenH, edgeOffset, screenH - edgeOffset);

        arrow.rectTransform.position = new Vector3(clampX, clampY, 0f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    #endregion
}
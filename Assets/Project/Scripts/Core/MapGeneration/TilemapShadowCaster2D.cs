using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapShadowCaster2D : MonoBehaviour
{
    [Header("Настройки теней")]
    [Tooltip("Если выключено, фасад стены освещается, а тень падает только ЗА стену")]
    [SerializeField] private bool selfShadows = false;

    private CompositeCollider2D _compositeCollider;
    private TilemapCollider2D _tilemapCollider;
    private readonly List<GameObject> _shadowCasters = new();

    private static readonly FieldInfo ShapePathField = FindShapePathField();
    private static readonly FieldInfo ShapePathHashField = FindShapePathHashField();

    private void Awake()
    {
        _compositeCollider = GetComponent<CompositeCollider2D>();
        _tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    /// <summary>
    /// Перестраивает тени по обновленным полигонам Composite Collider
    /// Можно вызвать вручную через ПКМ по компоненту в Инспекторе
    /// </summary>
    [ContextMenu("Rebuild Shadows")]
    public void RebuildShadows()
    {
        if (_compositeCollider == null) _compositeCollider = GetComponent<CompositeCollider2D>();
        if (_tilemapCollider == null) _tilemapCollider = GetComponent<TilemapCollider2D>();

        ClearShadows();

        // 1. Заставляем TilemapCollider2D принудительно обновить физику тайлов
        if (_tilemapCollider != null)
        {
            _tilemapCollider.ProcessTilemapChanges();
        }

        // 2. Заставляем CompositeCollider2D пересобрать полигоны стен
        if (_compositeCollider != null)
        {
            _compositeCollider.GenerateGeometry();
        }

        if (_compositeCollider == null || _compositeCollider.pathCount == 0)
        {
            Debug.LogWarning("[TilemapShadowCaster2D] Контуры стен не найдены (pathCount = 0). Убедись, что стены нарисованы на этом тайлмапе!");
            return;
        }

        Vector2[] pointsBuffer = new Vector2[1500];

        for (int i = 0; i < _compositeCollider.pathCount; i++)
        {
            int pointCount = _compositeCollider.GetPath(i, pointsBuffer);
            if (pointCount < 3) continue;

            GameObject shadowObj = new GameObject($"WallShadow_{i}");
            shadowObj.transform.SetParent(transform, false);
            _shadowCasters.Add(shadowObj);

            ShadowCaster2D caster = shadowObj.AddComponent<ShadowCaster2D>();
            caster.selfShadows = selfShadows;

            // Отключаем поиск силуэта спрайта, чтобы использовался векторный контур
            var prop = typeof(ShadowCaster2D).GetProperty("useRendererSilhouette");
            if (prop != null) prop.SetValue(caster, false);

            Vector3[] shapePath = new Vector3[pointCount];
            for (int j = 0; j < pointCount; j++)
            {
                shapePath[j] = pointsBuffer[j];
            }

            ShapePathField?.SetValue(caster, shapePath);
            ShapePathHashField?.SetValue(caster, Random.Range(1, int.MaxValue));

            // Перезапуск компонента для инициализации меша тени в Unity 6
            caster.enabled = false;
            caster.enabled = true;
        }

        Debug.Log($"<color=cyan>[TilemapShadowCaster2D] Успешно создано контуров теней: {_shadowCasters.Count}</color>");
    }

    public void ClearShadows()
    {
        foreach (var go in _shadowCasters)
        {
            if (go != null) Destroy(go);
        }
        _shadowCasters.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("WallShadow_"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private static FieldInfo FindShapePathField()
    {
        var field = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field;

        foreach (var f in typeof(ShadowCaster2D).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (f.FieldType == typeof(Vector3[])) return f;
        }
        return null;
    }

    private static FieldInfo FindShapePathHashField()
    {
        var field = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field;

        foreach (var f in typeof(ShadowCaster2D).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (f.Name.ToLower().Contains("hash") && f.FieldType == typeof(int)) return f;
        }
        return null;
    }
}
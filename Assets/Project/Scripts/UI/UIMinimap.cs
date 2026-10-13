using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Миникарта подземелья с динамическим туманом войны, исследованием комнат
/// и маркерами ключевых точек (POI): сундук, лестница, босс, алтарь.
/// </summary>
public class UIMinimap : MonoBehaviour
{
    #region Inspector Settings

    [Header("Ссылки на компоненты")]
    [Tooltip("Ссылка на MapGenerator на сцене")]
    [SerializeField] private MapGenerator mapGenerator;

    [Header("UI Ссылки")]
    [SerializeField] private RawImage mapDisplay;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform mapContainer;

    [Header("Настройки исследования (Туман войны)")]
    [Tooltip("Радиус раскрытия тумана вокруг игрока в тайлах")]
    [SerializeField] private int viewRadius = 8;
    [Tooltip("Клавиша раскрытия карты на весь экран (M вместо занятого Tab)")]
    [SerializeField] private Key toggleKey = Key.M;

    [Header("Цвета карты (RGBA)")]
    [SerializeField] private Color32 fogColor = new Color32(10, 12, 16, 220);
    [SerializeField] private Color32 floorColor = new Color32(190, 205, 225, 255);
    [SerializeField] private Color32 roomColor = new Color32(140, 165, 200, 255);
    [SerializeField] private Color32 wallColor = new Color32(45, 50, 65, 255);

    [Header("Цвета ключевых комнат")]
    [Tooltip("Жёлтый — сундук")]
    [SerializeField] private Color32 chestRoomColor = new Color32(245, 200, 60, 255);
    [Tooltip("Голубой — лестница/выход")]
    [SerializeField] private Color32 stairsRoomColor = new Color32(80, 170, 255, 255);
    [Tooltip("Красный — босс")]
    [SerializeField] private Color32 bossRoomColor = new Color32(235, 60, 60, 255);
    [Tooltip("Фиолетовый — алтарь")]
    [SerializeField] private Color32 altarRoomColor = new Color32(180, 90, 220, 255);

    [Header("Маркеры POI (иконки комнат)")]
    [Tooltip("Размер маркера в пикселях UI")]
    [SerializeField] private float poiMarkerSize = 14f;
    [Tooltip("Кастомный спрайт сундука. Если пусто — рисуется цветной кружок.")]
    [SerializeField] private Sprite chestMarkerSprite;
    [Tooltip("Кастомный спрайт лестницы/выхода. Если пусто — цветной кружок.")]
    [SerializeField] private Sprite stairsMarkerSprite;
    [Tooltip("Кастомный спрайт босса. Если пусто — цветной кружок.")]
    [SerializeField] private Sprite bossMarkerSprite;
    [Tooltip("Кастомный спрайт алтаря. Если пусто — цветной кружок.")]
    [SerializeField] private Sprite altarMarkerSprite;

    #endregion

    #region Private Fields

    private Transform _player;
    private Texture2D _mapTexture;
    private Color32[] _pixelBuffer;

    private int _mapWidth = 140;
    private int _mapHeight = 140;
    private int _originX = -70;
    private int _originY = -70;

    private readonly HashSet<Vector2Int> _exploredTiles = new();
    private readonly HashSet<int> _exploredRooms = new();
    private Vector2Int _lastPlayerTile = new(int.MinValue, int.MinValue);
    private Vector2 _lastPlayerWorld = new(float.MaxValue, float.MaxValue);

    private readonly List<RoomMarker> _roomMarkers = new();
    private Sprite _fallbackMarkerSprite;

    private bool _isExpanded;
    private Vector2 _compactSize = new(180, 180);
    private Vector2 _expandedSize = new(520, 520);
    private Vector2 _compactPosition;

    #endregion

    #region Nested Types

    /// <summary>Данные одного маркера POI комнаты.</summary>
    private class RoomMarker
    {
        public RectTransform Rect;
        public Image Icon;
        public int RoomIndex;
        public bool Revealed;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (mapContainer != null)
        {
            _compactSize = mapContainer.sizeDelta;
            _compactPosition = mapContainer.anchoredPosition;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автоматическое исправление: если в сцене остался старый сериализованный Tab,
        // переключаем на M, чтобы избежать конфликта с карточками прокачки
        if (toggleKey == Key.Tab)
        {
            toggleKey = Key.M;
        }
    }
#endif

    private void OnEnable()
    {
        if (mapGenerator != null)
        {
            mapGenerator.OnMapGenerated += HandleMapGenerated;
        }
    }

    private void OnDisable()
    {
        if (mapGenerator != null)
        {
            mapGenerator.OnMapGenerated -= HandleMapGenerated;
        }
    }

    private void Start()
    {
        if (mapGenerator != null && mapGenerator.FloorPositions != null && mapGenerator.FloorPositions.Count > 0)
        {
            HandleMapGenerated();
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateFogAndPlayer();
    }

    #endregion

    #region Map Initialization

    private void HandleMapGenerated()
    {
        _exploredTiles.Clear();
        _exploredRooms.Clear();
        _lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
        _lastPlayerWorld = new Vector2(float.MaxValue, float.MaxValue);

        _mapWidth = 140;
        _mapHeight = 140;
        _originX = -_mapWidth / 2;
        _originY = -_mapHeight / 2;

        if (_mapTexture == null || _mapTexture.width != _mapWidth || _mapTexture.height != _mapHeight)
        {
            _mapTexture = new Texture2D(_mapWidth, _mapHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        int totalPixels = _mapWidth * _mapHeight;
        _pixelBuffer = new Color32[totalPixels];

        for (int i = 0; i < totalPixels; i++)
        {
            _pixelBuffer[i] = fogColor;
        }

        _mapTexture.SetPixels32(_pixelBuffer);
        _mapTexture.Apply();

        if (mapDisplay != null)
        {
            mapDisplay.texture = _mapTexture;
        }

        BuildRoomMarkers();
    }

    #endregion

    #region Fog of War & Discovery

    private void UpdateFogAndPlayer()
    {
        if (_player == null)
        {
            if (GameManager.Instance?.PlayerTransform != null)
                _player = GameManager.Instance.PlayerTransform;
            else return;
        }

        Vector2 playerPos = _player.position;
        Vector2Int currentTile = new(Mathf.FloorToInt(playerPos.x), Mathf.FloorToInt(playerPos.y));

        UpdatePlayerMarker(playerPos);

        if (currentTile != _lastPlayerTile)
        {
            _lastPlayerTile = currentTile;
            bool textureChanged = RevealFogAround(currentTile);
            textureChanged |= CheckRoomEntrance(playerPos);

            if (textureChanged)
            {
                _mapTexture.SetPixels32(_pixelBuffer);
                _mapTexture.Apply();
            }
        }
    }

    private bool RevealFogAround(Vector2Int center)
    {
        if (mapGenerator == null) return false;

        bool changed = false;
        int rSqr = viewRadius * viewRadius;

        for (int x = -viewRadius; x <= viewRadius; x++)
        {
            for (int y = -viewRadius; y <= viewRadius; y++)
            {
                if (x * x + y * y > rSqr) continue;

                Vector2Int tile = new(center.x + x, center.y + y);
                if (_exploredTiles.Contains(tile)) continue;

                if (TryPaintTile(tile, floorColor))
                {
                    _exploredTiles.Add(tile);
                    changed = true;
                }
            }
        }

        return changed;
    }

    private bool CheckRoomEntrance(Vector2 playerPos)
    {
        if (mapGenerator == null || mapGenerator.Rooms == null) return false;

        bool changed = false;
        var rooms = mapGenerator.Rooms;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (_exploredRooms.Contains(i)) continue;

            if (rooms[i].Bounds.Contains(new Vector3Int(Mathf.FloorToInt(playerPos.x), Mathf.FloorToInt(playerPos.y), 0)))
            {
                _exploredRooms.Add(i);
                Color32 colorToUse = GetRoomRoleColor(i);

                foreach (var tile in rooms[i].FloorPositions)
                {
                    _exploredTiles.Add(tile);
                    TryPaintTile(tile, colorToUse);
                }

                RevealRoomMarker(i);
                changed = true;
            }
        }

        return changed;
    }

    private Color32 GetRoomRoleColor(int roomIndex)
    {
        RoomRole role = GetRoomRole(roomIndex);
        bool hasAltar = DungeonDirector.Instance != null && DungeonDirector.Instance.RoomHasAltar(roomIndex);

        switch (role)
        {
            case RoomRole.Chest: return chestRoomColor;
            case RoomRole.Stairs: return stairsRoomColor;
            case RoomRole.Boss: return bossRoomColor;
            default: return hasAltar ? altarRoomColor : roomColor;
        }
    }

    private bool TryPaintTile(Vector2Int tile, Color32 color)
    {
        int px = tile.x - _originX;
        int py = tile.y - _originY;

        if (px < 0 || px >= _mapWidth || py < 0 || py >= _mapHeight) return false;

        if (mapGenerator.FloorPositions.Contains(tile))
        {
            _pixelBuffer[py * _mapWidth + px] = color;
            return true;
        }

        if (mapGenerator.WallPositions.Contains(tile))
        {
            _pixelBuffer[py * _mapWidth + px] = wallColor;
            return true;
        }

        return false;
    }

    #endregion

    #region Room Markers (POI)

    /// <summary>
    /// Создаёт по одному скрытому маркеру для каждой комнаты и позиционирует их по центрам.
    /// Маркер показывается только после исследования комнаты и только для ключевых комнат.
    /// </summary>
    private void BuildRoomMarkers()
    {
        ClearRoomMarkers();
        if (mapGenerator == null || mapGenerator.Rooms == null || mapDisplay == null) return;

        var rooms = mapGenerator.Rooms;
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            GameObject markerObj = new GameObject($"POI_{i}", typeof(RectTransform), typeof(Image));

            // Родим под тем же контейнером, что и маркер игрока, чтобы координаты совпадали
            Transform markerParent = playerMarker != null && playerMarker.parent != null
                ? playerMarker.parent
                : mapDisplay.transform;
            markerObj.transform.SetParent(markerParent, false);

            RectTransform rt = (RectTransform)markerObj.transform;
            rt.sizeDelta = new Vector2(poiMarkerSize, poiMarkerSize);
            rt.anchoredPosition = WorldToMapAnchored(new Vector2(room.Center.x + 0.5f, room.Center.y + 0.5f));

            Image icon = markerObj.GetComponent<Image>();
            icon.raycastTarget = false;
            ApplyMarkerVisual(icon, i);

            // Туман войны: маркер скрыт, пока комната не исследована
            markerObj.SetActive(false);
            _roomMarkers.Add(new RoomMarker { Rect = rt, Icon = icon, RoomIndex = i });
        }
    }

    private void ClearRoomMarkers()
    {
        foreach (var marker in _roomMarkers)
        {
            if (marker.Rect != null)
                Destroy(marker.Rect.gameObject);
        }
        _roomMarkers.Clear();
    }

    /// <summary>Активирует маркер POI комнаты при её исследовании (только для ключевых комнат).</summary>
    private void RevealRoomMarker(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= _roomMarkers.Count) return;

        RoomMarker marker = _roomMarkers[roomIndex];
        if (marker == null || marker.Rect == null || marker.Revealed) return;

        // Показываем маркер только для ключевых комнат (POI)
        bool hasAltar = DungeonDirector.Instance != null && DungeonDirector.Instance.RoomHasAltar(roomIndex);
        RoomRole role = GetRoomRole(roomIndex);
        bool isPoi = role == RoomRole.Chest || role == RoomRole.Stairs || role == RoomRole.Boss || hasAltar;
        if (!isPoi) return;

        ApplyMarkerVisual(marker.Icon, roomIndex);
        marker.Revealed = true;
        marker.Rect.gameObject.SetActive(true);
    }

    /// <summary>Безопасно возвращает роль комнаты (с фолбэком на Combat).</summary>
    private RoomRole GetRoomRole(int roomIndex)
    {
        DungeonDirector director = DungeonDirector.Instance;
        return director != null ? director.GetRoomRole(roomIndex) : RoomRole.Combat;
    }

    /// <summary>
    /// Применяет к иконке маркера кастомный спрайт (если задан) или
    /// программно созданный цветной кружок, чтобы работало сразу из коробки.
    /// </summary>
    private void ApplyMarkerVisual(Image icon, int roomIndex)
    {
        RoomRole role = GetRoomRole(roomIndex);
        bool hasAltar = DungeonDirector.Instance != null && DungeonDirector.Instance.RoomHasAltar(roomIndex);

        // Приоритет: Босс > Лестница > Сундук > Алтарь > Обычная комната
        if (role == RoomRole.Boss) SetMarkerSprite(icon, bossMarkerSprite, bossRoomColor);
        else if (role == RoomRole.Stairs) SetMarkerSprite(icon, stairsMarkerSprite, stairsRoomColor);
        else if (role == RoomRole.Chest) SetMarkerSprite(icon, chestMarkerSprite, chestRoomColor);
        else if (hasAltar) SetMarkerSprite(icon, altarMarkerSprite, altarRoomColor);
        else SetMarkerSprite(icon, null, roomColor);
    }

    private void SetMarkerSprite(Image icon, Sprite customSprite, Color32 color)
    {
        if (customSprite != null)
        {
            icon.sprite = customSprite;
            icon.color = Color.white;
        }
        else
        {
            icon.sprite = GetFallbackMarkerSprite();
            icon.color = color;
        }
    }

    /// <summary>Создаёт (один раз) белый кружок-спрайт для фолбэка маркеров.</summary>
    private Sprite GetFallbackMarkerSprite()
    {
        if (_fallbackMarkerSprite != null) return _fallbackMarkerSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float radius = size * 0.5f;
        float center = radius - 0.5f;
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                bool inside = dx * dx + dy * dy <= radius * radius;
                tex.SetPixel(x, y, inside ? white : clear);
            }
        }

        tex.Apply();
        _fallbackMarkerSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _fallbackMarkerSprite;
    }

    #endregion

    #region Marker & Controls

    private void UpdatePlayerMarker(Vector2 worldPos)
    {
        if (playerMarker == null || mapDisplay == null) return;

        playerMarker.anchoredPosition = WorldToMapAnchored(worldPos);

        // Поворот маркера по направлению движения.
        // Основной источник — скорость Rigidbody2D; фолбэк — дельта позиции (без телепортов).
        Vector2 moveDir = Vector2.zero;
        if (_player.TryGetComponent(out Rigidbody2D rb) && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            moveDir = rb.linearVelocity;
        }
        else
        {
            Vector2 delta = worldPos - _lastPlayerWorld;
            if (delta.sqrMagnitude > 0.0001f && delta.sqrMagnitude < 50000f)
                moveDir = delta;
        }

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            playerMarker.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        _lastPlayerWorld = worldPos;
    }

    /// <summary>Переводит мировые координаты в anchoredPosition внутри RectTransform карты.</summary>
    private Vector2 WorldToMapAnchored(Vector2 worldPos)
    {
        Rect rect = mapDisplay != null ? mapDisplay.rectTransform.rect : Rect.zero;
        if (rect.width <= 0f || rect.height <= 0f)
        {
            Vector2 size = mapDisplay != null ? mapDisplay.rectTransform.sizeDelta : new Vector2(_mapWidth, _mapHeight);
            rect = new Rect(0f, 0f, size.x, size.y);
        }

        float normX = (worldPos.x - _originX) / _mapWidth;
        float normY = (worldPos.y - _originY) / _mapHeight;
        return new Vector2((normX - 0.5f) * rect.width, (normY - 0.5f) * rect.height);
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleMapSize();
        }
    }

    public void ToggleMapSize()
    {
        _isExpanded = !_isExpanded;
        if (mapContainer == null) return;

        if (_isExpanded)
        {
            mapContainer.anchorMin = new Vector2(0.5f, 0.5f);
            mapContainer.anchorMax = new Vector2(0.5f, 0.5f);
            mapContainer.pivot = new Vector2(0.5f, 0.5f);
            mapContainer.anchoredPosition = Vector2.zero;
            mapContainer.sizeDelta = _expandedSize;
        }
        else
        {
            mapContainer.anchorMin = new Vector2(1f, 1f);
            mapContainer.anchorMax = new Vector2(1f, 1f);
            mapContainer.pivot = new Vector2(1f, 1f);
            mapContainer.anchoredPosition = _compactPosition;
            mapContainer.sizeDelta = _compactSize;
        }
    }

    #endregion
}
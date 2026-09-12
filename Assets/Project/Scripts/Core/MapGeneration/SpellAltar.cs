using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Алтарь заклинаний — объект-«лотерея»: НЕ показывает, какой спелл внутри.
/// Игрок узнаёт содержимое только нажав E после зачистки комнаты.
/// До зачистки алтарь залочен: нет подсказки, E не работает.
/// </summary>
public class SpellAltar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject hintObject; // дочерняя подсказка «[E] Взять заклинание»

    [Header("Состояние блокировки")]
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.55f, 0.6f, 0.85f);
    [SerializeField] private Color unlockedColor = Color.white;

    private SpellSO _spell;
    private bool _playerInside;
    private bool _used;
    private bool _locked = true;

    /// <summary>Вызывает DungeonDirector. Содержимое НЕ раскрывает.</summary>
    public void Init(SpellSO spell)
    {
        _spell = spell;
        ApplyVisualState();
    }

    /// <summary>Вызывает RoomController в момент зачистки комнаты.</summary>
    public void Unlock()
    {
        if (!_locked) return;
        _locked = false;
        ApplyVisualState();

        // Игрок может стоять у алтаря в момент зачистки — покажем подсказку сразу
        if (_playerInside && !_used && hintObject != null)
            hintObject.SetActive(true);
    }

    private void ApplyVisualState()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = _locked ? lockedColor : unlockedColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used || _locked || !other.CompareTag("Player")) return;
        _playerInside = true;
        if (hintObject != null) hintObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        if (hintObject != null) hintObject.SetActive(false);
    }

    private void Update()
    {
        if (_used || _locked || !_playerInside) return;
        if (Time.timeScale <= 0f) return; // не открываемся поверх чужой паузы
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        _used = true;
        if (hintObject != null) hintObject.SetActive(false);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        UIAltarChoice ui = UIAltarChoice.Instance;
        if (ui != null) ui.Open(_spell);
        else Debug.LogWarning("SpellAltar: UIAltarChoice.Instance == null.");
    }
}
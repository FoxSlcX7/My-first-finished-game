using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private ComboDatabase comboDatabase;
    [SerializeField] private Transform firePoint;

    public event System.Action OnSpellCast;
    public event System.Action<bool> OnComboReadyChanged;

    private SpellSO _slotA;
    private SpellSO _slotB;
    private float _nextSlotATime;
    private float _nextSlotBTime;
    private float _nextComboTime;
    private bool _wasComboReady;

    private void Start()
    {
        _slotA = availableSpells[0];
        _slotB = availableSpells[1];

        GameEvents.OnSlotAChanged?.Raise(_slotA);
        GameEvents.OnSlotBChanged?.Raise(_slotB);

        RefreshComboState();
        _wasComboReady = IsComboReady();
    }

    private void Update()
    {
        bool isReady = IsComboReady();
        if (isReady != _wasComboReady)
        {
            _wasComboReady = isReady;
            OnComboReadyChanged?.Invoke(isReady);
        }
    }

    // ═══════════════════════════════════════
    // ЛКМ → всегда кастует заклинание слота A
    // ═══════════════════════════════════════
    public void CastSlot1()
    {
        if (_slotA == null || Time.time < _nextSlotATime) return;

        _nextSlotATime = Time.time + _slotA.cooldown * GetCooldownMult();
        CastBaseSpell(_slotA);
        OnSpellCast?.Invoke();
    }

    // ═══════════════════════════════════════
    // ПКМ → всегда кастует заклинание слота B
    // ═══════════════════════════════════════
    public void CastSlot2()
    {
        if (_slotB == null || Time.time < _nextSlotBTime) return;

        _nextSlotBTime = Time.time + _slotB.cooldown * GetCooldownMult();
        CastBaseSpell(_slotB);
        OnSpellCast?.Invoke();
    }

    // ═══════════════════════════════════════
    // Space/Q → кастует комбо, если доступно
    // ═══════════════════════════════════════
    public void CastCombo()
    {
        SpellComboSO combo = GetActiveCombo();
        if (combo == null || Time.time < _nextComboTime) return;

        _nextComboTime = Time.time + combo.cooldown * GetCooldownMult();
        SpawnComboProjectile(combo);

        GameEvents.OnComboCast?.Raise(combo);
        OnSpellCast?.Invoke();
    }

    // ═══════════════════════════════════════
    // Для UI: доступно ли комбо прямо сейчас?
    // ═══════════════════════════════════════
    public bool IsComboReady()
    {
        return GetActiveCombo() != null && Time.time >= _nextComboTime;
    }

    // ═══════════════════════════════════════
    // Внутренняя логика
    // ═══════════════════════════════════════
    private SpellComboSO GetActiveCombo()
    {
        if (comboDatabase == null || _slotA == null || _slotB == null) return null;
        return comboDatabase.FindCombo(_slotA.element, _slotB.element);
    }

    private float GetCooldownMult()
    {
        return PlayerStats.Instance != null ? PlayerStats.Instance.CooldownMultiplier : 1f;
    }

    private void RefreshComboState()
    {
        GameEvents.OnComboStateChanged?.Raise(GetActiveCombo());
    }

    private void CastBaseSpell(SpellSO spell)
    {
        if (spell.effect != null)
        {
            spell.effect.Cast(firePoint.position, firePoint.right, spell);
        }
        else
        {
            Debug.LogWarning($"SpellCaster: у заклинания {spell.name} не назначен effect!");
        }
        GameEvents.OnSpellCast?.Raise(spell);
    }

    private void SpawnComboProjectile(SpellComboSO combo)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;
        projectile.Init(firePoint.right);
        projectile.SetStats(combo.projectileSpeed, combo.lifetime, combo.damage, combo.knockbackForce);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = combo.projectileColor;
        }
    }

    /// <summary>
    /// Поворачивает только firePoint к курсору.
    /// Вызывается из PlayerController каждый кадр.
    /// </summary>
    public void SetAimDirection(Vector2 direction)
    {
        if (firePoint == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void EquipSpell(SpellSO spell)
    {
        if (_slotA == null)
        {
            _slotA = spell;
        }
        else if (_slotB == null)
        {
            _slotB = spell;
        }
        else
        {
            _slotB = _slotA;
            _slotA = spell;
        }

        GameEvents.OnSlotAChanged?.Raise(_slotA);
        GameEvents.OnSlotBChanged?.Raise(_slotB);
        RefreshComboState();
    }

    public SpellSO GetSlotA() => _slotA;
    public SpellSO GetSlotB() => _slotB;
}
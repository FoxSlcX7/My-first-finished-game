using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private ComboDatabase comboDatabase;
    [SerializeField] private Transform firePoint;

    public event System.Action OnSpellCast;
    public event System.Action<bool> OnComboReadyChanged;
    /// <summary>Заряд половин шкалы (A, B), 0..100. Для UI.</summary>
    public event System.Action<float, float> OnComboChargeChanged;

    private SpellSO _slotA;
    private SpellSO _slotB;
    private float _nextSlotATime;
    private float _nextSlotBTime;
    private float _chargeA;
    private float _chargeB;
    private bool _wasComboReady;

    public float ChargeA => _chargeA;
    public float ChargeB => _chargeB;

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
    // Касты слотов: каждый каст заряжает свою половину шкалы
    // ═══════════════════════════════════════
    public void CastSlot1()
    {
        if (_slotA == null || Time.time < _nextSlotATime) return;

        _nextSlotATime = Time.time + _slotA.cooldown * GetCooldownMult();
        CastBaseSpell(_slotA);
        AddCharge(true);
        OnSpellCast?.Invoke();
    }

    public void CastSlot2()
    {
        if (_slotB == null || Time.time < _nextSlotBTime) return;

        _nextSlotBTime = Time.time + _slotB.cooldown * GetCooldownMult();
        CastBaseSpell(_slotB);
        AddCharge(false);
        OnSpellCast?.Invoke();
    }

    // ═══════════════════════════════════════
    // Комбо: доступно ТОЛЬКО при обеих полных половинах
    // ═══════════════════════════════════════
    public void CastCombo()
    {
        if (!IsComboReady()) return;

        SpellComboSO combo = GetActiveCombo();
        SpawnComboProjectile(combo);

        _chargeA = 0f;
        _chargeB = 0f;
        RaiseCharge();

        GameEvents.OnComboCast?.Raise(combo);
        OnSpellCast?.Invoke();
    }

    public bool IsComboReady()
    {
        return GetActiveCombo() != null && _chargeA >= 100f && _chargeB >= 100f;
    }

    // ═══════════════════════════════════════
    // Слоты: установка из UI алтаря, БЕЗ автозамены.
    // Смена заклинания сбрасывает заряд (по плану).
    // ═══════════════════════════════════════
    public void SetSlotA(SpellSO spell)
    {
        if (spell == null) return;
        _slotA = spell;
        ResetCharge();
        GameEvents.OnSlotAChanged?.Raise(_slotA);
        RefreshComboState();
    }

    public void SetSlotB(SpellSO spell)
    {
        if (spell == null) return;
        _slotB = spell;
        ResetCharge();
        GameEvents.OnSlotBChanged?.Raise(_slotB);
        RefreshComboState();
    }

    public SpellSO GetSlotA() => _slotA;
    public SpellSO GetSlotB() => _slotB;

    // ═══════════════════════════════════════
    // Заряд
    // ═══════════════════════════════════════
    private void AddCharge(bool slotA)
    {
        SpellComboSO combo = GetActiveCombo();
        if (combo == null) return; // нет совместимости — заряжать нечего

        if (slotA) _chargeA = Mathf.Min(100f, _chargeA + combo.chargePerCast);
        else _chargeB = Mathf.Min(100f, _chargeB + combo.chargePerCast);
        RaiseCharge();
    }

    private void ResetCharge()
    {
        _chargeA = 0f;
        _chargeB = 0f;
        RaiseCharge();
        _wasComboReady = IsComboReady();
    }

    private void RaiseCharge() => OnComboChargeChanged?.Invoke(_chargeA, _chargeB);

    private float GetCooldownMult()
    {
        return PlayerStats.Instance != null ? PlayerStats.Instance.CooldownMultiplier : 1f;
    }

    // ═══════════════════════════════════════
    // Внутренняя логика (без изменений)
    // ═══════════════════════════════════════
    private SpellComboSO GetActiveCombo()
    {
        if (comboDatabase == null || _slotA == null || _slotB == null) return null;
        return comboDatabase.FindCombo(_slotA.element, _slotB.element);
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

    public void SetAimDirection(Vector2 direction)
    {
        if (firePoint == null) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }
}
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private SpellComboSO[] availableCombos;
    [SerializeField] private Transform firePoint;

    private SpellSO _slotA;
    private SpellSO _slotB;

    private float _slotACooldown;
    private float _slotBCooldown;
    private float _comboCooldown;

    private void Start()
    {
        _slotA = availableSpells[0];
        _slotB = availableSpells[1];

        // Первичная отправка для UI
        GameEvents.OnSlotAChanged?.Raise(_slotA);
        GameEvents.OnSlotBChanged?.Raise(_slotB);
    }

    private void Update()
    {
        // Уменьшаем таймеры каждый кадр
        _slotACooldown -= Time.deltaTime;
        _slotBCooldown -= Time.deltaTime;
        _comboCooldown -= Time.deltaTime;
    }

    public void CastSlot1()
    {
        if (_slotACooldown > 0f) return;

        SpellComboSO combo = FindCombo(_slotA, _slotB);
        if (combo != null)
        {
            if (_comboCooldown > 0f) return;
            _comboCooldown = combo.cooldown;
            CastCombo(combo);
        }
        else if (_slotA != null)
        {
            _slotACooldown = _slotA.cooldown;
            CastBaseSpell(_slotA);
        }
    }

    public void CastSlot2()
    {
        if (_slotBCooldown > 0f) return;

        SpellComboSO combo = FindCombo(_slotA, _slotB);
        if (combo != null)
        {
            if (_comboCooldown > 0f) return;
            _comboCooldown = combo.cooldown;
            CastCombo(combo);
        }
        else if (_slotB != null)
        {
            _slotBCooldown = _slotB.cooldown;
            CastBaseSpell(_slotB);
        }
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

    private void CastCombo(SpellComboSO combo)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;

        projectile.Init(firePoint.right);
        projectile.SetStats(combo.projectileSpeed, combo.lifetime, combo.damage);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = combo.projectileColor;
        }

        Debug.Log($"COMBO: {combo.name}");
    }

    private SpellComboSO FindCombo(SpellSO a, SpellSO b)
    {
        if (a == null || b == null) return null;

        foreach (var combo in availableCombos)
        {
            if (combo.Matches(a.element, b.element))
            {
                return combo;
            }
        }

        return null;
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

        // Сообщаем UI об изменении слотов
        GameEvents.OnSlotAChanged?.Raise(_slotA);
        GameEvents.OnSlotBChanged?.Raise(_slotB);
    }

    public SpellSO GetSlotA() => _slotA;
    public SpellSO GetSlotB() => _slotB;
}
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] private SpellSO[] availableSpells;
    [SerializeField] private SpellComboSO[] availableCombos;
    [SerializeField] private Transform firePoint;

    private SpellSO _slotA;
    private SpellSO _slotB;

    private void Start()
    {
        _slotA = availableSpells[0];
        _slotB = availableSpells[1];
    }

    public void CastSlot1()
    {
        Cast(_slotA);
    }

    public void CastSlot2()
    {
        Cast(_slotB);
    }

    private void Cast(SpellSO spell)
    {
        if (spell == null) return;

        SpellComboSO combo = FindCombo(_slotA, _slotB);

        if (combo != null)
        {
            CastCombo(combo);
        }
        else
        {
            CastBaseSpell(spell);
        }
    }

    private void CastBaseSpell(SpellSO spell)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;

        projectile.Init(firePoint.right);
        projectile.SetDamage(spell.damage);

        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = spell.projectileColor;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer on projectile prefab!");
        }
    }

    private void CastCombo(SpellComboSO combo)
    {
        Projectile projectile = PoolManager.Instance.GetProjectile();
        projectile.transform.position = firePoint.position;
        projectile.transform.rotation = firePoint.rotation;

        projectile.Init(firePoint.right);
        projectile.SetDamage(combo.damage);

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
            // Оба слота заняты: сдвигаем (A → B, новое → A)
            _slotB = _slotA;
            _slotA = spell;
        }
    }

    public SpellSO GetSlotA() => _slotA;
    public SpellSO GetSlotB() => _slotB;
}
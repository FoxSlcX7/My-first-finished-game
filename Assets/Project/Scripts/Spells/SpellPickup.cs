using UnityEngine;

public class SpellPickup : MonoBehaviour
{
    [SerializeField] private SpellSO spell;

    public void SetSpell(SpellSO newSpell)
    {
        spell = newSpell;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && spell != null)
        {
            sr.sprite = spell.icon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SpellCaster caster = other.GetComponent<SpellCaster>();
        if (caster == null) return;

        caster.EquipSpell(spell);
        Destroy(gameObject);
    }
}
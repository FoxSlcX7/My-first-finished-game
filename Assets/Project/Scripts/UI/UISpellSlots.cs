using UnityEngine;
using UnityEngine.UI;

public class UISpellSlots : MonoBehaviour
{
    [SerializeField] private SpellCaster spellCaster;
    [SerializeField] private Image slotAIcon;
    [SerializeField] private Image slotBIcon;

    private void Update()
    {
        UpdateSlot(spellCaster.GetSlotA(), slotAIcon);
        UpdateSlot(spellCaster.GetSlotB(), slotBIcon);
    }

    private void UpdateSlot(SpellSO spell, Image icon)
    {
        if (spell == null)
        {
            icon.sprite = null;
            icon.color = Color.gray;
        }
        else
        {
            icon.sprite = spell.icon;
        }
    }
}
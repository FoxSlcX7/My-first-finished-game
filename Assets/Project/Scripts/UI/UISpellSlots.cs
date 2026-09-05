using UnityEngine;
using UnityEngine.UI;

public class UISpellSlots : MonoBehaviour
{
    [SerializeField] private Image slotAIcon;
    [SerializeField] private Image slotBIcon;

    private void OnEnable()
    {
        GameEvents.OnSlotAChanged?.AddListener(UpdateSlotA);
        GameEvents.OnSlotBChanged?.AddListener(UpdateSlotB);
    }

    private void OnDisable()
    {
        GameEvents.OnSlotAChanged?.RemoveListener(UpdateSlotA);
        GameEvents.OnSlotBChanged?.RemoveListener(UpdateSlotB);
    }

    private void UpdateSlotA(SpellSO spell) => UpdateSlot(spell, slotAIcon);
    private void UpdateSlotB(SpellSO spell) => UpdateSlot(spell, slotBIcon);

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
            icon.color = Color.white;
        }
    }
}
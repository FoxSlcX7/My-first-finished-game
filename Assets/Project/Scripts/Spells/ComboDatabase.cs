using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Combo Database", menuName = "Arcane Arsenal/Combo Database")]
public class ComboDatabase : ScriptableObject
{
    [SerializeField] private List<SpellComboSO> combos = new List<SpellComboSO>();

    public SpellComboSO FindCombo(ElementType a, ElementType b)
    {
        foreach (SpellComboSO combo in combos)
        {
            if (combo.Matches(a, b))
            {
                return combo;
            }
        }
        return null;
    }
}
using UnityEngine;

public interface ISpellEffect
{
    void Cast(Vector2 origin, Vector2 direction, SpellSO data);
}
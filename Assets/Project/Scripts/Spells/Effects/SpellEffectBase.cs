using UnityEngine;

// Абстрактная база: Unity умеет сериализовать такие поля в инспекторе
public abstract class SpellEffectBase : ScriptableObject, ISpellEffect
{
    public abstract void Cast(Vector2 origin, Vector2 direction, SpellSO data);
}
using UnityEngine;

[CreateAssetMenu(fileName = "New AOE Effect", menuName = "Arcane Arsenal/Spell Effect/AOE")]
public class AOESpellEffect : SpellEffectBase
{
    [SerializeField] private GameObject aoePrefab;
    [SerializeField] private float radius = 2f;

    public override void Cast(Vector2 origin, Vector2 direction, SpellSO data)
    {
        if (aoePrefab == null)
        {
            Debug.LogError("AOESpellEffect: не назначен aoePrefab!");
            return;
        }

        GameObject zone = Object.Instantiate(aoePrefab, origin, Quaternion.identity);
        AOEZone aoeZone = zone.GetComponent<AOEZone>();
        if (aoeZone != null)
        {
            aoeZone.Init(data.damage, radius, data.lifetime, data.projectileColor, data.knockbackForce);
        }
    }
}
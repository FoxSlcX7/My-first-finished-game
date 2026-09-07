using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Rarity Config", fileName = "RarityConfig")]
public class RarityConfigSO : ScriptableObject
{
    [Header("Базовые веса (важна пропорция, не сумма)")]
    public float weightCommon = 60f;
    public float weightRare = 25f;
    public float weightEpic = 10f;
    public float weightLegendary = 5f;

    [Header("Скалинг за каждый этаж после первого")]
    public float rarePerFloor = 2f;
    public float epicPerFloor = 1f;
    public float legendaryPerFloor = 0.5f;

    public Rarity Roll(int floor)
    {
        int f = Mathf.Max(0, floor - 1);
        float wR = weightRare + rarePerFloor * f;
        float wE = weightEpic + epicPerFloor * f;
        float wL = weightLegendary + legendaryPerFloor * f;

        float roll = Random.Range(0f, weightCommon + wR + wE + wL);
        if ((roll -= weightCommon) < 0f) return Rarity.Common;
        if ((roll -= wR) < 0f) return Rarity.Rare;
        if ((roll -= wE) < 0f) return Rarity.Epic;
        return Rarity.Legendary;
    }
}
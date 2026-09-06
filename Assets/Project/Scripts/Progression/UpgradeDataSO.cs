using UnityEngine;

[CreateAssetMenu(menuName = "Progression/Upgrade Data", fileName = "NewUpgrade")]
public class UpgradeDataSO : ScriptableObject
{
    public string upgradeName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public UpgradeEffectBase effect;
    // Редкость добавим в дни 45–46
}
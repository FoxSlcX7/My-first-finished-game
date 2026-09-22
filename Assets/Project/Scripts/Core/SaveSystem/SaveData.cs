using System.Collections.Generic;

[System.Serializable]
public class MetaUpgradeSaveEntry
{
    public string upgradeId;
    public int level;

    public MetaUpgradeSaveEntry(string id, int lvl)
    {
        upgradeId = id;
        level = lvl;
    }
}

[System.Serializable]
public class SaveData
{
    // Мета-прогрессия (между забегами)
    public int runeStones;                                              // Мета-валюта
    public List<string> unlockedSpells = new List<string>();           // Открытые заклинания
    public List<MetaUpgradeSaveEntry> metaUpgrades = new List<MetaUpgradeSaveEntry>(); // Купленные улучшения

    // Статистика
    public int totalKills;      // Всего убийств
    public int totalDeaths;     // Всего смертей
    public int runsPlayed;      // Всего забегов
    public float totalPlayTime; // Общее время игры (секунды)

    // Хелперы для работы с мета-апгрейдами
    public int GetUpgradeLevel(string upgradeId)
    {
        var entry = metaUpgrades.Find(u => u.upgradeId == upgradeId);
        return entry != null ? entry.level : 0;
    }

    public void SetUpgradeLevel(string upgradeId, int level)
    {
        var entry = metaUpgrades.Find(u => u.upgradeId == upgradeId);
        if (entry != null)
        {
            entry.level = level;
        }
        else
        {
            metaUpgrades.Add(new MetaUpgradeSaveEntry(upgradeId, level));
        }
    }
}
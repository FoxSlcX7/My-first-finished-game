using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Мета-прогрессия (между забегами)
    public int runeStones;                                // мета-валюта (Рунные Камни)
    public List<string> unlockedSpells = new List<string>(); // открытые заклинания

    // Статистика
    public int totalKills;      // всего убийств
    public int totalDeaths;     // всего смертей
    public int runsPlayed;      // всего забегов
    public float totalPlayTime; // общее время игры (секунды)
}
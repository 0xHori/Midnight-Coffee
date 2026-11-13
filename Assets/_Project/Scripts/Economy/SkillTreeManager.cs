using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public ClickerManager clickerManager;

    [Header("Все навыки в игре (перетащи сюда SO)")]
    public List<SkillData> allSkills = new List<SkillData>();

    // Словарь для быстрого доступа в рантайме: skillId -> текущий уровень
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    public int GetSkillLevel(string skillId)
    {
        return skillLevels.TryGetValue(skillId, out int lvl) ? lvl : 0;
    }

    public bool IsSkillUnlocked(SkillData skill)
    {
        if (skill.requiredSkill == null) return true;
        return GetSkillLevel(skill.requiredSkill.skillId) >= skill.requiredSkillLevel;
    }

    public bool CanUpgradeSkill(SkillData skill)
    {
        if (!IsSkillUnlocked(skill)) return false;
        int currentLvl = GetSkillLevel(skill.skillId);
        if (currentLvl >= skill.maxLevel) return false;
        return clickerManager.totalCrystals >= skill.GetCost(currentLvl);
    }

    public void UpgradeSkill(SkillData skill)
    {
        if (!CanUpgradeSkill(skill)) return;

        int currentLvl = GetSkillLevel(skill.skillId);
        double cost = skill.GetCost(currentLvl);

        clickerManager.totalCrystals -= cost;
        skillLevels[skill.skillId] = currentLvl + 1;

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
        GameEvents.UpdateSkillUI();
    }

    // Универсальный подсчет бонуса от всех прокачанных навыков по типу стата
    public double GetTotalStatBonus(StatType type)
    {
        double total = 0;
        foreach (var skill in allSkills)
        {
            if (skill != null && skill.statType == type)
            {
                int lvl = GetSkillLevel(skill.skillId);
                if (lvl > 0) total += skill.GetBonus(lvl);
            }
        }
        return total;
    }

    // Удобные геттеры для совместимости
    public double ClickMultiplier => 1.0 + GetTotalStatBonus(StatType.ClickMultiplier);
    public double AutoMultiplier => 1.0 + GetTotalStatBonus(StatType.AutoMultiplier);
    public double CritChance => 0.10 + GetTotalStatBonus(StatType.CritChance);
    public double CritDamageBonus => GetTotalStatBonus(StatType.CritDamage);
    public double PrestigeBonusMultiplier => 1.0 + GetTotalStatBonus(StatType.PrestigeBonus);

    // Методы для SaveManager
    public List<IdLevelPair> ExportSaveData()
    {
        List<IdLevelPair> list = new List<IdLevelPair>();
        foreach (var kvp in skillLevels)
        {
            list.Add(new IdLevelPair(kvp.Key, kvp.Value));
        }
        return list;
    }

    public void ImportSaveData(List<IdLevelPair> savedList)
    {
        skillLevels.Clear();
        if (savedList == null) return;
        foreach (var pair in savedList)
        {
            skillLevels[pair.id] = pair.level;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public ClickerManager clickerManager;

    [Header("Все навыки в игре (перетащи сюда SO)")]
    public List<SkillData> allSkills = new List<SkillData>();

    // Словарь для быстрого доступа в рантайме: skillId -> текущий уровень
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    private readonly HashSet<string> _processedIds = new HashSet<string>();

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
        if (skill == null || !IsSkillUnlocked(skill)) return;
        int currentLvl = GetSkillLevel(skill.skillId);
        if (currentLvl >= skill.maxLevel) return;

        double cost = skill.GetCost(currentLvl);
        if (clickerManager.totalCrystals < cost)
        {
            GameEvents.NotEnoughCurrency();
            return;
        }

        clickerManager.totalCrystals -= cost;
        skillLevels[skill.skillId] = currentLvl + 1;

        GameEvents.SaveGame();
        GameEvents.UpdateUI();
        GameEvents.UpdateSkillUI();
    }

    void OnValidate()
    {
        if (allSkills == null) return;
        HashSet<string> seen = new HashSet<string>();
        for (int i = 0; i < allSkills.Count; i++)
        {
            var skill = allSkills[i];
            if (skill == null || string.IsNullOrEmpty(skill.skillId)) continue;
            if (!seen.Add(skill.skillId))
                Debug.LogWarning($"[SkillTreeManager] Дублирующийся skillId в списке: {skill.skillId}", this);
        }
    }

    public double GetTotalStatBonus(StatType type)
    {
        if (allSkills == null) return 0;

        double total = 0;
        _processedIds.Clear();

        foreach (var skill in allSkills)
        {
            if (skill == null || string.IsNullOrEmpty(skill.skillId)) continue;
            if (!_processedIds.Add(skill.skillId)) continue;

            if (skill.statType == type)
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
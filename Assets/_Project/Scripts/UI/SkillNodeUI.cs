using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNodeUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;
    public Image iconImage;

    private SkillData skillData;
    private SkillTreeManager skillTreeManager;
    private ClickerManager clickerManager;

    public void Setup(SkillData skill, SkillTreeManager treeManager, ClickerManager clicker)
    {
        skillData = skill;
        skillTreeManager = treeManager;
        clickerManager = clicker;

        if (iconImage != null && skill.icon != null)
        {
            iconImage.sprite = skill.icon;
        }

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        Refresh();
    }

    public void Refresh()
    {
        if (skillData == null || skillTreeManager == null) return;

        int currentLvl = skillTreeManager.GetSkillLevel(skillData.skillId);
        bool isUnlocked = skillTreeManager.IsSkillUnlocked(skillData);
        bool isMaxed = currentLvl >= skillData.maxLevel;

        titleText.text = $"{skillData.title} (Ур. {currentLvl}/{skillData.maxLevel})";

        if (!isUnlocked)
        {
            string reqName = skillData.requiredSkill != null ? skillData.requiredSkill.title : "???";
            descriptionText.text = $"<color=#FF6B6B>Требуется: {reqName} (Ур. {skillData.requiredSkillLevel})</color>";
            costText.text = "ЗАКРЫТО";
            upgradeButton.interactable = false;
        }
        else if (isMaxed)
        {
            descriptionText.text = skillData.description;
            costText.text = "МАКС.";
            upgradeButton.interactable = false;
        }
        else
        {
            double cost = skillData.GetCost(currentLvl);
            descriptionText.text = skillData.description;
            costText.text = $"Улучшить: {cost.ToCompactString()} кр.";
            upgradeButton.interactable = (clickerManager.totalCrystals >= cost);
        }
    }

    private void OnUpgradeClicked()
    {
        skillTreeManager.UpgradeSkill(skillData);
    }
}
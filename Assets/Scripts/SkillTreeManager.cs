using UnityEngine;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    public UIManager uiManager;

    [Header("Навыки (Уровни 0 или 1)")]
    public int clickBoostLvl = 0;
    public int autoBoostLvl = 0;
    public int critBoostLvl = 0;

    [Header("Цены (в кристаллах)")]
    public int clickBoostCost = 5;
    public int autoBoostCost = 10;
    public int critBoostCost = 15;

    // Геттеры для множителей (если уровень куплен, возвращаем 1.5, иначе 1.0)
    public double ClickMultiplier => clickBoostLvl > 0 ? 1.5 : 1.0;
    public double AutoMultiplier => autoBoostLvl > 0 ? 1.5 : 1.0;
    public double CritChance => critBoostLvl > 0 ? 0.25 : 0.10; // 25% вместо 10%

    public void BuyClickBoost()
    {
        if (clickerManager.totalCrystals >= clickBoostCost && clickBoostLvl == 0)
        {
            clickerManager.totalCrystals -= clickBoostCost;
            clickBoostLvl = 1;
            clickerManager.saveManager.SaveGame();
            uiManager.UpdateUI();
            uiManager.UpdateSkillUI();
        }
    }

    public void BuyAutoBoost()
    {
        if (clickerManager.totalCrystals >= autoBoostCost && autoBoostLvl == 0)
        {
            clickerManager.totalCrystals -= autoBoostCost;
            autoBoostLvl = 1;
            clickerManager.saveManager.SaveGame();
            uiManager.UpdateUI();
            uiManager.UpdateSkillUI();
        }
    }

    public void BuyCritBoost()
    {
        if (clickerManager.totalCrystals >= critBoostCost && critBoostLvl == 0)
        {
            clickerManager.totalCrystals -= critBoostCost;
            critBoostLvl = 1;
            clickerManager.saveManager.SaveGame();
            uiManager.UpdateUI();
            uiManager.UpdateSkillUI();
        }
    }
}
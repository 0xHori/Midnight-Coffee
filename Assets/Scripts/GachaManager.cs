using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public ClickerManager clickerManager;
    
    [Header("Пул персонажей")]
    public List<CharacterData> allCharacters; 
    public List<string> ownedCharacterNames = new List<string>();

    [Header("Настройки")]
    public int rollCost = 10; // 10 кристаллов

    public void RollGacha()
    {
        if (clickerManager.totalCrystals < rollCost)
        {
            GameEvents.ShowGachaResult("Не хватает кристаллов!", null, Color.red);
            return;
        }

        // Тратим кристаллы
        clickerManager.totalCrystals -= rollCost;

        // 1. Ролл редкости (70% - 1, 25% - 2, 5% - 3)
        float roll = Random.Range(0f, 100f);
        int targetRarity = 1;
        if (roll > 70f && roll <= 95f) targetRarity = 2;
        else if (roll > 95f) targetRarity = 3;

        // 2. Фильтруем пул по редкости
        List<CharacterData> pool = allCharacters.FindAll(c => c.rarity == targetRarity);
        if (pool.Count == 0) pool = allCharacters; // Защита от краша

        // 3. Выбираем случайного
        CharacterData result = pool[Random.Range(0, pool.Count)];
        
        // 4. Сохраняем в коллекцию (если такого еще нет)
        if (!ownedCharacterNames.Contains(result.characterName))
        {
            ownedCharacterNames.Add(result.characterName);
        }

        // 5. Определяем цвет текста для UI
        Color textColor = Color.white;
        if (targetRarity == 2) textColor = new Color(0.2f, 0.6f, 1f); // Синий
        if (targetRarity == 3) textColor = new Color(1f, 0.8f, 0f);   // Золотой

        // 6. Триггерим события
        GameEvents.ShowGachaResult(result.characterName, result.portrait, textColor);
        GameEvents.UpdateUI();
        GameEvents.SaveGame();
    }
}
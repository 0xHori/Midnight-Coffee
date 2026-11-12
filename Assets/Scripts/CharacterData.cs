using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Gacha/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait; // Картинка (9:16)
    public int rarity; // 1 - Common, 2 - Rare, 3 - Legendary
}
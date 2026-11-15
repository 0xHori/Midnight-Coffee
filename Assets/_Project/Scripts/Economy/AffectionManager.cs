using System.Collections.Generic;
using UnityEngine;

public class AffectionManager : MonoBehaviour
{
    public GachaManager gachaManager;

    [Header("Настройки начисления")]
    [Tooltip("Опыт близости за обычный тап")]
    public int xpPerTap = 1;
    [Tooltip("Опыт близости за критический тап")]
    public int xpPerCritTap = 3;
    [Tooltip("Шанс (0..1), что персонаж скажет реплику при тапе")]
    [Range(0f, 1f)] public float speakChanceOnTap = 0.08f;
    [Tooltip("Минимальный интервал между случайными репликами (сек)")]
    public float speakCooldown = 6f;

    private struct AffectionState
    {
        public int level;
        public int xp;
    }

    private readonly Dictionary<string, AffectionState> _states = new Dictionary<string, AffectionState>();
    private float _lastSpeakTime = -99f;

    void OnEnable()
    {
        GameEvents.OnPlayerTapped += HandlePlayerTapped;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerTapped -= HandlePlayerTapped;
    }

    public int GetAffectionLevel(string charId)
    {
        if (string.IsNullOrEmpty(charId)) return 1;
        return _states.TryGetValue(charId, out var st) ? Mathf.Max(1, st.level) : 1;
    }

    public int GetAffectionXp(string charId)
    {
        if (string.IsNullOrEmpty(charId)) return 0;
        return _states.TryGetValue(charId, out var st) ? st.xp : 0;
    }

    public float GetNormalizedProgress(CharacterData character)
    {
        if (character == null) return 0f;
        int lvl = GetAffectionLevel(character.characterId);
        if (lvl >= character.maxAffectionLevel) return 1f;

        int required = character.GetRequiredAffectionXp(lvl);
        if (required <= 0) return 1f;

        return Mathf.Clamp01((float)GetAffectionXp(character.characterId) / required);
    }

    private void HandlePlayerTapped(double earnedPoints, bool isCrit, bool isFever)
    {
        if (gachaManager == null) return;
        CharacterData equipped = gachaManager.GetEquippedCharacter();
        if (equipped == null) return;

        int gain = isCrit ? xpPerCritTap : xpPerTap;
        if (isFever) gain *= 2;

        AddAffectionXp(equipped, gain);

        // Случайная реплика при тапе
        if (Time.time - _lastSpeakTime >= speakCooldown && Random.value <= speakChanceOnTap)
        {
            int lvl = GetAffectionLevel(equipped.characterId);
            string quote = equipped.GetRandomUnlockedQuote(lvl);
            if (!string.IsNullOrEmpty(quote))
            {
                _lastSpeakTime = Time.time;
                GameEvents.CharacterSpeak(quote);
            }
        }
    }

    public void AddAffectionXp(CharacterData character, int amount)
    {
        if (character == null || amount <= 0) return;

        string id = character.characterId;
        AffectionState state = _states.TryGetValue(id, out var existing)
            ? existing
            : new AffectionState { level = 1, xp = 0 };

        if (state.level >= character.maxAffectionLevel) return;

        state.xp += amount;
        bool leveledUp = false;

        while (state.level < character.maxAffectionLevel)
        {
            int req = character.GetRequiredAffectionXp(state.level);
            if (req <= 0 || state.xp < req) break;

            state.xp -= req;
            state.level++;
            leveledUp = true;
        }

        if (state.level >= character.maxAffectionLevel)
            state.xp = 0;

        _states[id] = state;

        if (leveledUp)
        {
            GameEvents.AffectionLevelUp(character, state.level);

            string newQuote = character.GetQuoteForLevel(state.level);
            if (!string.IsNullOrEmpty(newQuote))
            {
                _lastSpeakTime = Time.time;
                GameEvents.CharacterSpeak(newQuote);
            }

            GameEvents.UpdateUI();
            GameEvents.SaveGame();
        }
    }

    public List<IdProgressPair> ExportSaveData()
    {
        List<IdProgressPair> list = new List<IdProgressPair>();
        foreach (var kvp in _states)
        {
            list.Add(new IdProgressPair(kvp.Key, kvp.Value.level, kvp.Value.xp));
        }
        return list;
    }

    public void ImportSaveData(List<IdProgressPair> savedList)
    {
        _states.Clear();
        if (savedList == null) return;

        foreach (var item in savedList)
        {
            if (!string.IsNullOrEmpty(item.id))
            {
                _states[item.id] = new AffectionState
                {
                    level = Mathf.Max(1, item.level),
                    xp = Mathf.Max(0, item.xp)
                };
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlaytestTelemetryRecorder : MonoBehaviour
{
    [Header("Ссылки на менеджеры")]
    public ClickerManager clickerManager;
    public ShopManager shopManager;
    public SkillTreeManager skillTreeManager;
    public GachaManager gachaManager;
    public AffectionManager affectionManager;

    [Header("Настройки записи")]
    [Tooltip("Если включено — при старте Play Mode автоматически удалит старый сейв и начнёт чистый прогон")]
    public bool resetSaveOnStart = true;

    [Tooltip("Автоматически записывать срез состояния каждые N секунд")]
    public float snapshotIntervalSeconds = 15f;

    public bool IsRecording { get; private set; }
    public float ElapsedSeconds { get; private set; }

    private string _sessionStartUtc;
    private string _reportFilePath;
    private float _nextSnapshotTime;
    private int _prestigeCounter;

    private readonly List<string> _timelineEvents = new List<string>();
    private readonly List<string> _prestigeLogs = new List<string>();
    private readonly List<string> _minuteSnapshots = new List<string>();

    void Awake()
    {
#if UNITY_EDITOR
        if (resetSaveOnStart)
        {
            string savePath = Path.Combine(Application.persistentDataPath, "MyClickerSave.json");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[Playtest] Старый сейв удалён для чистого прогона.");
            }
        }
#endif
    }

    void Start()
    {
        StartSession();
    }

    void OnEnable()
    {
        GameEvents.OnPrestigeCompleted += HandlePrestigeCompleted;
        GameEvents.OnAffectionLevelUp += HandleAffectionLevelUp;
        GameEvents.OnCharacterEquipped += HandleCharacterEquipped;
        GameEvents.OnFeverTriggered += HandleFeverTriggered;
    }

    void OnDisable()
    {
        GameEvents.OnPrestigeCompleted -= HandlePrestigeCompleted;
        GameEvents.OnAffectionLevelUp -= HandleAffectionLevelUp;
        GameEvents.OnCharacterEquipped -= HandleCharacterEquipped;
        GameEvents.OnFeverTriggered -= HandleFeverTriggered;
    }

    public void StartSession()
    {
        IsRecording = true;
        ElapsedSeconds = 0f;
        _prestigeCounter = 0;
        _nextSnapshotTime = snapshotIntervalSeconds;
        _sessionStartUtc = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.persistentDataPath;
        _reportFilePath = Path.Combine(projectRoot, $"PlaytestReport_{_sessionStartUtc}.md");

        _timelineEvents.Clear();
        _prestigeLogs.Clear();
        _minuteSnapshots.Clear();

        LogEvent("Старт чистой сессии плейтеста");
        RecordSnapshot("00:00 (Старт)");
        Debug.Log("[Playtest] Запись началась! Нажми F9 (или выйди из Play Mode) для сохранения отчёта.");
    }

    void Update()
    {
        if (!IsRecording) return;

        ElapsedSeconds += Time.unscaledDeltaTime;

        if (ElapsedSeconds >= _nextSnapshotTime)
        {
            RecordSnapshot($"{FormatTime(ElapsedSeconds)} (Авто-срез)");
            _nextSnapshotTime += snapshotIntervalSeconds;
        }

        if (IsStopKeyPressed())
        {
            StopAndSaveReport();
        }
    }

    private bool IsStopKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F9);
#endif
    }

    private void HandlePrestigeCompleted(double crystalsEarned)
    {
        if (!IsRecording || clickerManager == null) return;

        _prestigeCounter++;
        string timeStr = FormatTime(ElapsedSeconds);

        var sb = new StringBuilder();
        sb.AppendLine($"### Престиж #{_prestigeCounter} — время `{timeStr}`");
        sb.AppendLine($"- **Получено кристаллов:** `+{crystalsEarned:F0}` (Всего в наличии: `{clickerManager.totalCrystals:F0}`, За всё время: `{clickerManager.lifetimeCrystals:F0}`)");
        sb.AppendLine($"- **Новый порог престижа (после умножения):** `{clickerManager.prestigeThreshold:F0}`");
        sb.AppendLine($"- **Кликов / Критов к моменту престижа:** `{clickerManager.totalClicks}` / `{clickerManager.totalCritClicks}`");
        sb.AppendLine($"- **Заработано очков за всё время:** `{clickerManager.lifetimeScore:F0}`");
        sb.AppendLine(BuildDetailedStateBlock());

        _prestigeLogs.Add(sb.ToString());
        LogEvent($"ПРЕСТИЖ #{_prestigeCounter}: получено +{crystalsEarned:F0} кристаллов (всего lifetime: {clickerManager.lifetimeCrystals:F0})");
    }

    private void HandleAffectionLevelUp(CharacterData character, int newLevel)
    {
        if (!IsRecording || character == null) return;
        LogEvent($"Близость UP: {character.characterName} -> Ур. {newLevel}");
    }

    private void HandleCharacterEquipped(CharacterData character)
    {
        if (!IsRecording || character == null) return;
        LogEvent($"Выбран активный персонаж: {character.characterName}");
    }

    private void HandleFeverTriggered()
    {
        if (!IsRecording) return;
        LogEvent("Активирован Fever Mode");
    }

    private void LogEvent(string message)
    {
        _timelineEvents.Add($"- `[{FormatTime(ElapsedSeconds)}]` {message}");
    }

    private void RecordSnapshot(string label)
    {
        if (clickerManager == null) return;

        double clickPower = clickerManager.GetTotalClickPower();
        double autoIncome = clickerManager.GetTotalAutoIncomePerSecond();
        double ratio = clickPower > 0 ? autoIncome / clickPower : 0;

        _minuteSnapshots.Add(
            $"| {label} | {clickerManager.score:F0} | {clickerManager.lifetimeScore:F0} | {clickPower:F1} | {autoIncome:F1}/с | {ratio:F2} | {clickerManager.totalCrystals:F0} / {clickerManager.lifetimeCrystals:F0} | {clickerManager.totalClicks} |"
        );
    }

    private string BuildDetailedStateBlock()
    {
        var sb = new StringBuilder();

        if (clickerManager != null)
        {
            sb.AppendLine($"- **Текущая сила клика:** `{clickerManager.GetTotalClickPower():F2}` (Крит шанс: `{clickerManager.GetTotalCritChance() * 100:F1}%`, Крит множ: `x{clickerManager.GetTotalCritMultiplier():F1}`)");
            sb.AppendLine($"- **Текущий автодоход:** `{clickerManager.GetTotalAutoIncomePerSecond():F2}/сек`");
            sb.AppendLine($"- **Множитель от кристаллов:** `x{clickerManager.PrestigeMultiplier:F2}`");
        }

        if (shopManager != null && shopManager.allUpgrades != null)
        {
            sb.Append("- **Магазин:** ");
            List<string> items = new List<string>();
            foreach (var upg in shopManager.allUpgrades)
            {
                if (upg == null) continue;
                int lvl = shopManager.GetUpgradeLevel(upg.upgradeId);
                items.Add($"{upg.upgradeName}: Ур.{lvl}");
            }
            sb.AppendLine(items.Count > 0 ? string.Join(", ", items) : "пусто");
        }

        if (skillTreeManager != null && skillTreeManager.allSkills != null)
        {
            sb.Append("- **Навыки:** ");
            List<string> skills = new List<string>();
            foreach (var sk in skillTreeManager.allSkills)
            {
                if (sk == null) continue;
                int lvl = skillTreeManager.GetSkillLevel(sk.skillId);
                if (lvl > 0) skills.Add($"{sk.title}: Ур.{lvl}/{sk.maxLevel}");
            }
            sb.AppendLine(skills.Count > 0 ? string.Join(", ", skills) : "не прокачаны");
        }

        if (gachaManager != null && gachaManager.allCharacters != null)
        {
            sb.AppendLine($"- **Гача (круток: {gachaManager.totalGachaRolls}, активна: {gachaManager.equippedCharacterId}):**");
            foreach (var ch in gachaManager.allCharacters)
            {
                if (ch == null) continue;
                int lvl = gachaManager.GetCharacterLevel(ch.characterId);
                int affLvl = affectionManager != null ? affectionManager.GetAffectionLevel(ch.characterId) : 1;
                int affXp = affectionManager != null ? affectionManager.GetAffectionXp(ch.characterId) : 0;
                sb.AppendLine($"  - `{ch.characterName}` (редкость {ch.rarity}★): Дубликаты Ур.{lvl}, Близость Ур.{affLvl} (XP: {affXp})");
            }
        }

        return sb.ToString();
    }

    [ContextMenu("Stop and Save Playtest Report")]
    public void StopAndSaveReport()
    {
        if (!IsRecording) return;

        IsRecording = false;
        RecordSnapshot($"{FormatTime(ElapsedSeconds)} (Финал)");

        var report = new StringBuilder();
        report.AppendLine($"# Отчёт плейтеста баланса ({_sessionStartUtc})");
        report.AppendLine($"- **Общая длительность сессии:** `{FormatTime(ElapsedSeconds)}` (`{ElapsedSeconds:F1}` сек)");
        report.AppendLine($"- **Сделано престижей:** `{_prestigeCounter}`");
        if (clickerManager != null)
        {
            report.AppendLine($"- **Итоговый баланс очков:** `{clickerManager.score:F0}` (За всё время: `{clickerManager.lifetimeScore:F0}`)");
            report.AppendLine($"- **Итоговые кристаллы:** на руках `{clickerManager.totalCrystals:F0}` / заработано всего `{clickerManager.lifetimeCrystals:F0}`");
            report.AppendLine($"- **Всего кликов:** `{clickerManager.totalClicks}` (из них критов: `{clickerManager.totalCritClicks}`)");
        }
        report.AppendLine();

        report.AppendLine("## 1. Финальное состояние аккаунта");
        report.AppendLine(BuildDetailedStateBlock());
        report.AppendLine();

        report.AppendLine("## 2. Детальные срезы по каждому Престижу");
        if (_prestigeLogs.Count == 0)
            report.AppendLine("_За эту сессию престижи не совершались._\n");
        else
            foreach (var p in _prestigeLogs) report.AppendLine(p);

        report.AppendLine("## 3. Динамика по интервалам (Срезы)");
        report.AppendLine("| Время | Баланс | Всего очков | Сила клика | Автодоход | Авто/Клик | Кристаллы (тек/всего) | Кликов |");
        report.AppendLine("| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |");
        foreach (var snap in _minuteSnapshots) report.AppendLine(snap);
        report.AppendLine();

        report.AppendLine("## 4. Хронология событий");
        foreach (var ev in _timelineEvents) report.AppendLine(ev);

        File.WriteAllText(_reportFilePath, report.ToString(), Encoding.UTF8);
        Debug.Log($"[Playtest] Отчёт успешно сохранён в файл:\n{_reportFilePath}");
    }

    void OnApplicationQuit()
    {
        if (IsRecording) StopAndSaveReport();
    }

    private static string FormatTime(float totalSeconds)
    {
        int mins = Mathf.FloorToInt(totalSeconds / 60f);
        int secs = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
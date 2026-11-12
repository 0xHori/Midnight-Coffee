using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Менеджеры")]
    public SkillTreeManager skillTreeManager;
    public ClickerManager clickerManager;

    [Header("Контейнер и Префаб")]
    public Transform skillsContainer;
    public GameObject skillNodePrefab;

    [Header("Кнопки вкладок")]
    public Button clickTabButton;
    public Button autoTabButton;
    public Button luckTabButton;

    private SkillBranch currentBranch = SkillBranch.Click;
    private List<SkillNodeUI> spawnedNodes = new List<SkillNodeUI>();

    void Start()
    {
        if (clickTabButton != null) clickTabButton.onClick.AddListener(() => SelectBranch(SkillBranch.Click));
        if (autoTabButton != null) autoTabButton.onClick.AddListener(() => SelectBranch(SkillBranch.AutoIncome));
        if (luckTabButton != null) luckTabButton.onClick.AddListener(() => SelectBranch(SkillBranch.CritAndLuck));
    }

    void OnEnable()
    {
        GameEvents.OnUpdateSkillUI += RefreshCurrentTab;
        RebuildBranch();
    }

    void OnDisable()
    {
        GameEvents.OnUpdateSkillUI -= RefreshCurrentTab;
    }

    public void SelectBranch(SkillBranch branch)
    {
        currentBranch = branch;
        RebuildBranch();
    }

    public void RebuildBranch()
    {
        if (skillTreeManager == null || skillsContainer == null || skillNodePrefab == null) return;

        foreach (Transform child in skillsContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedNodes.Clear();

        var branchSkills = skillTreeManager.allSkills
            .Where(s => s != null && s.branch == currentBranch)
            .OrderBy(s => s.orderInBranch)
            .ToList();

        foreach (var skill in branchSkills)
        {
            GameObject nodeObj = Instantiate(skillNodePrefab, skillsContainer);
            SkillNodeUI nodeUI = nodeObj.GetComponent<SkillNodeUI>();
            if (nodeUI != null)
            {
                nodeUI.Setup(skill, skillTreeManager, clickerManager);
                spawnedNodes.Add(nodeUI);
            }
        }

        UpdateTabVisuals();
    }

    public void RefreshCurrentTab()
    {
        foreach (var node in spawnedNodes)
        {
            if (node != null) node.Refresh();
        }
    }

    private void UpdateTabVisuals()
    {
        if (clickTabButton != null) clickTabButton.interactable = (currentBranch != SkillBranch.Click);
        if (autoTabButton != null) autoTabButton.interactable = (currentBranch != SkillBranch.AutoIncome);
        if (luckTabButton != null) luckTabButton.interactable = (currentBranch != SkillBranch.CritAndLuck);
    }
}
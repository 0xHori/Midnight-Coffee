using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Менеджеры")]
    public ShopManager shopManager;
    public ClickerManager clickerManager;

    [Header("Список товаров")]
    public Transform shopContainer;
    public GameObject upgradeItemPrefab;

    [Header("Кнопка режима покупки")]
    public Button buyModeButton;
    public TextMeshProUGUI buyModeText;

    private List<UpgradeItemUI> spawnedItems = new List<UpgradeItemUI>();

    void Start()
    {
        if (buyModeButton != null)
        {
            buyModeButton.onClick.AddListener(() => shopManager.CycleBuyMode());
        }
        BuildShop();
    }

    void OnEnable()
    {
        GameEvents.OnUpdateUI += RefreshShop;
        RefreshShop();
    }

    void OnDisable()
    {
        GameEvents.OnUpdateUI -= RefreshShop;
    }

    public void BuildShop()
    {
        if (shopManager == null || shopContainer == null || upgradeItemPrefab == null) return;

        foreach (Transform child in shopContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        var sortedUpgrades = shopManager.allUpgrades
            .Where(u => u != null)
            .OrderBy(u => u.orderInShop)
            .ToList();

        foreach (var upgrade in sortedUpgrades)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, shopContainer);
            UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(upgrade, shopManager, clickerManager);
                spawnedItems.Add(itemUI);
            }
        }

        RefreshShop();
    }

    public void RefreshShop()
    {
        if (shopManager == null) return;

        if (buyModeText != null)
        {
            switch (shopManager.CurrentBuyMode)
            {
                case BuyMode.X1: buyModeText.text = "Покупка: x1"; break;
                case BuyMode.X10: buyModeText.text = "Покупка: x10"; break;
                case BuyMode.Max: buyModeText.text = "Покупка: MAX"; break;
            }
        }

        foreach (var item in spawnedItems)
        {
            if (item != null) item.Refresh();
        }
    }
}
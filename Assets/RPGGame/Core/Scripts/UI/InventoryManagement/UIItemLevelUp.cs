using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIItemLevelUp : UIItemWithMaterials
{
    // UI
    public Button levelUpButton;
    public AnimItemLevelUp animCharacterLevelUp;
    public AnimItemLevelUp animEquipmentLevelUp;
    // Events
    public UnityEvent eventLevelUpSuccess;
    public UnityEvent eventLevelUpFail;
    // Private
    private int totalLevelUpPrice;
    private PlayerItem newItem;
    private Dictionary<PlayerItem, int> materials;

    public override void Show()
    {
        base.Show();
        SetupLevelUp();
    }

    public void SetupLevelUp()
    {
        if (levelUpButton != null)
            levelUpButton.interactable = Item.CanLevelUp;

        materials = GetSelectedItemAmountPair(true);
        var levelUpPrice = Item.LevelUpPrice;
        var increasingExp = 0;
        totalLevelUpPrice = 0;
        foreach (var entry in materials)
        {
            increasingExp += entry.Value * entry.Key.RewardExp;
            totalLevelUpPrice += entry.Value * levelUpPrice;
        }

        newItem = Item.CreateLevelUpItem(increasingExp);

        if (uiAfterInfo != null)
            uiAfterInfo.SetData(newItem);

        if (uiCurrency != null)
        {
            var currencyData = PlayerCurrency.SoftCurrency.Clone().SetAmount(totalLevelUpPrice, 0);
            uiCurrency.SetData(currencyData);
        }
    }

    protected override List<PlayerItem> GetAvailableItemList()
    {
        if (!Item.IsReachMaxLevel)
        {
            if (Item.CharacterData != null)
            {
                var filterSetting = GameInstance.GameDatabase.characterLevelUpMaterialFilter;
                var list = PlayerItem.DataMap.Values.Where(a => !a.Id.Equals(Item.Id) && a.CanBeMaterial && UIItemListFilter.Filter(a, filterSetting)).ToList();
                list.SortRewardExp();
                return list;
            }
            if (Item.EquipmentData != null)
            {
                var filterSetting = GameInstance.GameDatabase.equipmentLevelUpMaterialFilter;
                var list = PlayerItem.DataMap.Values.Where(a => !a.Id.Equals(Item.Id) && a.CanBeMaterial && UIItemListFilter.Filter(a, filterSetting)).ToList();
                list.SortRewardExp();
                return list;
            }
        }
        return new List<PlayerItem>();
    }

    protected override void OnSetListItem(UIItem ui)
    {
        base.OnSetListItem(ui);
        ui.displayStats = UIItem.DisplayStats.RewardExp;
    }

    protected override void SelectItem(UIDataItem ui)
    {
        base.SelectItem(ui);
        SetupLevelUp();
    }

    protected override void DeselectItem(UIDataItem ui)
    {
        base.DeselectItem(ui);
        SetupLevelUp();
    }

    public void OnClickLevelUp()
    {
        var gameInstance = GameInstance.Singleton;
        var gameService = GameInstance.GameService;
        if (!PlayerCurrency.HaveEnoughSoftCurrency(totalLevelUpPrice))
        {
            gameInstance.WarnNotEnoughSoftCurrency();
            return;
        }
        var idAmountPair = GetSelectedItemIdAmountPair();
        gameService.LevelUpItem(Item.Id, idAmountPair, OnLevelUpSuccess, OnLevelUpFail);
    }

    private void OnLevelUpSuccess(ItemResult result)
    {
        if (animCharacterLevelUp != null && Item.CharacterData != null)
            animCharacterLevelUp.Play(Item, newItem, materials.Keys.ToList());

        if (animEquipmentLevelUp != null && Item.EquipmentData != null)
            animEquipmentLevelUp.Play(Item, newItem, materials.Keys.ToList());

        GameInstance.Singleton.OnGameServiceItemResult(result);
        eventLevelUpSuccess.Invoke();
        if (uiSelectedItemList != null)
            uiSelectedItemList.ClearListItems();
        var items = GetAvailableItems();
        var updateItems = result.updateItems;
        foreach (var updateItem in updateItems)
        {
            var id = updateItem.Id;
            if (updateItem.Id == Item.Id)
                Item = updateItem;
            if (items.ContainsKey(id))
                items[id].SetData(updateItem);
        }
        var deleteItemIds = result.deleteItemIds;
        foreach (var deleteItemId in deleteItemIds)
        {
            if (uiAvailableItemList != null)
                uiAvailableItemList.RemoveListItem(deleteItemId);
        }
        var updateCurrencies = result.updateCurrencies;
        foreach (var updateCurrency in updateCurrencies)
        {
            PlayerCurrency.SetData(updateCurrency);
        }
        SetupLevelUp();
    }

    private void OnLevelUpFail(string error)
    {
        GameInstance.Singleton.OnGameServiceError(error);
        eventLevelUpFail.Invoke();
    }

    protected override void OnUpdateItemAmount(PlayerItem item, int amount)
    {
        SetupLevelUp();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIItemEvolve : UIItemWithMaterials
{
    // Options
    public bool autoSelectMaterials;
    // UI
    public Button evolveButton;
    public AnimItemEvolve animCharacterEvolve;
    public AnimItemEvolve animEquipmentEvolve;
    // Events
    public UnityEvent eventEvolveSuccess;
    public UnityEvent eventEvolveFail;
    // Private
    private Dictionary<string, int> evolveMaterials = new Dictionary<string, int>();
    private PlayerItem newItem;
    private List<PlayerItem> materials;

    public override void Show()
    {
        base.Show();

        if (uiAvailableItemList != null)
            uiAvailableItemList.limitSelection = 0; // Always fix amount to 0, amount of evolve materials are varying

        SetupEvolve();
    }

    private void SetupEvolve()
    {
        if (evolveButton != null)
            evolveButton.interactable = Item.CanEvolve;
        
        if (Item.EvolveItem != null)
        {
            newItem = Item.CreateEvolveItem();

            if (uiAfterInfo != null)
                uiAfterInfo.SetData(newItem);

            materials = new List<PlayerItem>();
            evolveMaterials = Item.EvolveMaterials;
            foreach (var evolveItem in evolveMaterials)
            {
                var evolveItemDataId = evolveItem.Key;
                var evolveItemAmount = evolveItem.Value;
                var materialItem = new PlayerItem();
                materialItem.Id = evolveItemDataId;
                materialItem.DataId = evolveItemDataId;
                materialItem.Amount = 1;
                materials.Add(materialItem);
                if (uiSelectedItemList != null)
                {
                    var newUIMaterial = uiSelectedItemList.SetListItem(materialItem);
                    newUIMaterial.ForceUpdate();
                    newUIMaterial.SetupSelectedAmount(0, evolveItemAmount);
                }
            }

            if (autoSelectMaterials && uiAvailableItemList != null)
            {
                foreach (var evolveItem in evolveMaterials)
                {
                    var evolveItemDataId = evolveItem.Key;
                    var evolveItemAmount = evolveItem.Value;
                    var selectedAmount = 0;
                    var selectingUIs = uiAvailableItemList.UIEntries.Values.Where(a => a.data.DataId == evolveItemDataId).ToList();
                    foreach (var selectingUI in selectingUIs)
                    {
                        selectingUI.Select();
                        selectedAmount += selectingUI.data.Amount;
                        if (selectedAmount >= evolveItemAmount)
                            break;
                    }
                }
            }

            if (uiCurrency != null)
            {
                var currencyData = PlayerCurrency.SoftCurrency.Clone().SetAmount(Item.EvolvePrice, 0);
                uiCurrency.SetData(currencyData);
            }
        }
    }

    protected override List<PlayerItem> GetAvailableItemList()
    {
        if (Item.CanEvolve)
        {
            var evolveMaterialDataIds = Item.EvolveMaterials.Keys.ToList();
            var list = PlayerItem.DataMap.Values.Where(a => !a.Id.Equals(Item.Id) && evolveMaterialDataIds.Contains(a.DataId)).ToList();
            list.SortLevel();
            return list;
        }
        return new List<PlayerItem>();
    }

    protected override void SelectItem(UIDataItem ui)
    {
        if (uiSelectedItemList == null || !uiSelectedItemList.UIEntries.ContainsKey((ui as UIItem).data.DataId))
            return;
        UpdateSelectMaterialAmount((ui as UIItem).data);
    }

    protected override void DeselectItem(UIDataItem ui)
    {
        UpdateSelectMaterialAmount((ui as UIItem).data);
    }

    private void UpdateSelectMaterialAmount(PlayerItem item)
    {
        var dataId = item.DataId;
        var list = uiAvailableItemList.GetSelectedUIList(dataId);
        var selectedAmount = 0;
        foreach (var entry in list)
        {
            selectedAmount += entry.SelectedAmount;
        }
        var material = uiSelectedItemList.UIEntries[dataId];
        material.SelectedAmount = selectedAmount;
        material.RequiredAmount = evolveMaterials[dataId];
    }

    public void OnClickEvolve()
    {
        var gameInstance = GameInstance.Singleton;
        var gameService = GameInstance.GameService;
        if (!PlayerCurrency.HaveEnoughSoftCurrency(Item.EvolvePrice))
        {
            gameInstance.WarnNotEnoughSoftCurrency();
            return;
        }
        var idAmountPair = GetSelectedItemIdAmountPair();
        gameService.EvolveItem(Item.Id, idAmountPair, OnEvolveSuccess, OnEvolveFail);
    }

    private void OnEvolveSuccess(ItemResult result)
    {
        if (animCharacterEvolve != null && Item.CharacterData != null)
            animCharacterEvolve.Play(Item, newItem, materials);

        if (animEquipmentEvolve != null && Item.EquipmentData != null)
            animEquipmentEvolve.Play(Item, newItem, materials);

        GameInstance.Singleton.OnGameServiceItemResult(result);
        eventEvolveSuccess.Invoke();
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
        SetupEvolve();
    }

    private void OnEvolveFail(string error)
    {
        GameInstance.Singleton.OnGameServiceError(error);
        eventEvolveFail.Invoke();
    }
}

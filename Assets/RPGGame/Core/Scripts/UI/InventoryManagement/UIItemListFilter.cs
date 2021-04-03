[System.Serializable]
public struct UIItemListFilterSetting
{
    public bool showCharacter;
    public bool showEquipment;
    public bool showMaterial;
    public string category;
}

public static class UIItemListFilter
{
    public static bool Filter(PlayerItem item, UIItemListFilterSetting setting)
    {
        if (!setting.showCharacter &&
            !setting.showEquipment &&
            !setting.showMaterial &&
            string.IsNullOrEmpty(setting.category))
        {
            // No setting, No filter, Show all
            return true;
        }
        if ((setting.showMaterial && IsMaterial(item)) ||
            (setting.showCharacter && IsCharacter(item)) ||
            (setting.showEquipment && IsEquipment(item)))
        {
            // Filter by item types and category
            return string.IsNullOrEmpty(setting.category) || MatchCategory(item, setting.category);
        }
        return false;
    }

    public static bool IsCharacter(PlayerItem item)
    {
        return item != null && item.CharacterData != null;
    }

    public static bool IsEquipment(PlayerItem item)
    {
        return item != null && item.EquipmentData != null;
    }

    public static bool IsMaterial(PlayerItem item)
    {
        return item != null && item.MaterialData != null && !IsCharacter(item) && !IsEquipment(item);
    }

    public static bool MatchCategory(PlayerItem item, string category)
    {
        return item != null && item.ItemData != null && item.ItemData.category == category;
    }
}
